using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;
using SmartMed.UI.Services;
using SmartMed.UI.Theme;
using SmartMed.UI.Theme.Controls;

namespace SmartMed.UI.Forms
{
    /// <summary>
    /// Combined shopping-cart and checkout screen: edit quantities, see the
    /// running total, attach a prescription when required, and place the
    /// order without leaving the form.
    /// </summary>
    public class CartAndCheckoutForm : Form
    {
        private readonly CartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IPrescriptionService _prescriptionService;
        private readonly ISessionManager _sessionManager;

        private DataGridView dgvCart;
        private Label lblSubTotal;
        private Label lblDiscount;
        private Label lblGrandTotal;
        private Panel prescriptionPanel;
        private Label lblPrescriptionStatus;
        private RoundedButton btnUploadPrescription;
        private RoundedButton btnPlaceOrder;
        private Label lblError;

        private string _uploadedPrescriptionPath;

        public CartAndCheckoutForm(
            CartService cartService,
            IOrderService orderService,
            IPrescriptionService prescriptionService,
            ISessionManager sessionManager)
        {
            _cartService = cartService;
            _orderService = orderService;
            _prescriptionService = prescriptionService;
            _sessionManager = sessionManager;

            InitializeComponents();
            _cartService.CartChanged += (s, e) => RefreshCart();
            RefreshCart();
        }

