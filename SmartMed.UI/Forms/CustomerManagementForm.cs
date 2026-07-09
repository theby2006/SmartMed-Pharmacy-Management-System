using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;
using SmartMed.UI.Theme;
using SmartMed.UI.Theme.Controls;

namespace SmartMed.UI.Forms
{
    /// <summary>
    /// Administrator screen for viewing/updating customer accounts and
    /// browsing a customer's order history.
    /// </summary>
    public class CustomerManagementForm : Form
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;

        private ModernTextBox txtSearch;
        private RoundedButton btnSearch;
        private DataGridView dgvCustomers;
        private CardPanel detailCard;
        private ModernTextBox txtFullName;
        private ModernTextBox txtPhone;
        private ModernTextBox txtEmail;
        private RoundedButton btnSave;
        private RoundedButton btnToggleActive;
        private DataGridView dgvOrders;
        private Label lblToast;

        private List<Customer> _customers = new List<Customer>();
        private Customer _selectedCustomer;

        public CustomerManagementForm(ICustomerService customerService, IOrderService orderService)
        {
            _customerService = customerService;
            _orderService = orderService;
            InitializeComponents();
            LoadCustomers();
        }

        private void InitializeComponents()
        {
            Text = "Customers";
            BackColor = AppTheme.Background;

            Label title = new Label { AutoSize = true, Font = AppTheme.PageTitle, ForeColor = AppTheme.TextPrimary, Location = new Point(0, 0), Text = "Customers" };
            Controls.Add(title);

            txtSearch = new ModernTextBox { Location = new Point(0, 48), Width = 280, PlaceholderText = "Search by name, phone, or email", LeadingIcon = IconFactory.Search };
            Controls.Add(txtSearch);

            btnSearch = new RoundedButton { Variant = ButtonVariant.Primary, Text = "Search", Location = new Point(292, 48), Width = 100 };
            btnSearch.Click += (s, e) => ApplySearch();
            Controls.Add(btnSearch);

            dgvCustomers = new DataGridView { Location = new Point(0, 96), Size = new Size(560, 420), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom };
            DataGridViewStyler.Apply(dgvCustomers);
            dgvCustomers.SelectionChanged += (s, e) => ShowSelectedCustomer();
            Controls.Add(dgvCustomers);

            detailCard = new CardPanel { Location = new Point(584, 96), Size = new Size(400, 420), Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom };
            Controls.Add(detailCard);

            int y = 20;
            Label detailTitle = new Label { AutoSize = true, Font = AppTheme.SectionHeader, ForeColor = AppTheme.TextPrimary, Location = new Point(20, y), Text = "Customer Details" };
            detailCard.Controls.Add(detailTitle);
            y += 40;

            txtFullName = AddDetailField(detailCard, "Full name", ref y);
            txtPhone = AddDetailField(detailCard, "Phone number", ref y);
            txtEmail = AddDetailField(detailCard, "Email", ref y);

            btnSave = new RoundedButton { Variant = ButtonVariant.Primary, Text = "Save Changes", Location = new Point(20, y), Width = 170 };
            btnSave.Click += BtnSave_Click;
            detailCard.Controls.Add(btnSave);

            btnToggleActive = new RoundedButton { Variant = ButtonVariant.Danger, Text = "Deactivate", Location = new Point(200, y), Width = 160 };
            btnToggleActive.Click += BtnToggleActive_Click;
            detailCard.Controls.Add(btnToggleActive);
            y += AppTheme.ControlHeight + AppTheme.Space4;

            Label ordersLabel = new Label { AutoSize = true, Font = AppTheme.CaptionBold, ForeColor = AppTheme.TextSecondary, Location = new Point(20, y), Text = "ORDER HISTORY" };
            detailCard.Controls.Add(ordersLabel);
            y += 24;

            dgvOrders = new DataGridView { Location = new Point(20, y), Size = new Size(360, 160), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
            DataGridViewStyler.Apply(dgvOrders);
            detailCard.Controls.Add(dgvOrders);

            lblToast = new Label { AutoSize = true, Font = AppTheme.Caption, ForeColor = AppTheme.Success, Location = new Point(400, 48), Visible = false };
            Controls.Add(lblToast);

            SetDetailEnabled(false);
        }

        private ModernTextBox AddDetailField(Control parent, string placeholder, ref int y)
        {
            ModernTextBox field = new ModernTextBox { Location = new Point(20, y), Width = 360, PlaceholderText = placeholder };
            parent.Controls.Add(field);
            y += field.Height + AppTheme.Space2;
            return field;
        }

        private void SetDetailEnabled(bool enabled)
        {
            txtFullName.Enabled = enabled;
            txtPhone.Enabled = enabled;
            txtEmail.Enabled = enabled;
            btnSave.Enabled = enabled;
            btnToggleActive.Enabled = enabled;
        }

        private void LoadCustomers()
        {
            OperationResult<List<Customer>> result = _customerService.GetAllCustomers();
            _customers = result.IsSuccess ? result.Data : new List<Customer>();
            BindCustomers();
        }

        private void ApplySearch()
        {
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                LoadCustomers();
                return;
            }

            OperationResult<List<Customer>> result = _customerService.SearchCustomers(keyword);
            _customers = result.IsSuccess ? result.Data : new List<Customer>();
            BindCustomers();
        }

