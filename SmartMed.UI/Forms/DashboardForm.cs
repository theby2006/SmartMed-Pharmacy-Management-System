using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Reports;
using SmartMed.Models.Results;
using SmartMed.UI.Theme;
using SmartMed.UI.Theme.Controls;

namespace SmartMed.UI.Forms
{
    /// <summary>
    /// Administrator dashboard: KPI summary tiles, a 7-day sales trend
    /// column chart, a stock-status doughnut, and a low-stock/near-expiry
    /// alert panel.
    /// </summary>
    public class DashboardForm : Form
    {
        private readonly IReportService _reportService;

        private KpiCard kpiRevenue;
        private KpiCard kpiTransactions;
        private KpiCard kpiLowStock;
        private KpiCard kpiNearExpiry;
        private Chart salesChart;
        private Chart stockChart;
        private CardPanel alertCard;
        private FlowLayoutPanel alertList;

        public DashboardForm(IReportService reportService)
        {
            _reportService = reportService;
            InitializeComponents();
            LoadDashboard();
        }

        private void InitializeComponents()
        {
            Text = "Dashboard";
            BackColor = AppTheme.Background;
            AutoScroll = true;

            Label title = new Label { AutoSize = true, Font = AppTheme.PageTitle, ForeColor = AppTheme.TextPrimary, Location = new Point(0, 0), Text = "Dashboard" };
            Controls.Add(title);

            int kpiY = 48;
            int kpiWidth = 210;
            int kpiGap = 16;

            kpiRevenue = new KpiCard { Location = new Point(0, kpiY), Glyph = IconFactory.Sales, AccentColor = AppTheme.Success, Caption = "Today's Revenue" };
            kpiRevenue.Width = kpiWidth;
            Controls.Add(kpiRevenue);

            kpiTransactions = new KpiCard { Location = new Point(kpiWidth + kpiGap, kpiY), Glyph = IconFactory.Dashboard, AccentColor = AppTheme.Info, Caption = "Today's Transactions" };
            kpiTransactions.Width = kpiWidth;
            Controls.Add(kpiTransactions);

            kpiLowStock = new KpiCard { Location = new Point((kpiWidth + kpiGap) * 2, kpiY), Glyph = IconFactory.Warning, AccentColor = AppTheme.Warning, Caption = "Low Stock Items" };
            kpiLowStock.Width = kpiWidth;
            Controls.Add(kpiLowStock);

            kpiNearExpiry = new KpiCard { Location = new Point((kpiWidth + kpiGap) * 3, kpiY), Glyph = IconFactory.Warning, AccentColor = AppTheme.Danger, Caption = "Near Expiry (30d)" };
            kpiNearExpiry.Width = kpiWidth;
            Controls.Add(kpiNearExpiry);

            int chartY = kpiY + 130;

            CardPanel salesCard = new CardPanel { Location = new Point(0, chartY), Size = new Size(580, 320) };
            Controls.Add(salesCard);
            Label salesTitle = new Label { AutoSize = true, Font = AppTheme.SectionHeader, ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), Text = "Last 7 Days Sales" };
            salesCard.Controls.Add(salesTitle);

            salesChart = new Chart { Location = new Point(20, 56), Size = new Size(540, 244) };
            ChartArea salesArea = new ChartArea("SalesArea");
            salesArea.AxisX.MajorGrid.LineColor = AppTheme.Divider;
            salesArea.AxisY.MajorGrid.LineColor = AppTheme.Divider;
            salesArea.AxisX.LineColor = AppTheme.Border;
            salesArea.AxisY.LineColor = AppTheme.Border;
            salesArea.AxisX.LabelStyle.ForeColor = AppTheme.TextSecondary;
            salesArea.AxisY.LabelStyle.ForeColor = AppTheme.TextSecondary;
            salesArea.BackColor = Color.Transparent;
            salesChart.ChartAreas.Add(salesArea);
            salesChart.BackColor = Color.Transparent;
            salesCard.Controls.Add(salesChart);

            CardPanel stockCard = new CardPanel { Location = new Point(604, chartY), Size = new Size(300, 320) };
            Controls.Add(stockCard);
            Label stockTitle = new Label { AutoSize = true, Font = AppTheme.SectionHeader, ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), Text = "Stock Status" };
            stockCard.Controls.Add(stockTitle);

