using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.Models.Reports;
using SmartMed.Models.Results;
using SmartMed.UI.Components;

namespace SmartMed.UI.Forms
{
    public enum ReportType
    {
        DailySales,
        SalesByDateRange,
        MonthlySales,
        SalesReport,
        PaymentSummary,
        PurchaseReport,
        DailyPurchases,
        CurrentStock,
        LowStock,
        NearExpiry,
        ExpiredMedicines,
        StockMovements,
        BatchReport,
        MedicineList,
        CategorySummary,
        TopSelling,
        SlowMoving,
        SupplierList,
        ProfitReport,
        RevenueReport,
        PurchaseCostReport,
        MonthlySalesTrend
    }

    public class ReportsForm : Form
    {
        private readonly IReportService _reportService;

        private ComboBox cboReportType;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private NumericUpDown nudTopN;
        private NumericUpDown nudThreshold;
        private NumericUpDown nudDays;
        private NumericUpDown nudMonths;
        private NumericUpDown nudYear;
        private Label lblTopN;
        private Label lblThreshold;
        private Label lblDays;
        private Label lblMonths;
        private Label lblYear;
        private Button btnGenerate;
        private Button btnExportCsv;
        private Button btnExportExcel;
        private Button btnExportPdf;
        private Button btnPrint;
        private DataGridView dgvReport;
        private Label lblStatus;

        private object _currentData;

