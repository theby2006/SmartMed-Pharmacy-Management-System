using System;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Reports;
using SmartMed.Models.Results;

namespace SmartMed.UI.Forms
{
    public class DashboardForm : Form
    {
        private readonly IReportService _reportService;

        private Label lblTodaySales;
        private Label lblTodayRevenue;
        private Label lblTodayProfit;
        private Label lblTotalMedicines;
        private Label lblLowStock;
        private Label lblExpired;
        private Label lblNearExpiry;
        private Label lblMonthSales;
        private Label lblMonthPurchases;
        private Label lblActiveSuppliers;
        private Button btnRefresh;

        public DashboardForm(IReportService reportService)
        {
            _reportService = reportService;
            InitializeComponents();
            LoadDashboard();
        }

        private void InitializeComponents()
        {
            Text = "SmartMed - Dashboard";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(600, 420);
            ShowIcon = false;

            int labelX = 24;
            int valueX = 220;
            int rowH = 28;
            int startY = 24;
            int colGap = 300;

            Font headerFont = new Font("Segoe UI", 12, FontStyle.Bold);
            Font labelFont = new Font("Segoe UI", 10);
            Font valueFont = new Font("Segoe UI", 10, FontStyle.Bold);

            Label headerLabel = new Label
            {
                Text = "Dashboard Summary",
                Font = headerFont,
                AutoSize = true,
                Location = new Point(labelX, startY)
            };
            Controls.Add(headerLabel);

            int y = startY + 36;

            lblTodaySales = CreateMetricLabel(labelFont, valueFont, labelX, valueX, y, "Today's Transactions:");
            y += rowH;
            lblTodayRevenue = CreateMetricLabel(labelFont, valueFont, labelX, valueX, y, "Today's Revenue:");
            y += rowH;
            lblTodayProfit = CreateMetricLabel(labelFont, valueFont, labelX, valueX, y, "Today's Profit:");
            y += rowH;

            y += 8;
            int y2 = startY + 36;
            int labelX2 = 24 + colGap;
            int valueX2 = 220 + colGap;

            lblTotalMedicines = CreateMetricLabel(labelFont, valueFont, labelX2, valueX2, y2, "Total Medicines:");
            y2 += rowH;
            lblLowStock = CreateMetricLabel(labelFont, valueFont, labelX2, valueX2, y2, "Low Stock Items:");
            y2 += rowH;
            lblExpired = CreateMetricLabel(labelFont, valueFont, labelX2, valueX2, y2, "Expired Batches:");
            y2 += rowH;
            lblNearExpiry = CreateMetricLabel(labelFont, valueFont, labelX2, valueX2, y2, "Near Expiry (30d):");
            y2 += rowH;
            lblMonthSales = CreateMetricLabel(labelFont, valueFont, labelX2, valueX2, y2, "Monthly Sales:");
            y2 += rowH;
            lblMonthPurchases = CreateMetricLabel(labelFont, valueFont, labelX2, valueX2, y2, "Monthly Purchases:");
            y2 += rowH;
            lblActiveSuppliers = CreateMetricLabel(labelFont, valueFont, labelX2, valueX2, y2, "Active Suppliers:");

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(ClientSize.Width - 120, ClientSize.Height - 48),
                Size = new Size(96, 28)
            };
            btnRefresh.Click += (s, e) => LoadDashboard();
            Controls.Add(btnRefresh);
        }

        private Label CreateMetricLabel(Font labelFont, Font valueFont, int labelX, int valueX, int y, string labelText)
        {
            Label label = new Label
            {
                Text = labelText,
                Font = labelFont,
                AutoSize = true,
                Location = new Point(labelX, y)
            };
            Controls.Add(label);

            Label value = new Label
            {
                Text = "0",
                Font = valueFont,
                AutoSize = true,
                Location = new Point(valueX, y),
                ForeColor = Color.DarkBlue
            };
            Controls.Add(value);

            return value;
        }

        private void LoadDashboard()
        {
            OperationResult<DashboardSummary> result = _reportService.GetDashboardSummary();
            if (result.IsSuccess)
            {
                DashboardSummary data = result.Data;
                lblTodaySales.Text = data.TodaySalesCount.ToString("N0");
                lblTodayRevenue.Text = data.TodaySalesRevenue.ToString("N2");
                lblTodayProfit.Text = data.TodayProfit.ToString("N2");
                lblTotalMedicines.Text = data.TotalMedicines.ToString("N0");
                lblLowStock.Text = data.LowStockCount.ToString("N0");
                lblExpired.Text = data.ExpiredCount.ToString("N0");
                lblNearExpiry.Text = data.NearExpiryCount.ToString("N0");
                lblMonthSales.Text = data.TotalSalesMonth.ToString("N2");
                lblMonthPurchases.Text = data.TotalPurchasesMonth.ToString("N2");
                lblActiveSuppliers.Text = data.TotalActiveSuppliers.ToString("N0");
            }
            else
            {
                MessageBox.Show(result.Message, "Dashboard Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
