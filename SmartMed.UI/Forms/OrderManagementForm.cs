using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;
using SmartMed.UI.Theme;
using SmartMed.UI.Theme.Controls;

namespace SmartMed.UI.Forms
{
    /// <summary>
    /// Administrator/pharmacist screen for advancing orders through their
    /// lifecycle (Pending → Approved → Ready for Pickup → Delivered, or
    /// Cancelled/Rejected). Stock commit/reversal logic already lives in
    /// <see cref="IOrderService"/> — this form only calls it.
    /// </summary>
    public class OrderManagementForm : Form
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;

        private FlowLayoutPanel filterPillsPanel;
        private DataGridView dgvOrders;
        private CardPanel detailCard;
        private Label lblOrderNumber;
        private StatusBadge statusBadge;
        private Label lblCustomerInfo;
        private DataGridView dgvItems;
        private RoundedButton btnViewPrescription;
        private RoundedButton btnAdvance;
        private RoundedButton btnReject;
        private RoundedButton btnCancel;
        private Label lblToast;

        private OrderStatus? _activeFilter;
        private List<Order> _orders = new List<Order>();
        private Order _selectedOrder;

        public OrderManagementForm(IOrderService orderService, ICustomerService customerService)
        {
            _orderService = orderService;
            _customerService = customerService;
            InitializeComponents();
            LoadOrders();
        }

        private void InitializeComponents()
        {
            Text = "Manage Orders";
            BackColor = AppTheme.Background;

            Label title = new Label { AutoSize = true, Font = AppTheme.PageTitle, ForeColor = AppTheme.TextPrimary, Location = new Point(0, 0), Text = "Manage Orders" };
            Controls.Add(title);

            filterPillsPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 48),
                Size = new Size(900, 40),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            Controls.Add(filterPillsPanel);
            BuildFilterPills();

            dgvOrders = new DataGridView { Location = new Point(0, 96), Size = new Size(560, 420), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom };
            DataGridViewStyler.Apply(dgvOrders);
            dgvOrders.SelectionChanged += (s, e) => ShowSelectedOrder();
            Controls.Add(dgvOrders);

            detailCard = new CardPanel { Location = new Point(584, 96), Size = new Size(400, 420), Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom };
            Controls.Add(detailCard);

            lblOrderNumber = new Label { AutoSize = true, Font = AppTheme.SectionHeader, ForeColor = AppTheme.TextPrimary, Location = new Point(20, 20), Text = "Select an order" };
            detailCard.Controls.Add(lblOrderNumber);

            statusBadge = new StatusBadge { Location = new Point(20, 52) };
            detailCard.Controls.Add(statusBadge);

            lblCustomerInfo = new Label { AutoSize = true, Font = AppTheme.Body, ForeColor = AppTheme.TextSecondary, Location = new Point(20, 84), Text = "" };
            detailCard.Controls.Add(lblCustomerInfo);

            dgvItems = new DataGridView { Location = new Point(20, 112), Size = new Size(360, 200), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            DataGridViewStyler.Apply(dgvItems);
            detailCard.Controls.Add(dgvItems);

            btnViewPrescription = new RoundedButton { Variant = ButtonVariant.Outline, Text = "View Prescription", IconGlyph = IconFactory.Upload, Location = new Point(20, 320), Width = 360, Visible = false };
            btnViewPrescription.Click += BtnViewPrescription_Click;
            detailCard.Controls.Add(btnViewPrescription);

            btnAdvance = new RoundedButton { Variant = ButtonVariant.Primary, Text = "Advance", Location = new Point(20, 364), Width = 360, Height = AppTheme.ControlHeight };
            btnAdvance.Click += BtnAdvance_Click;
            detailCard.Controls.Add(btnAdvance);

            btnReject = new RoundedButton { Variant = ButtonVariant.Outline, Text = "Reject", Location = new Point(20, 412), Width = 175 };
            btnReject.Click += (s, e) => ChangeStatus(OrderStatus.Rejected);
            detailCard.Controls.Add(btnReject);

            btnCancel = new RoundedButton { Variant = ButtonVariant.Danger, Text = "Cancel", Location = new Point(205, 412), Width = 175 };
            btnCancel.Click += (s, e) => ChangeStatus(OrderStatus.Cancelled);
            detailCard.Controls.Add(btnCancel);

            lblToast = new Label { AutoSize = true, Font = AppTheme.Caption, ForeColor = AppTheme.Success, Location = new Point(410, 52), Visible = false };
            Controls.Add(lblToast);

            SetDetailVisible(false);
        }

        private void BuildFilterPills()
        {
            filterPillsPanel.Controls.Clear();
            AddPill("All", null);
            AddPill("Pending", OrderStatus.Pending);
            AddPill("Approved", OrderStatus.Approved);
            AddPill("Ready for Pickup", OrderStatus.Processing);
            AddPill("Delivered", OrderStatus.Completed);
            AddPill("Cancelled", OrderStatus.Cancelled);
        }