        public ReportsForm(IReportService reportService)
        {
            _reportService = reportService;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "SmartMed - Reports";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(960, 640);
            ShowIcon = false;

            int y = 12;
            int rowH = 28;
            int labelX = 12;
            int fieldX = 100;

            Label lblType = new Label { Text = "Report Type:", Location = new Point(labelX, y + 4), AutoSize = true };
            Controls.Add(lblType);

            cboReportType = new ComboBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(240, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboReportType.Items.AddRange(Enum.GetNames(typeof(ReportType)));
            cboReportType.SelectedIndex = 0;
            cboReportType.SelectedIndexChanged += ReportTypeChanged;
            Controls.Add(cboReportType);

            int filterX = 380;
            dtpFrom = new DateTimePicker
            {
                Location = new Point(filterX, y),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(-30)
            };
            Controls.Add(dtpFrom);

            Label lblTo = new Label { Text = "to", Location = new Point(filterX + 110, y + 4), AutoSize = true };
            Controls.Add(lblTo);

            dtpTo = new DateTimePicker
            {
                Location = new Point(filterX + 130, y),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            Controls.Add(dtpTo);

            y += rowH;

            lblYear = new Label { Text = "Year:", Location = new Point(labelX, y + 4), AutoSize = true, Visible = false };
            Controls.Add(lblYear);
            nudYear = new NumericUpDown
            {
                Location = new Point(fieldX, y),
                Width = 80,
                Minimum = 2020,
                Maximum = 2100,
                Value = DateTime.Today.Year,
                Visible = false
            };
            Controls.Add(nudYear);

            lblTopN = new Label { Text = "Top N:", Location = new Point(labelX + 140, y + 4), AutoSize = true, Visible = false };
            Controls.Add(lblTopN);
            nudTopN = new NumericUpDown
            {
                Location = new Point(labelX + 190, y),
                Width = 60,
                Minimum = 1,
                Maximum = 100,
                Value = 10,
                Visible = false
            };
            Controls.Add(nudTopN);

            lblThreshold = new Label { Text = "Threshold Days:", Location = new Point(labelX + 270, y + 4), AutoSize = true, Visible = false };
            Controls.Add(lblThreshold);
            nudThreshold = new NumericUpDown
            {
                Location = new Point(labelX + 380, y),
                Width = 60,
                Minimum = 1,
                Maximum = 365,
                Value = 90,
                Visible = false
            };
            Controls.Add(nudThreshold);

            lblDays = new Label { Text = "Days:", Location = new Point(labelX + 140, y + 4), AutoSize = true, Visible = false };
            Controls.Add(lblDays);
            nudDays = new NumericUpDown
            {
                Location = new Point(labelX + 180, y),
                Width = 60,
                Minimum = 1,
                Maximum = 365,
                Value = 30,
                Visible = false
            };
            Controls.Add(nudDays);

            lblMonths = new Label { Text = "Months:", Location = new Point(labelX + 140, y + 4), AutoSize = true, Visible = false };
            Controls.Add(lblMonths);
            nudMonths = new NumericUpDown
            {
                Location = new Point(labelX + 200, y),
                Width = 60,
                Minimum = 1,
                Maximum = 60,
                Value = 12,
                Visible = false
            };
            Controls.Add(nudMonths);

            y += rowH + 4;

            btnGenerate = new Button
            {
                Text = "Generate Report",
                Location = new Point(labelX, y),
                Size = new Size(120, 28)
            };
            btnGenerate.Click += GenerateReport;
            Controls.Add(btnGenerate);

            btnExportCsv = new Button
            {
                Text = "Export CSV",
                Location = new Point(labelX + 130, y),
                Size = new Size(96, 28),
                Enabled = false
            };
            btnExportCsv.Click += ExportCsv;
            Controls.Add(btnExportCsv);

            btnExportExcel = new Button
            {
                Text = "Export Excel",
                Location = new Point(labelX + 236, y),
                Size = new Size(96, 28),
                Enabled = false
            };
            btnExportExcel.Click += ExportExcel;
            Controls.Add(btnExportExcel);

            btnExportPdf = new Button
            {
                Text = "Export PDF",
                Location = new Point(labelX + 342, y),
                Size = new Size(96, 28),
                Enabled = false
            };
            btnExportPdf.Click += ExportPdf;
            Controls.Add(btnExportPdf);

            btnPrint = new Button
            {
                Text = "Print",
                Location = new Point(labelX + 448, y),
                Size = new Size(80, 28),
                Enabled = false
            };
            btnPrint.Click += PrintReport;
            Controls.Add(btnPrint);

            y += rowH + 4;

            dgvReport = new DataGridView
            {
                Location = new Point(labelX, y),
                Size = new Size(930, 520),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Controls.Add(dgvReport);

            y += dgvReport.Height + 4;

            lblStatus = new Label
            {
                Text = "Ready",
                Location = new Point(labelX, y),
                AutoSize = true,
                ForeColor = Color.Gray
            };
            Controls.Add(lblStatus);

            ReportTypeChanged(null, null);
        }

        private void ReportTypeChanged(object sender, EventArgs e)
        {
            string selected = cboReportType.SelectedItem?.ToString() ?? "";
            bool hasDateRange = true;
            bool hasTopN = false;
            bool hasThreshold = false;
            bool hasDays = false;
            bool hasMonths = false;
            bool hasYear = false;

            switch (selected)
            {
                case nameof(ReportType.MonthlySales):
                    hasYear = true;
                    hasDateRange = false;
                    break;
                case nameof(ReportType.TopSelling):
                    hasTopN = true;
                    break;
                case nameof(ReportType.SlowMoving):
                    hasThreshold = true;
                    hasDateRange = false;
                    break;
                case nameof(ReportType.NearExpiry):
                    hasDays = true;
                    hasDateRange = false;
                    break;
                case nameof(ReportType.CurrentStock):
                case nameof(ReportType.ExpiredMedicines):
                case nameof(ReportType.MedicineList):
                case nameof(ReportType.CategorySummary):
                case nameof(ReportType.SupplierList):
                    hasDateRange = false;
                    break;
                case nameof(ReportType.MonthlySalesTrend):
                    hasMonths = true;
                    hasDateRange = false;
                    break;
                case nameof(ReportType.DailySales):
                case nameof(ReportType.DailyPurchases):
                case nameof(ReportType.LowStock):
                case nameof(ReportType.BatchReport):
                    break;
            }

            dtpFrom.Visible = hasDateRange;
            dtpTo.Visible = hasDateRange;
            Label lblTo = (Label)Controls[3];
            lblTo.Visible = hasDateRange;
            lblYear.Visible = hasYear;
            nudYear.Visible = hasYear;
            lblTopN.Visible = hasTopN;
            nudTopN.Visible = hasTopN;
            lblThreshold.Visible = hasThreshold;
            nudThreshold.Visible = hasThreshold;
            lblDays.Visible = hasDays;
            nudDays.Visible = hasDays;
            lblMonths.Visible = hasMonths;
            nudMonths.Visible = hasMonths;
        }

        private void GenerateReport(object sender, EventArgs e)
        {
            string selected = cboReportType.SelectedItem?.ToString() ?? "";
            DateTime from = dtpFrom.Value;
            DateTime to = dtpTo.Value;
            _currentData = null;

            try
            {
                switch (selected)
                {
                    case nameof(ReportType.DailySales):
                        ShowResult(_reportService.GetDailySales(from), "DailySalesRow");
                        break;
                    case nameof(ReportType.SalesByDateRange):
                        ShowResult(_reportService.GetSalesByDateRange(from, to), "DailySalesRow");
                        break;
                    case nameof(ReportType.MonthlySales):
                        ShowResult(_reportService.GetMonthlySales((int)nudYear.Value), "MonthlySalesRow");
                        break;
                    case nameof(ReportType.SalesReport):
                        ShowResult(_reportService.GetSalesReport(from, to), "SalesReportRow");
                        break;
                    case nameof(ReportType.PaymentSummary):
                        ShowResult(_reportService.GetPaymentSummary(from, to), "PaymentSummaryRow");
                        break;
                    case nameof(ReportType.PurchaseReport):
                        ShowResult(_reportService.GetPurchaseReport(from, to), "PurchaseReportRow");
                        break;
                    case nameof(ReportType.DailyPurchases):
                        ShowResult(_reportService.GetDailyPurchases(from), "PurchaseReportRow");
                        break;
                    case nameof(ReportType.CurrentStock):
                        ShowResult(_reportService.GetCurrentStockReport(), "InventoryReportRow");
                        break;
                    case nameof(ReportType.LowStock):
                        ShowResult(_reportService.GetLowStockReport((int)nudThreshold.Value), "InventoryReportRow");
                        break;
                    case nameof(ReportType.NearExpiry):
                        ShowResult(_reportService.GetNearExpiryReport((int)nudDays.Value), "InventoryReportRow");
                        break;
                    case nameof(ReportType.ExpiredMedicines):
                        ShowResult(_reportService.GetExpiredMedicinesReport(), "InventoryReportRow");
                        break;
                    case nameof(ReportType.StockMovements):
                        ShowResult(_reportService.GetStockMovementReport(from, to), "StockMovementReportRow");
                        break;
                    case nameof(ReportType.BatchReport):
                        ShowResult(_reportService.GetBatchReport(), "BatchReportRow");
                        break;
                    case nameof(ReportType.MedicineList):
                        ShowResult(_reportService.GetMedicineList(), "MedicineReportRow");
                        break;
                    case nameof(ReportType.CategorySummary):
                        ShowResult(_reportService.GetCategorySummary(), "CategorySummaryRow");
                        break;
                    case nameof(ReportType.TopSelling):
                        ShowResult(_reportService.GetTopSellingMedicines(from, to, (int)nudTopN.Value), "TopSellingMedicineRow");
                        break;
                    case nameof(ReportType.SlowMoving):
                        ShowResult(_reportService.GetSlowMovingMedicines((int)nudThreshold.Value), "SlowMovingMedicineRow");
                        break;
                    case nameof(ReportType.SupplierList):
                        ShowResult(_reportService.GetSupplierList(), "SupplierReportRow");
                        break;
                    case nameof(ReportType.ProfitReport):
                        ShowResult(_reportService.GetProfitReport(from, to), "ProfitReportRow");
                        break;
                    case nameof(ReportType.RevenueReport):
                        ShowResult(_reportService.GetRevenueReport(from, to), "SalesReportRow");
                        break;
                    case nameof(ReportType.PurchaseCostReport):
                        ShowResult(_reportService.GetPurchaseCostReport(from, to), "PurchaseReportRow");
                        break;
                    case nameof(ReportType.MonthlySalesTrend):
                        ShowResult(_reportService.GetMonthlySalesTrend((int)nudMonths.Value), "MonthlySalesRow");
                        break;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void ShowResult<T>(OperationResult<List<T>> result, string rowType)
        {
            if (result.IsSuccess && result.Data != null)
            {
                _currentData = result.Data;
                dgvReport.DataSource = null;
                dgvReport.DataSource = result.Data;
                dgvReport.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                lblStatus.Text = $"Loaded {result.Data.Count} rows";
                lblStatus.ForeColor = Color.Gray;
                btnExportCsv.Enabled = result.Data.Count > 0;
                btnExportExcel.Enabled = result.Data.Count > 0;
                btnExportPdf.Enabled = result.Data.Count > 0;
                btnPrint.Enabled = result.Data.Count > 0;
            }
            else
            {
                lblStatus.Text = result.Message;
                lblStatus.ForeColor = Color.Red;
                btnExportCsv.Enabled = false;
                btnExportExcel.Enabled = false;
                btnExportPdf.Enabled = false;
                btnPrint.Enabled = false;
            }
        }

        private void ExportCsv(object sender, EventArgs e)
        {
            if (_currentData == null) return;

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"{cboReportType.SelectedItem}_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OperationResult<byte[]> result = CallExportCsv();
                if (result.IsSuccess)
                {
                    System.IO.File.WriteAllBytes(dialog.FileName, result.Data);
                    lblStatus.Text = "Exported to CSV successfully";
                }
                else
                {
                    MessageBox.Show(result.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private OperationResult<byte[]> CallExportCsv()
        {
            return _currentData switch
            {
                List<DailySalesSummary> d => _reportService.ExportToCsv(d),
                List<MonthlySalesSummary> m => _reportService.ExportToCsv(m),
                List<SalesReportRow> s => _reportService.ExportToCsv(s),
                List<PaymentSummaryRow> p => _reportService.ExportToCsv(p),
                List<PurchaseReportRow> p => _reportService.ExportToCsv(p),
                List<InventoryReportRow> i => _reportService.ExportToCsv(i),
                List<StockMovementReportRow> s => _reportService.ExportToCsv(s),
                List<BatchReportRow> b => _reportService.ExportToCsv(b),
                List<MedicineReportRow> m => _reportService.ExportToCsv(m),
                List<CategorySummaryRow> c => _reportService.ExportToCsv(c),
                List<TopSellingMedicineRow> t => _reportService.ExportToCsv(t),
                List<SlowMovingMedicineRow> s => _reportService.ExportToCsv(s),
                List<SupplierReportRow> s => _reportService.ExportToCsv(s),
                List<ProfitReportRow> p => _reportService.ExportToCsv(p),
                _ => OperationResult<byte[]>.Failure("Unsupported data type for export")
            };
        }

        private void ExportExcel(object sender, EventArgs e)
        {
            if (_currentData == null) return;

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"{cboReportType.SelectedItem}_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OperationResult<byte[]> result = CallExportExcel();
                if (result.IsSuccess)
                {
                    System.IO.File.WriteAllBytes(dialog.FileName, result.Data);
                    lblStatus.Text = "Exported to Excel successfully";
                }
                else
                {
                    MessageBox.Show(result.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportPdf(object sender, EventArgs e)
        {
            if (_currentData == null) return;

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"{cboReportType.SelectedItem}_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OperationResult<byte[]> result = CallExportPdf();
                if (result.IsSuccess)
                {
                    System.IO.File.WriteAllBytes(dialog.FileName, result.Data);
                    lblStatus.Text = "Exported to PDF successfully";
                }
                else
                {
                    MessageBox.Show(result.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private OperationResult<byte[]> CallExportPdf()
        {
            string title = cboReportType.SelectedItem?.ToString() ?? "Report";

            return _currentData switch
            {
                List<DailySalesSummary> d => _reportService.ExportToPdf(title, d),
                List<MonthlySalesSummary> m => _reportService.ExportToPdf(title, m),
                List<SalesReportRow> s => _reportService.ExportToPdf(title, s),
                List<PaymentSummaryRow> p => _reportService.ExportToPdf(title, p),
                List<PurchaseReportRow> p => _reportService.ExportToPdf(title, p),
                List<InventoryReportRow> i => _reportService.ExportToPdf(title, i),
                List<StockMovementReportRow> s => _reportService.ExportToPdf(title, s),
                List<BatchReportRow> b => _reportService.ExportToPdf(title, b),
                List<MedicineReportRow> m => _reportService.ExportToPdf(title, m),
                List<CategorySummaryRow> c => _reportService.ExportToPdf(title, c),
                List<TopSellingMedicineRow> t => _reportService.ExportToPdf(title, t),
                List<SlowMovingMedicineRow> s => _reportService.ExportToPdf(title, s),
                List<SupplierReportRow> s => _reportService.ExportToPdf(title, s),
                List<ProfitReportRow> p => _reportService.ExportToPdf(title, p),
                _ => OperationResult<byte[]>.Failure("Unsupported data type for export")
            };
        }

        private OperationResult<byte[]> CallExportExcel()
        {
            return _currentData switch
            {
                List<DailySalesSummary> d => _reportService.ExportToExcel(d),
                List<MonthlySalesSummary> m => _reportService.ExportToExcel(m),
                List<SalesReportRow> s => _reportService.ExportToExcel(s),
                List<PaymentSummaryRow> p => _reportService.ExportToExcel(p),
                List<PurchaseReportRow> p => _reportService.ExportToExcel(p),
                List<InventoryReportRow> i => _reportService.ExportToExcel(i),
                List<StockMovementReportRow> s => _reportService.ExportToExcel(s),
                List<BatchReportRow> b => _reportService.ExportToExcel(b),
                List<MedicineReportRow> m => _reportService.ExportToExcel(m),
                List<CategorySummaryRow> c => _reportService.ExportToExcel(c),
                List<TopSellingMedicineRow> t => _reportService.ExportToExcel(t),
                List<SlowMovingMedicineRow> s => _reportService.ExportToExcel(s),
                List<SupplierReportRow> s => _reportService.ExportToExcel(s),
                List<ProfitReportRow> p => _reportService.ExportToExcel(p),
                _ => OperationResult<byte[]>.Failure("Unsupported data type for export")
            };
        }

        private void PrintReport(object sender, EventArgs e)
        {
            if (_currentData == null) return;

            string[] headers = GetHeadersForCurrentReport();
            List<string[]> rows = GetRowsForCurrentReport();
            string title = cboReportType.SelectedItem?.ToString() ?? "Report";
            string dateRange = $"Period: {dtpFrom.Value:yyyy-MM-dd} to {dtpTo.Value:yyyy-MM-dd}";

            ReportPrintDocument printDoc = new ReportPrintDocument(title, dateRange, headers, rows);
            PrintDialog printDialog = new PrintDialog { Document = printDoc };

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDoc.Print();
            }
        }

        private string[] GetHeadersForCurrentReport()
        {
            return _currentData switch
            {
                List<DailySalesSummary> => new[] { "Date", "Transactions", "Revenue", "Discount", "Tax", "Avg Value" },
                List<MonthlySalesSummary> => new[] { "Year", "Month", "Transactions", "Revenue", "Cost", "Profit" },
                List<SalesReportRow> => new[] { "Sale#", "Date", "Cashier", "Customer", "Items", "Total" },
                List<PaymentSummaryRow> => new[] { "Method", "Transactions", "Amount", "%" },
                List<PurchaseReportRow> => new[] { "PO#", "Date", "Supplier", "Invoice", "Items", "Amount", "Status" },
                List<InventoryReportRow> => new[] { "Medicine", "Category", "Batch", "Qty", "Reorder", "Expiry", "Days Left" },
                List<StockMovementReportRow> => new[] { "Date", "Medicine", "Batch", "Type", "Qty", "Price", "Total" },
                List<BatchReportRow> => new[] { "Batch#", "Medicine", "Expiry", "Initial", "Current", "Price", "Status" },
                List<MedicineReportRow> => new[] { "Medicine", "Category", "Brand", "Stock", "Reorder", "Price", "Active" },
                List<CategorySummaryRow> => new[] { "Category", "Medicines", "Total Stock", "Stock Value" },
                List<TopSellingMedicineRow> => new[] { "Rank", "Medicine", "Category", "Sold", "Revenue" },
                List<SlowMovingMedicineRow> => new[] { "Medicine", "Category", "Stock", "Days Since Sale" },
                List<SupplierReportRow> => new[] { "Supplier", "Contact", "Phone", "Purchases", "Last Purchase", "Count" },
                List<ProfitReportRow> => new[] { "Sale#", "Date", "Revenue", "COGS", "Profit", "Margin" },
                _ => new[] { "Data" }
            };
        }

        private List<string[]> GetRowsForCurrentReport()
        {
            List<string[]> rows = new List<string[]>();

            switch (_currentData)
            {
                case List<DailySalesSummary> dailyList:
                    foreach (var r in dailyList)
                        rows.Add(new[] { r.Date.ToString("yyyy-MM-dd"), r.TransactionCount.ToString(), r.TotalRevenue.ToString("N2"), r.TotalDiscount.ToString("N2"), r.TotalTax.ToString("N2"), r.AverageTransactionValue.ToString("N2") });
                    break;
                case List<MonthlySalesSummary> monthlyList:
                    foreach (var r in monthlyList)
                        rows.Add(new[] { r.Year.ToString(), r.Month.ToString(), r.TransactionCount.ToString(), r.TotalRevenue.ToString("N2"), r.TotalCost.ToString("N2"), r.TotalProfit.ToString("N2") });
                    break;
                case List<SalesReportRow> salesList:
                    foreach (var r in salesList)
                        rows.Add(new[] { r.SaleNumber, r.SaleDate.ToString("yyyy-MM-dd"), r.CashierName, r.CustomerName, r.ItemCount.ToString(), r.GrandTotal.ToString("N2") });
                    break;
                case List<PaymentSummaryRow> paymentList:
                    foreach (var r in paymentList)
                        rows.Add(new[] { r.PaymentMethod, r.TransactionCount.ToString(), r.TotalAmount.ToString("N2"), r.PercentageOfTotal.ToString("N1") + "%" });
                    break;
                case List<PurchaseReportRow> purchaseList:
                    foreach (var r in purchaseList)
                        rows.Add(new[] { r.PurchaseNumber, r.PurchaseDate.ToString("yyyy-MM-dd"), r.SupplierName, r.InvoiceNumber, r.ItemCount.ToString(), r.TotalAmount.ToString("N2"), r.Status.ToString() });
                    break;
                case List<InventoryReportRow> inventoryList:
                    foreach (var r in inventoryList)
                        rows.Add(new[] { r.MedicineName, r.CategoryName, r.BatchNumber, r.CurrentQuantity.ToString(), r.ReorderLevel.ToString(), r.ExpiryDate.ToString("yyyy-MM-dd"), r.DaysUntilExpiry.ToString() });
                    break;
                case List<StockMovementReportRow> movementList:
                    foreach (var r in movementList)
                        rows.Add(new[] { r.MovementDate.ToString("yyyy-MM-dd HH:mm"), r.MedicineName, r.BatchNumber, r.MovementType.ToString(), r.Quantity.ToString(), r.UnitPrice.ToString("N2"), r.TotalAmount.ToString("N2") });
                    break;
                case List<BatchReportRow> batchList:
                    foreach (var r in batchList)
                        rows.Add(new[] { r.BatchNumber, r.MedicineName, r.ExpiryDate.ToString("yyyy-MM-dd"), r.InitialQuantity.ToString(), r.CurrentQuantity.ToString(), r.PurchasePrice.ToString("N2"), r.BatchStatus.ToString() });
                    break;
                case List<MedicineReportRow> medicineList:
                    foreach (var r in medicineList)
                        rows.Add(new[] { r.MedicineName, r.CategoryName, r.Brand, r.StockQuantity.ToString(), r.ReorderLevel.ToString(), r.UnitPrice.ToString("N2"), r.IsActive ? "Yes" : "No" });
                    break;
                case List<CategorySummaryRow> categoryList:
                    foreach (var r in categoryList)
                        rows.Add(new[] { r.CategoryName, r.MedicineCount.ToString(), r.TotalStockQuantity.ToString(), r.TotalStockValue.ToString("N2") });
                    break;
                case List<TopSellingMedicineRow> topList:
                    foreach (var r in topList)
                        rows.Add(new[] { r.Rank.ToString(), r.MedicineName, r.CategoryName, r.TotalQuantitySold.ToString(), r.TotalRevenue.ToString("N2") });
                    break;
                case List<SlowMovingMedicineRow> slowList:
                    foreach (var r in slowList)
                        rows.Add(new[] { r.MedicineName, r.CategoryName, r.StockQuantity.ToString(), r.DaysSinceLastSale.ToString() });
                    break;
                case List<SupplierReportRow> supplierList:
                    foreach (var r in supplierList)
                        rows.Add(new[] { r.SupplierName, r.ContactPerson, r.Phone, r.TotalPurchases.ToString("N2"), r.LastPurchaseDate?.ToString("yyyy-MM-dd") ?? "N/A", r.PurchaseCount.ToString() });
                    break;
                case List<ProfitReportRow> profitList:
                    foreach (var r in profitList)
                        rows.Add(new[] { r.SaleNumber, r.SaleDate.ToString("yyyy-MM-dd"), r.GrandTotal.ToString("N2"), r.TotalCostOfGoodsSold.ToString("N2"), r.Profit.ToString("N2"), r.ProfitMargin.ToString("N1") + "%" });
                    break;
            }

            return rows;
        }
    }
}
