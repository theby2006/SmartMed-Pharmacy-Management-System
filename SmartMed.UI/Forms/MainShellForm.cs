using System;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Diagnostics;
using SmartMed.Models.Enums;
using SmartMed.Models.Session;
using SmartMed.UI.Theme;
using SmartMed.UI.Theme.Controls;

namespace SmartMed.UI.Forms
{
    /// <summary>
    /// The staff application shell: a themed sidebar for navigation, a top
    /// bar showing the active page title and alert count, and a content area
    /// that either embeds a restyled form inline or opens a legacy dialog for
    /// forms not yet restyled in this pass.
    /// </summary>
    public class MainShellForm : Form
    {
        private readonly ISessionManager _sessionManager;
        private readonly IAuthenticationService _authService;
        private readonly ISalesService _salesService;
        private readonly IPaymentService _paymentService;
        private readonly IPricingService _pricingService;
        private readonly IMedicineService _medicineService;
        private readonly IMedicineCategoryService _medicineCategoryService;
        private readonly ISupplierService _supplierService;
        private readonly IPurchaseService _purchaseService;
        private readonly IInventoryService _inventoryService;
        private readonly ISaleNumberGenerator _saleNumberGenerator;
        private readonly IReportService _reportService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;

        private SidebarNavControl _sidebar;
        private Panel _contentHost;
        private Label _pageTitleLabel;
        private PictureBox _bellIcon;
        private Label _bellBadge;
        private Form _activeChild;

        public MainShellForm(
            ApplicationStartupContext startupContext,
            ISessionManager sessionManager,
            IAuthenticationService authService,
            ISalesService salesService,
            IPaymentService paymentService,
            IPricingService pricingService,
            IMedicineService medicineService,
            IMedicineCategoryService medicineCategoryService,
            ISupplierService supplierService,
            IPurchaseService purchaseService,
            IInventoryService inventoryService,
            ISaleNumberGenerator saleNumberGenerator,
            IReportService reportService,
            ICustomerService customerService,
            IOrderService orderService)
        {
            _sessionManager = sessionManager;
            _authService = authService;
            _salesService = salesService;
            _paymentService = paymentService;
            _pricingService = pricingService;
            _medicineService = medicineService;
            _medicineCategoryService = medicineCategoryService;
            _supplierService = supplierService;
            _purchaseService = purchaseService;
            _inventoryService = inventoryService;
            _saleNumberGenerator = saleNumberGenerator;
            _reportService = reportService;
            _customerService = customerService;
            _orderService = orderService;

            Text = startupContext.ApplicationName;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1100, 640);
            Width = 1280;
            Height = 800;
            WindowState = FormWindowState.Maximized;
            Icon = IconFactory.BuildAppIcon();
            ShowIcon = true;
            BackColor = AppTheme.Background;

            BuildLayout();
            UpdateMenuVisibility();
            OpenDashboard();

            FormClosing += MainShellForm_FormClosing;
        }