        private void AddPill(string label, OrderStatus? status)
        {
            RoundedButton pill = new RoundedButton
            {
                Text = label,
                Variant = _activeFilter == status ? ButtonVariant.Primary : ButtonVariant.Outline,
                Width = 140,
                Height = 32,
                Margin = new Padding(0, 0, 8, 0)
            };
            pill.Click += (s, e) => { _activeFilter = status; BuildFilterPills(); LoadOrders(); };
            filterPillsPanel.Controls.Add(pill);
        }

        private void LoadOrders()
        {
            OperationResult<List<Order>> result = _activeFilter.HasValue
                ? _orderService.GetOrdersByStatus(_activeFilter.Value)
                : _orderService.GetAllOrders();

            _orders = result.IsSuccess ? result.Data : new List<Order>();

            var rows = _orders.Select(o => new
            {
                o.OrderNumber,
                Date = o.OrderDate.ToString("yyyy-MM-dd"),
                Total = o.GrandTotal.ToString("C2"),
                Status = o.Status.ToString()
            }).ToList();
            dgvOrders.DataSource = rows;
        }

        private void ShowSelectedOrder()
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                SetDetailVisible(false);
                return;
            }

            int rowIndex = dgvOrders.SelectedRows[0].Index;
            if (rowIndex < 0 || rowIndex >= _orders.Count) return;

            OperationResult<Order> detailResult = _orderService.GetOrderById(_orders[rowIndex].Id);
            if (!detailResult.IsSuccess) return;

            _selectedOrder = detailResult.Data;
            SetDetailVisible(true);

            lblOrderNumber.Text = _selectedOrder.OrderNumber;
            statusBadge.SetStatus(_selectedOrder.Status);

            OperationResult<Customer> customerResult = _customerService.GetCustomerById(_selectedOrder.CustomerId);
            lblCustomerInfo.Text = customerResult.IsSuccess
                ? $"{customerResult.Data.FullName} · {customerResult.Data.PhoneNumber}"
                : $"Customer #{_selectedOrder.CustomerId}";

            var items = _selectedOrder.Items.Select(i => new { i.MedicineId, i.Quantity, Total = i.LineTotal.ToString("C2") }).ToList();
            dgvItems.DataSource = items;

            btnViewPrescription.Visible = !string.IsNullOrEmpty(_selectedOrder.PrescriptionFilePath);

            UpdateActionButtons();
        }

        private void UpdateActionButtons()
        {
            switch (_selectedOrder.Status)
            {
                case OrderStatus.Pending:
                    btnAdvance.Text = "Approve Order";
                    btnAdvance.Visible = true;
                    btnReject.Visible = true;
                    btnCancel.Visible = true;
                    break;
                case OrderStatus.Approved:
                    btnAdvance.Text = "Mark Ready for Pickup";
                    btnAdvance.Visible = true;
                    btnReject.Visible = false;
                    btnCancel.Visible = true;
                    break;
                case OrderStatus.Processing:
                    btnAdvance.Text = "Mark Delivered";
                    btnAdvance.Visible = true;
                    btnReject.Visible = false;
                    btnCancel.Visible = true;
                    break;
                default:
                    btnAdvance.Visible = false;
                    btnReject.Visible = false;
                    btnCancel.Visible = false;
                    break;
            }
        }

        private void BtnAdvance_Click(object sender, EventArgs e)
        {
            OrderStatus next = _selectedOrder.Status switch
            {
                OrderStatus.Pending => OrderStatus.Approved,
                OrderStatus.Approved => OrderStatus.Processing,
                OrderStatus.Processing => OrderStatus.Completed,
                _ => _selectedOrder.Status
            };
            ChangeStatus(next);
        }

        private void ChangeStatus(OrderStatus next)
        {
            if (_selectedOrder == null) return;

            OperationResult result = _orderService.UpdateOrderStatus(_selectedOrder.Id, next);
            lblToast.ForeColor = result.IsSuccess ? AppTheme.Success : AppTheme.Danger;
            lblToast.Text = result.Message;
            lblToast.Visible = true;

            if (result.IsSuccess)
            {
                LoadOrders();
                ShowSelectedOrder();
            }
        }

        private void BtnViewPrescription_Click(object sender, EventArgs e)
        {
            if (_selectedOrder == null || string.IsNullOrEmpty(_selectedOrder.PrescriptionFilePath)) return;

            string fullPath = Path.IsPathRooted(_selectedOrder.PrescriptionFilePath)
                ? _selectedOrder.PrescriptionFilePath
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _selectedOrder.PrescriptionFilePath);

            if (File.Exists(fullPath))
            {
                Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
            }
            else
            {
                MessageBox.Show("Prescription file could not be found.", "File Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetDetailVisible(bool visible)
        {
            statusBadge.Visible = visible;
            lblCustomerInfo.Visible = visible;
            dgvItems.Visible = visible;
            btnAdvance.Visible = visible && btnAdvance.Visible;
            btnReject.Visible = visible && btnReject.Visible;
            btnCancel.Visible = visible && btnCancel.Visible;
            if (!visible)
            {
                lblOrderNumber.Text = "Select an order";
                btnViewPrescription.Visible = false;
            }
        }
    }
}