            stockChart = new Chart { Location = new Point(20, 56), Size = new Size(260, 244) };
            ChartArea stockArea = new ChartArea("StockArea") { BackColor = Color.Transparent };
            stockChart.ChartAreas.Add(stockArea);
            stockChart.BackColor = Color.Transparent;
            Legend legend = new Legend { Docking = Docking.Bottom, Font = AppTheme.Caption, ForeColor = AppTheme.TextSecondary };
            stockChart.Legends.Add(legend);
            stockCard.Controls.Add(stockChart);

            int alertY = chartY + 340;
            alertCard = new CardPanel { Location = new Point(0, alertY), Size = new Size(900, 140), Visible = false };
            Controls.Add(alertCard);

            Label alertTitle = new Label { AutoSize = true, Font = AppTheme.SectionHeader, ForeColor = AppTheme.Warning, Location = new Point(20, 16), Text = "⚠ Attention Needed" };
            alertCard.Controls.Add(alertTitle);

            alertList = new FlowLayoutPanel { Location = new Point(20, 52), Size = new Size(860, 80), FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            alertCard.Controls.Add(alertList);
        }

        private void LoadDashboard()
        {
            OperationResult<DashboardSummary> result = _reportService.GetDashboardSummary();
            if (!result.IsSuccess)
            {
                ToastNotifier.Show(FindForm(), result.Message, ToastKind.Error);
                return;
            }

            DashboardSummary data = result.Data;
            kpiRevenue.MetricValue = data.TodaySalesRevenue.ToString("C2");
            kpiTransactions.MetricValue = data.TodaySalesCount.ToString("N0");
            kpiLowStock.MetricValue = data.LowStockCount.ToString("N0");
            kpiNearExpiry.MetricValue = data.NearExpiryCount.ToString("N0");

            LoadSalesChart();
            LoadStockChart(data);
            LoadAlerts(data);
        }

        private void LoadSalesChart()
        {
            DateTime to = DateTime.Today;
            DateTime from = to.AddDays(-6);

            OperationResult<List<DailySalesSummary>> result = _reportService.GetSalesByDateRange(from, to);
            List<DailySalesSummary> days = result.IsSuccess ? result.Data : new List<DailySalesSummary>();

            salesChart.Series.Clear();
            Series series = new Series("Revenue")
            {
                ChartType = SeriesChartType.Column,
                Color = AppTheme.Accent
            };

            for (DateTime day = from; day <= to; day = day.AddDays(1))
            {
                DailySalesSummary match = days.FirstOrDefault(d => d.Date.Date == day.Date);
                decimal revenue = match?.TotalRevenue ?? 0m;
                series.Points.AddXY(day.ToString("MM/dd"), revenue);
            }

            salesChart.Series.Add(series);
        }

        private void LoadStockChart(DashboardSummary data)
        {
            int okCount = Math.Max(0, data.TotalMedicines - data.LowStockCount - data.ExpiredCount - data.NearExpiryCount);

            stockChart.Series.Clear();
            Series series = new Series("Stock") { ChartType = SeriesChartType.Doughnut };

            series.Points.AddXY("OK", okCount);
            series.Points[0].Color = AppTheme.Success;

            series.Points.AddXY("Low Stock", data.LowStockCount);
            series.Points[1].Color = AppTheme.Warning;

            series.Points.AddXY("Near Expiry", data.NearExpiryCount);
            series.Points[2].Color = AppTheme.Info;

            series.Points.AddXY("Expired", data.ExpiredCount);
            series.Points[3].Color = AppTheme.Danger;

            stockChart.Series.Add(series);
        }

        private void LoadAlerts(DashboardSummary data)
        {
            alertList.Controls.Clear();
            bool hasAlerts = data.LowStockCount > 0 || data.NearExpiryCount > 0;
            alertCard.Visible = hasAlerts;

            if (data.LowStockCount > 0)
            {
                alertList.Controls.Add(CreateAlertLine($"{data.LowStockCount} medicine(s) at or below their reorder level.", AppTheme.Warning));
            }

            if (data.NearExpiryCount > 0)
            {
                alertList.Controls.Add(CreateAlertLine($"{data.NearExpiryCount} medicine(s) expire within 30 days.", AppTheme.Danger));
            }
        }

        private Label CreateAlertLine(string text, Color color)
        {
            return new Label
            {
                AutoSize = true,
                Font = AppTheme.Body,
                ForeColor = color,
                Text = "• " + text
            };
        }
    }
}
