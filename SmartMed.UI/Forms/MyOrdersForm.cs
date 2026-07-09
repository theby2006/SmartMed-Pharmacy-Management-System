using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    /// A customer's own order history, with a visual status stepper and a
    /// cancel action while an order is still pending.
    /// </summary>
    public class MyOrdersForm : Form
    {
        private readonly IOrderService _orderService;
        private readonly ISessionManager _sessionManager;
        private readonly IReportService _reportService;

        private DataGridView dgvOrders;
        private CardPanel detailCard;
        private Panel stepperPanel;
        private DataGridView dgvItems;
        private RoundedButton btnCancelOrder;
        private Label lblOrderNumber;
        private RoundedButton btnExportPdf;
        private RoundedButton btnExportExcel;

        private List<Order> _orders = new List<Order>();
        private Order _selectedOrder;

        public MyOrdersForm(IOrderService orderService, ISessionManager sessionManager, IReportService reportService)
        {
            _orderService = orderService;
            _sessionManager = sessionManager;
            _reportService = reportService;
            InitializeComponents();
            LoadOrders();
        }

        private void InitializeComponents()
        {
            Text = "My Orders";
            BackColor = AppTheme.Background;

            Label title = new Label { AutoSize = true, Font = AppTheme.PageTitle, ForeColor = AppTheme.TextPrimary, Location = new Point(0, 0), Text = "My Orders" };
            Controls.Add(title);

            btnExportPdf = new RoundedButton { Variant = ButtonVariant.Outline, Text = "Export PDF", IconGlyph = IconFactory.Pdf, Location = new Point(400, 8), Width = 120 };
            btnExportPdf.Click += (s, e) => ExportHistory(pdf: true);
            Controls.Add(btnExportPdf);

            btnExportExcel = new RoundedButton { Variant = ButtonVariant.Outline, Text = "Export Excel", IconGlyph = IconFactory.Excel, Location = new Point(528, 8), Width = 130 };
            btnExportExcel.Click += (s, e) => ExportHistory(pdf: false);
            Controls.Add(btnExportExcel);

            dgvOrders = new DataGridView
            {
                Location = new Point(0, 48),
                Size = new Size(560, 480),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
            };
            DataGridViewStyler.Apply(dgvOrders);
            dgvOrders.SelectionChanged += (s, e) => ShowSelectedOrderDetail();
            Controls.Add(dgvOrders);

            detailCard = new CardPanel
            {
                Location = new Point(584, 48),
                Size = new Size(360, 480),
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };
            Controls.Add(detailCard);

            lblOrderNumber = new Label { AutoSize = true, Font = AppTheme.SectionHeader, ForeColor = AppTheme.TextPrimary, Location = new Point(20, 20), Text = "Select an order" };
            detailCard.Controls.Add(lblOrderNumber);

            stepperPanel = new Panel { Location = new Point(20, 60), Size = new Size(320, 60) };
            stepperPanel.Paint += StepperPanel_Paint;
            detailCard.Controls.Add(stepperPanel);

            dgvItems = new DataGridView { Location = new Point(20, 132), Size = new Size(320, 260) };
            DataGridViewStyler.Apply(dgvItems);
            detailCard.Controls.Add(dgvItems);

            btnCancelOrder = new RoundedButton { Variant = ButtonVariant.Danger, Text = "Cancel Order", Location = new Point(20, 404), Width = 320, Visible = false };
            btnCancelOrder.Click += BtnCancelOrder_Click;
            detailCard.Controls.Add(btnCancelOrder);
        }

        private void LoadOrders()
        {
            int? customerId = _sessionManager.CurrentSession?.CustomerId;
            if (!customerId.HasValue) return;

            OperationResult<List<Order>> result = _orderService.GetOrdersByCustomer(customerId.Value);
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

        private void ShowSelectedOrderDetail()
        {
            if (dgvOrders.SelectedRows.Count == 0) return;
            int rowIndex = dgvOrders.SelectedRows[0].Index;
            if (rowIndex < 0 || rowIndex >= _orders.Count) return;

            OperationResult<Order> detailResult = _orderService.GetOrderById(_orders[rowIndex].Id);
            if (!detailResult.IsSuccess) return;

            _selectedOrder = detailResult.Data;
            lblOrderNumber.Text = _selectedOrder.OrderNumber;

            var items = _selectedOrder.Items.Select(i => new
            {
                i.MedicineId,
                Quantity = i.Quantity,
                Total = i.LineTotal.ToString("C2")
            }).ToList();
            dgvItems.DataSource = items;

            btnCancelOrder.Visible = _selectedOrder.Status == OrderStatus.Pending;
            stepperPanel.Invalidate();
        }

        private void StepperPanel_Paint(object sender, PaintEventArgs e)
        {
            if (_selectedOrder == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_selectedOrder.Status == OrderStatus.Cancelled || _selectedOrder.Status == OrderStatus.Rejected)
            {
                using (SolidBrush brush = new SolidBrush(AppTheme.StatusCancelled))
                {
                    g.DrawString(_selectedOrder.Status == OrderStatus.Cancelled ? "Order Cancelled" : "Order Rejected",
                        AppTheme.BodyBold, brush, new PointF(0, 16));
                }
                return;
            }

            string[] stepLabels = { "Pending", "Ready for Pickup", "Delivered" };
            int currentStep = _selectedOrder.Status switch
            {
                OrderStatus.Pending => 0,
                OrderStatus.PrescriptionReviewRequired => 0,
                OrderStatus.Approved => 0,
                OrderStatus.Processing => 1,
                OrderStatus.Completed => 2,
                _ => 0
            };

            int circleSize = 20;
            int spacing = 150;
            int startX = 10;
            int centerY = 12;

            for (int i = 0; i < stepLabels.Length; i++)
            {
                int cx = startX + i * spacing;
                bool reached = i <= currentStep;
                Color circleColor = reached ? AppTheme.Accent : AppTheme.Border;

                if (i > 0)
                {
                    int prevCx = startX + (i - 1) * spacing + circleSize;
                    using (Pen linePen = new Pen(i <= currentStep ? AppTheme.Accent : AppTheme.Border, 2))
                        g.DrawLine(linePen, prevCx, centerY + circleSize / 2, cx, centerY + circleSize / 2);
                }

                using (SolidBrush circleBrush = new SolidBrush(circleColor))
                    g.FillEllipse(circleBrush, cx, centerY, circleSize, circleSize);

                using (SolidBrush labelBrush = new SolidBrush(reached ? AppTheme.TextPrimary : AppTheme.TextSecondary))
                using (StringFormat format = new StringFormat { Alignment = StringAlignment.Near })
                {
                    g.DrawString(stepLabels[i], AppTheme.Caption, labelBrush, new PointF(cx - 10, centerY + circleSize + 4));
                }
            }
        }

        private void BtnCancelOrder_Click(object sender, EventArgs e)
        {
            if (_selectedOrder == null) return;

            DialogResult confirm = MessageBox.Show(
                $"Cancel order {_selectedOrder.OrderNumber}?",
                "Confirm Cancellation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            OperationResult result = _orderService.UpdateOrderStatus(_selectedOrder.Id, OrderStatus.Cancelled);
            if (result.IsSuccess)
            {
                LoadOrders();
                btnCancelOrder.Visible = false;
            }
            else
            {
                MessageBox.Show(result.Message, "Cancellation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportHistory(bool pdf)
        {
            if (_orders.Count == 0)
            {
                MessageBox.Show("There are no orders to export.", "Nothing to Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<OrderHistoryExportRow> exportRows = _orders.Select(o => new OrderHistoryExportRow
            {
                OrderNumber = o.OrderNumber,
                Date = o.OrderDate.ToString("yyyy-MM-dd"),
                Status = o.Status.ToString(),
                Total = o.GrandTotal
            }).ToList();

            OperationResult<byte[]> result = pdf
                ? _reportService.ExportToPdf("My Order History", exportRows)
                : _reportService.ExportToExcel(exportRows);

            if (!result.IsSuccess)
            {
                MessageBox.Show(result.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string extension = pdf ? "pdf" : "xlsx";
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = pdf ? "PDF Files (*.pdf)|*.pdf" : "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"MyOrderHistory_{DateTime.Now:yyyyMMdd}.{extension}"
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                System.IO.File.WriteAllBytes(dialog.FileName, result.Data);
            }
        }

        private class OrderHistoryExportRow
        {
            public string OrderNumber { get; set; }
            public string Date { get; set; }
            public string Status { get; set; }
            public decimal Total { get; set; }
        }
    }
}