        private void InitializeComponents()
        {
            Text = "Cart & Checkout";
            BackColor = AppTheme.Background;

            Label title = new Label
            {
                AutoSize = true,
                Font = AppTheme.PageTitle,
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(0, 0),
                Text = "Your Cart"
            };
            Controls.Add(title);

            dgvCart = new DataGridView
            {
                Location = new Point(0, 48),
                Size = new Size(600, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
            };
            DataGridViewStyler.Apply(dgvCart);
            dgvCart.ReadOnly = false;
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "Medicine", HeaderText = "Medicine", ReadOnly = true });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "Unit Price", ReadOnly = true });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Qty" });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "LineTotal", HeaderText = "Total", ReadOnly = true });
            dgvCart.Columns.Add(new DataGridViewButtonColumn { Name = "Remove", HeaderText = "", Text = "Remove", UseColumnTextForButtonValue = true });
            dgvCart.CellEndEdit += DgvCart_CellEndEdit;
            dgvCart.CellClick += DgvCart_CellClick;
            Controls.Add(dgvCart);

            CardPanel summaryCard = new CardPanel
            {
                Location = new Point(624, 48),
                Size = new Size(300, 340),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            Controls.Add(summaryCard);

            int sy = 20;
            Label summaryTitle = new Label { AutoSize = true, Font = AppTheme.SectionHeader, ForeColor = AppTheme.TextPrimary, Location = new Point(20, sy), Text = "Order Summary" };
            summaryCard.Controls.Add(summaryTitle);
            sy += 40;

            lblSubTotal = AddSummaryRow(summaryCard, "Subtotal", ref sy);
            lblDiscount = AddSummaryRow(summaryCard, "Discount", ref sy);
            sy += 4;
            lblGrandTotal = AddSummaryRow(summaryCard, "Grand Total", ref sy, bold: true);
            sy += 16;

            prescriptionPanel = new Panel { Location = new Point(20, sy), Size = new Size(260, 70), Visible = false };
            Label rxLabel = new Label { AutoSize = true, Font = AppTheme.CaptionBold, ForeColor = AppTheme.Warning, Location = new Point(0, 0), Text = "Prescription required for this order" };
            prescriptionPanel.Controls.Add(rxLabel);
            lblPrescriptionStatus = new Label { AutoSize = true, Font = AppTheme.Caption, ForeColor = AppTheme.TextSecondary, Location = new Point(0, 20), Text = "No file attached" };
            prescriptionPanel.Controls.Add(lblPrescriptionStatus);
            btnUploadPrescription = new RoundedButton { Variant = ButtonVariant.Outline, Text = "Upload Prescription", IconGlyph = IconFactory.Upload, Location = new Point(0, 36), Width = 260 };
            btnUploadPrescription.Click += BtnUploadPrescription_Click;
            prescriptionPanel.Controls.Add(btnUploadPrescription);
            summaryCard.Controls.Add(prescriptionPanel);
            sy += 80;

            lblError = new Label { AutoSize = false, Font = AppTheme.Caption, ForeColor = AppTheme.Danger, Location = new Point(20, sy), Size = new Size(260, 32), Visible = false };
            summaryCard.Controls.Add(lblError);
            sy += 36;

            btnPlaceOrder = new RoundedButton { Variant = ButtonVariant.Primary, Text = "Place Order", Location = new Point(20, sy), Width = 260, Height = AppTheme.ControlHeight };
            btnPlaceOrder.Click += BtnPlaceOrder_Click;
            summaryCard.Controls.Add(btnPlaceOrder);
        }

        private Label AddSummaryRow(Control parent, string caption, ref int y, bool bold = false)
        {
            Label captionLabel = new Label { AutoSize = true, Font = AppTheme.Body, ForeColor = AppTheme.TextSecondary, Location = new Point(20, y), Text = caption };
            parent.Controls.Add(captionLabel);

            Label valueLabel = new Label
            {
                AutoSize = true,
                Font = bold ? AppTheme.SectionHeader : AppTheme.BodyBold,
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(180, y - (bold ? 4 : 0)),
                Text = "$0.00"
            };
            parent.Controls.Add(valueLabel);
            y += bold ? 32 : 24;
            return valueLabel;
        }

        private void RefreshCart()
        {
            var rows = _cartService.Lines.Select(l => new
            {
                l.Medicine.Id,
                Medicine = l.Medicine.Name,
                UnitPrice = l.Medicine.UnitPrice.ToString("C2"),
                Quantity = l.Quantity,
                LineTotal = l.LineTotal.ToString("C2")
            }).ToList();

            dgvCart.Rows.Clear();
            foreach (var row in rows)
            {
                int idx = dgvCart.Rows.Add(row.Medicine, row.UnitPrice, row.Quantity, row.LineTotal, "Remove");
                dgvCart.Rows[idx].Tag = row.Id;
            }

            lblSubTotal.Text = _cartService.SubTotal.ToString("C2");
            lblDiscount.Text = _cartService.DiscountAmount.ToString("C2");
            lblGrandTotal.Text = _cartService.GrandTotal.ToString("C2");

            prescriptionPanel.Visible = _cartService.RequiresPrescription;
            btnPlaceOrder.Enabled = _cartService.Lines.Count > 0;
        }

        private void DgvCart_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvCart.Columns[e.ColumnIndex].Name != "Quantity") return;

            int medicineId = (int)dgvCart.Rows[e.RowIndex].Tag;
            string text = dgvCart.Rows[e.RowIndex].Cells["Quantity"].Value?.ToString();

            if (int.TryParse(text, out int quantity))
            {
                _cartService.UpdateQuantity(medicineId, quantity);
            }
        }

        private void DgvCart_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvCart.Columns[e.ColumnIndex].Name != "Remove") return;

            int medicineId = (int)dgvCart.Rows[e.RowIndex].Tag;
            _cartService.RemoveItem(medicineId);
        }

        private void BtnUploadPrescription_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Prescription files (*.jpg;*.jpeg;*.png;*.pdf)|*.jpg;*.jpeg;*.png;*.pdf",
                Title = "Select prescription file"
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _uploadedPrescriptionPath = dialog.FileName;
                    lblPrescriptionStatus.Text = Path.GetFileName(dialog.FileName);
                    lblPrescriptionStatus.ForeColor = AppTheme.Success;
                }
            }
        }

        private void BtnPlaceOrder_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            if (_cartService.RequiresPrescription && string.IsNullOrEmpty(_uploadedPrescriptionPath))
            {
                lblError.Text = "Please attach a prescription before placing this order.";
                lblError.Visible = true;
                return;
            }

            int? customerId = _sessionManager.CurrentSession?.CustomerId;
            if (!customerId.HasValue)
            {
                lblError.Text = "You must be signed in as a customer to place an order.";
                lblError.Visible = true;
                return;
            }

            Order order = new Order { CustomerId = customerId.Value };
            List<OrderItem> items = _cartService.Lines
                .Select(l => new OrderItem { MedicineId = l.Medicine.Id, Quantity = l.Quantity })
                .ToList();

            btnPlaceOrder.Enabled = false;
            OperationResult<int> result = _orderService.PlaceOrder(order, items);

            if (!result.IsSuccess)
            {
                btnPlaceOrder.Enabled = true;
                lblError.Text = result.Message;
                lblError.Visible = true;
                return;
            }

            if (_cartService.RequiresPrescription && !string.IsNullOrEmpty(_uploadedPrescriptionPath))
            {
                _prescriptionService.UploadPrescription(result.Data, _uploadedPrescriptionPath);
            }

            string orderNumberDisplay = order.OrderNumber ?? $"#{result.Data}";
            _cartService.Clear();
            _uploadedPrescriptionPath = null;
            btnPlaceOrder.Enabled = true;

            MessageBox.Show(
                $"Order {orderNumberDisplay} placed successfully! Track it from My Orders.",
                "Order Placed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