        private void BindCustomers()
        {
            var rows = _customers.Select(c => new
            {
                c.FullName,
                c.PhoneNumber,
                Email = c.Email ?? "-",
                Status = c.IsActive ? "Active" : "Inactive"
            }).ToList();
            dgvCustomers.DataSource = rows;
        }

        private void ShowSelectedCustomer()
        {
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                SetDetailEnabled(false);
                return;
            }

            int rowIndex = dgvCustomers.SelectedRows[0].Index;
            if (rowIndex < 0 || rowIndex >= _customers.Count) return;

            _selectedCustomer = _customers[rowIndex];
            txtFullName.Text = _selectedCustomer.FullName;
            txtPhone.Text = _selectedCustomer.PhoneNumber;
            txtEmail.Text = _selectedCustomer.Email ?? "";
            btnToggleActive.Text = _selectedCustomer.IsActive ? "Deactivate" : "Activate";
            SetDetailEnabled(true);

            OperationResult<List<Order>> ordersResult = _orderService.GetOrdersByCustomer(_selectedCustomer.Id);
            var orderRows = (ordersResult.IsSuccess ? ordersResult.Data : new List<Order>())
                .Select(o => new { o.OrderNumber, Date = o.OrderDate.ToString("yyyy-MM-dd"), Total = o.GrandTotal.ToString("C2"), Status = o.Status.ToString() })
                .ToList();
            dgvOrders.DataSource = orderRows;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_selectedCustomer == null) return;

            _selectedCustomer.FullName = txtFullName.Text.Trim();
            _selectedCustomer.PhoneNumber = txtPhone.Text.Trim();
            _selectedCustomer.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();

            OperationResult result = _customerService.UpdateProfile(_selectedCustomer);
            ShowResult(result);
            if (result.IsSuccess) LoadCustomers();
        }

        private void BtnToggleActive_Click(object sender, EventArgs e)
        {
            if (_selectedCustomer == null) return;

            if (_selectedCustomer.IsActive)
            {
                OperationResult result = _customerService.DeactivateCustomer(_selectedCustomer.Id);
                ShowResult(result);
                if (result.IsSuccess) LoadCustomers();
            }
        }

        private void ShowResult(OperationResult result)
        {
            lblToast.ForeColor = result.IsSuccess ? AppTheme.Success : AppTheme.Danger;
            lblToast.Text = result.Message;
            lblToast.Visible = true;
        }
    }
}
