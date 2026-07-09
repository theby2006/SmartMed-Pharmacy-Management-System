using System.Drawing;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Diagnostics;
using SmartMed.Models.Session;
using SmartMed.UI.Services;
using SmartMed.UI.Theme;
using SmartMed.UI.Theme.Controls;

namespace SmartMed.UI.Forms
{
    /// <summary>
    /// The customer application shell: same sidebar/top-bar pattern as
    /// <see cref="MainShellForm"/>, scoped to the customer-facing screens
    /// (browse, cart, orders) and backed by a single <see cref="CartService"/>
    /// instance shared across this session.
    /// </summary>
    public class CustomerShellForm : Form
    {
        private readonly ISessionManager _sessionManager;
        private readonly IAuthenticationService _authService;
        private readonly IMedicineService _medicineService;
        private readonly IMedicineCategoryService _medicineCategoryService;
        private readonly IMedicineSearchService _medicineSearchService;
        private readonly IOrderService _orderService;
        private readonly IPrescriptionService _prescriptionService;
        private readonly IReportService _reportService;
        private readonly CartService _cartService;

        private SidebarNavControl _sidebar;
        private Panel _contentHost;
        private Label _pageTitleLabel;
        private Label _cartBadge;
        private Form _activeChild;

        public CustomerShellForm(
            ApplicationStartupContext startupContext,
            ISessionManager sessionManager,
            IAuthenticationService authService,
            IMedicineService medicineService,
            IMedicineCategoryService medicineCategoryService,
            IMedicineSearchService medicineSearchService,
            IOrderService orderService,
            IPrescriptionService prescriptionService,
            IPricingService pricingService,
            IReportService reportService)
        {
            _sessionManager = sessionManager;
            _authService = authService;
            _medicineService = medicineService;
            _medicineCategoryService = medicineCategoryService;
            _medicineSearchService = medicineSearchService;
            _orderService = orderService;
            _prescriptionService = prescriptionService;
            _reportService = reportService;
            _cartService = new CartService(pricingService);

            Text = startupContext.ApplicationName + " — Customer Portal";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 600);
            Width = 1200;
            Height = 760;
            WindowState = FormWindowState.Maximized;
            Icon = IconFactory.BuildAppIcon();
            ShowIcon = true;
            BackColor = AppTheme.Background;

            BuildLayout();
            _cartService.CartChanged += (s, e) => UpdateCartBadge();
            OpenBrowse();

            FormClosing += CustomerShellForm_FormClosing;
        }

        private void BuildLayout()
        {
            _sidebar = new SidebarNavControl();
            SessionContext session = _sessionManager.CurrentSession;
            _sidebar.SetUserInfo(session?.DisplayName ?? "", "Customer");

            _sidebar.AddItem("browse", IconFactory.Medicine, "Browse Medicines");
            _sidebar.AddItem("cart", IconFactory.Cart, "My Cart");
            _sidebar.AddItem("orders", IconFactory.Orders, "My Orders");

            _sidebar.ItemClicked += Sidebar_ItemClicked;
            _sidebar.LogoutClicked += (s, e) => Logout();
            Controls.Add(_sidebar);

            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = AppTheme.Surface };
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
                Text = "Browse Medicines"
            };
            topBar.Controls.Add(_pageTitleLabel);

            _cartBadge = new Label
            {
                AutoSize = false,
                Size = new Size(20, 20),
                BackColor = AppTheme.Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Visible = false
            };
            topBar.Controls.Add(_cartBadge);
            topBar.Resize += (s, e) => _cartBadge.Location = new Point(topBar.Width - 48, 18);
            _cartBadge.Location = new Point(topBar.Width - 48, 18);

            Controls.Add(topBar);

            _contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Background,
                Padding = new Padding(AppTheme.PagePadding)
            };
            Controls.Add(_contentHost);

            topBar.BringToFront();
        }

        private void UpdateCartBadge()
        {
            int count = _cartService.ItemCount;
            _cartBadge.Visible = count > 0;
            _cartBadge.Text = count > 99 ? "99+" : count.ToString();
        }

        private void Sidebar_ItemClicked(object sender, SidebarItemClickedEventArgs e)
        {
            _sidebar.SetActive(e.Key);
            switch (e.Key)
            {
                case "browse":
                    OpenBrowse();
                    break;
                case "cart":
                    _pageTitleLabel.Text = "My Cart";
                    OpenChild(new CartAndCheckoutForm(_cartService, _orderService, _prescriptionService, _sessionManager));
                    break;
                case "orders":
                    _pageTitleLabel.Text = "My Orders";
                    OpenChild(new MyOrdersForm(_orderService, _sessionManager, _reportService));
                    break;
            }
        }

        private void OpenBrowse()
        {
            _sidebar.SetActive("browse");
            _pageTitleLabel.Text = "Browse Medicines";
            OpenChild(new BrowseMedicinesForm(_medicineService, _medicineCategoryService, _medicineSearchService, _cartService));
        }

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

        private void Logout()
        {
            _authService.Logout();
            Close();
        }

        private void CustomerShellForm_FormClosing(object sender, FormClosingEventArgs e)
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