        private void BuildLayout()
        {
            _sidebar = new SidebarNavControl();
            SessionContext session = _sessionManager.CurrentSession;
            _sidebar.SetUserInfo(session?.DisplayName ?? "", session?.Role.ToString() ?? "");

            _sidebar.AddItem("dashboard", IconFactory.Dashboard, "Dashboard");
            _sidebar.AddSection("Inventory");
            _sidebar.AddItem("medicines", IconFactory.Medicine, "Medicines");
            _sidebar.AddItem("categories", IconFactory.Category, "Categories");
            _sidebar.AddSection("Procurement");
            _sidebar.AddItem("suppliers", IconFactory.Suppliers, "Suppliers");
            _sidebar.AddItem("purchases", IconFactory.Purchases, "Purchases");
            _sidebar.AddSection("Sales");
            _sidebar.AddItem("sales", IconFactory.Sales, "New Sale");
            _sidebar.AddSection("Orders");
            _sidebar.AddItem("orders", IconFactory.Orders, "Manage Orders");
            _sidebar.AddSection("People");
            _sidebar.AddItem("customers", IconFactory.Customers, "Customers");
            _sidebar.AddSection("Insights");
            _sidebar.AddItem("reports", IconFactory.Reports, "Reports");

            _sidebar.ItemClicked += Sidebar_ItemClicked;
            _sidebar.LogoutClicked += (s, e) => Logout();
            Controls.Add(_sidebar);

            Panel topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = AppTheme.Surface
            };
            topBar.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(AppTheme.Border))
                    e.Graphics.DrawLine(pen, 0, topBar.Height - 1, topBar.Width, topBar.Height - 1);
            };

            _pageTitleLabel = new Label
            {
                AutoSize = true,
                Font = AppTheme.SectionHeader,
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(AppTheme.PagePadding, 18),
                Text = "Dashboard"
            };
            topBar.Controls.Add(_pageTitleLabel);

            _bellIcon = new PictureBox
            {
                Image = IconFactory.GetGlyph(IconFactory.Bell, 20, AppTheme.TextSecondary),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Size = new Size(24, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            topBar.Controls.Add(_bellIcon);

            _bellBadge = new Label
            {
                AutoSize = false,
                Size = new Size(16, 16),
                BackColor = AppTheme.Danger,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 6.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Visible = false
            };
            topBar.Controls.Add(_bellBadge);
            topBar.Resize += (s, e) => PositionTopBarWidgets(topBar);
            PositionTopBarWidgets(topBar);

            Controls.Add(topBar);

            _contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Background,
                Padding = new Padding(AppTheme.PagePadding)
            };
            Controls.Add(_contentHost);

            topBar.BringToFront();
            _contentHost.SendToBack();
        }

        private void PositionTopBarWidgets(Panel topBar)
        {
            _bellIcon.Location = new Point(topBar.Width - 48, 20);
            _bellBadge.Location = new Point(topBar.Width - 36, 14);
        }

        private void UpdateMenuVisibility()
        {
            SessionContext session = _sessionManager.CurrentSession;
            bool isAdmin = session != null && session.Role == RoleType.Administrator;
            bool isPharmacist = session != null && session.Role == RoleType.Pharmacist;
            bool isCashier = session != null && session.Role == RoleType.Cashier;

            _sidebar.SetItemVisible("dashboard", isAdmin);
            _sidebar.SetItemVisible("medicines", isAdmin || isPharmacist);
            _sidebar.SetItemVisible("categories", isAdmin || isPharmacist);
            _sidebar.SetItemVisible("suppliers", isAdmin);
            _sidebar.SetItemVisible("purchases", isAdmin);
            _sidebar.SetItemVisible("sales", isAdmin || isCashier);
            _sidebar.SetItemVisible("orders", isAdmin || isPharmacist);
            _sidebar.SetItemVisible("customers", isAdmin);
            _sidebar.SetItemVisible("reports", isAdmin);
        }

        private void UpdateLowStockBadge()
        {
            try
            {
                var summaryResult = _reportService.GetDashboardSummary();
                if (summaryResult.IsSuccess)
                {
                    int count = summaryResult.Data.LowStockCount + summaryResult.Data.NearExpiryCount;
                    _bellBadge.Visible = count > 0;
                    _bellBadge.Text = count > 99 ? "99+" : count.ToString();
                }
            }
            catch
            {
                _bellBadge.Visible = false;
            }
        }

        private void Sidebar_ItemClicked(object sender, SidebarItemClickedEventArgs e)
        {
            _sidebar.SetActive(e.Key);

            switch (e.Key)
            {
                case "dashboard":
                    OpenDashboard();
                    break;
                case "medicines":
                    SetPageTitle("Medicines");
                    OpenChild(new MedicineForm(_medicineService, _medicineCategoryService));
                    break;
                case "categories":
                    SetPageTitle("Categories");
                    ShowLegacyDialog(new MedicineCategoryForm(_medicineCategoryService));
                    break;
                case "suppliers":
                    SetPageTitle("Suppliers");
                    OpenChild(new SupplierForm(_supplierService));
                    break;
                case "purchases":
                    SetPageTitle("Purchases");
                    ShowLegacyDialog(new PurchaseForm(_purchaseService, _medicineService, _supplierService));
                    break;
                case "sales":
                    SetPageTitle("New Sale");
                    ShowLegacyDialog(new SalesForm(_salesService, _paymentService, _pricingService, _medicineService, _inventoryService, _sessionManager, _saleNumberGenerator));
                    break;
                case "orders":
                    SetPageTitle("Manage Orders");
                    OpenChild(new OrderManagementForm(_orderService, _customerService));
                    break;
                case "customers":
                    SetPageTitle("Customers");
                    OpenChild(new CustomerManagementForm(_customerService, _orderService));
                    break;
                case "reports":
                    SetPageTitle("Reports");
                    ShowLegacyDialog(new ReportsForm(_reportService));
                    break;
            }
        }

        private void OpenDashboard()
        {
            _sidebar.SetActive("dashboard");
            SetPageTitle("Dashboard");
            OpenChild(new DashboardForm(_reportService));
            UpdateLowStockBadge();
        }

        private void SetPageTitle(string title)
        {
            _pageTitleLabel.Text = title;
        }

        /// <summary>
        /// Embeds a restyled form directly into the content area (non-modal,
        /// filling the available space) and disposes whatever was hosted
        /// there previously.
        /// </summary>
        private void OpenChild(Form child)
        {
            if (_activeChild != null)
            {
                _contentHost.Controls.Remove(_activeChild);
                _activeChild.Dispose();
                _activeChild = null;
            }

            child.TopLevel = false;
            child.FormBorderStyle = FormBorderStyle.None;
            child.Dock = DockStyle.Fill;
            _contentHost.Controls.Add(child);
            child.Show();
            _activeChild = child;
        }

        /// <summary>
        /// Opens a form not yet restyled to the theme system as a modal
        /// dialog, exactly as the previous MenuStrip-based shell did, so its
        /// existing fixed-size layout keeps working unchanged.
        /// </summary>
        private void ShowLegacyDialog(Form dialog)
        {
            using (dialog)
            {
                dialog.ShowDialog(this);
            }

            UpdateLowStockBadge();
        }

        private void Logout()
        {
            _authService.Logout();
            Close();
        }

        private void MainShellForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_authService.IsAuthenticated)
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to exit?",
                    "Confirm Exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
