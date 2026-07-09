using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Reports;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            Guard.AgainstNull(reportRepository, nameof(reportRepository));
            _reportRepository = reportRepository;
        }

        public OperationResult<List<DailySalesSummary>> GetDailySales(DateTime date)
        {
            try
            {
                List<DailySalesSummary> data = _reportRepository.GetDailySales(date);
                return OperationResult<List<DailySalesSummary>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<DailySalesSummary>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<DailySalesSummary>> GetSalesByDateRange(DateTime from, DateTime to)
        {
            try
            {
                List<DailySalesSummary> data = _reportRepository.GetSalesByDateRange(from, to);
                return OperationResult<List<DailySalesSummary>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<DailySalesSummary>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<MonthlySalesSummary>> GetMonthlySales(int year)
        {
            try
            {
                List<MonthlySalesSummary> data = _reportRepository.GetMonthlySales(year);
                return OperationResult<List<MonthlySalesSummary>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<MonthlySalesSummary>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<SalesReportRow>> GetSalesReport(DateTime from, DateTime to, int? cashierId = null)
        {
            try
            {
                List<SalesReportRow> data;
                if (cashierId.HasValue)
                    data = _reportRepository.GetSalesByCashier(cashierId.Value, from, to);
                else
                    data = _reportRepository.GetRevenueReport(from, to);
                return OperationResult<List<SalesReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<SalesReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<SalesReportRow>> GetCustomerSales(int? customerId, DateTime from, DateTime to)
        {
            try
            {
                List<SalesReportRow> data = _reportRepository.GetCustomerSales(customerId, from, to);
                return OperationResult<List<SalesReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<SalesReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<PaymentSummaryRow>> GetPaymentSummary(DateTime from, DateTime to)
        {
            try
            {
                List<PaymentSummaryRow> data = _reportRepository.GetPaymentSummary(from, to);
                return OperationResult<List<PaymentSummaryRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<PaymentSummaryRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<PurchaseReportRow>> GetPurchaseReport(DateTime from, DateTime to, int? supplierId = null)
        {
            try
            {
                List<PurchaseReportRow> data;
                if (supplierId.HasValue)
                    data = _reportRepository.GetPurchasesBySupplier(supplierId.Value, from, to);
                else
                    data = _reportRepository.GetPurchaseHistory(from, to);
                return OperationResult<List<PurchaseReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<PurchaseReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<PurchaseReportRow>> GetDailyPurchases(DateTime date)
        {
            try
            {
                List<PurchaseReportRow> data = _reportRepository.GetDailyPurchases(date);
                return OperationResult<List<PurchaseReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<PurchaseReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<InventoryReportRow>> GetCurrentStockReport()
        {
            try
            {
                List<InventoryReportRow> data = _reportRepository.GetCurrentStock();
                return OperationResult<List<InventoryReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<InventoryReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<InventoryReportRow>> GetLowStockReport(int threshold)
        {
            try
            {
                List<InventoryReportRow> data = _reportRepository.GetLowStock(threshold);
                return OperationResult<List<InventoryReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<InventoryReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<InventoryReportRow>> GetNearExpiryReport(int days)
        {
            try
            {
                List<InventoryReportRow> data = _reportRepository.GetNearExpiry(days);
                return OperationResult<List<InventoryReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<InventoryReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<InventoryReportRow>> GetExpiredMedicinesReport()
        {
            try
            {
                List<InventoryReportRow> data = _reportRepository.GetExpiredMedicines();
                return OperationResult<List<InventoryReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<InventoryReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<StockMovementReportRow>> GetStockMovementReport(DateTime from, DateTime to)
        {
            try
            {
                List<StockMovementReportRow> data = _reportRepository.GetStockMovements(from, to);
                return OperationResult<List<StockMovementReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<StockMovementReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<BatchReportRow>> GetBatchReport(int? medicineId = null)
        {
            try
            {
                List<BatchReportRow> data = _reportRepository.GetBatchReport(medicineId);
                return OperationResult<List<BatchReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<BatchReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<MedicineReportRow>> GetMedicineList(int? categoryId = null)
        {
            try
            {
                List<MedicineReportRow> data = _reportRepository.GetMedicineList(categoryId);
                return OperationResult<List<MedicineReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<MedicineReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<CategorySummaryRow>> GetCategorySummary()
        {
            try
            {
                List<CategorySummaryRow> data = _reportRepository.GetCategorySummary();
                return OperationResult<List<CategorySummaryRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<CategorySummaryRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<TopSellingMedicineRow>> GetTopSellingMedicines(DateTime from, DateTime to, int topN = 10)
        {
            try
            {
                List<TopSellingMedicineRow> data = _reportRepository.GetTopSellingMedicines(from, to, topN);
                return OperationResult<List<TopSellingMedicineRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<TopSellingMedicineRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<SlowMovingMedicineRow>> GetSlowMovingMedicines(int thresholdDays)
        {
            try
            {
                List<SlowMovingMedicineRow> data = _reportRepository.GetSlowMovingMedicines(thresholdDays);
                return OperationResult<List<SlowMovingMedicineRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<SlowMovingMedicineRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<SupplierReportRow>> GetSupplierList()
        {
            try
            {
                List<SupplierReportRow> data = _reportRepository.GetSupplierList();
                return OperationResult<List<SupplierReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<SupplierReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<ProfitReportRow>> GetProfitReport(DateTime from, DateTime to)
        {
            try
            {
                List<ProfitReportRow> data = _reportRepository.GetProfitReport(from, to);
                return OperationResult<List<ProfitReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<ProfitReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<SalesReportRow>> GetRevenueReport(DateTime from, DateTime to)
        {
            try
            {
                List<SalesReportRow> data = _reportRepository.GetRevenueReport(from, to);
                return OperationResult<List<SalesReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<SalesReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<PurchaseReportRow>> GetPurchaseCostReport(DateTime from, DateTime to)
        {
            try
            {
                List<PurchaseReportRow> data = _reportRepository.GetPurchaseCostReport(from, to);
                return OperationResult<List<PurchaseReportRow>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<PurchaseReportRow>>.Failure(ex.Message);
            }
        }

        public OperationResult<DashboardSummary> GetDashboardSummary()
        {
            try
            {
                DashboardSummary data = _reportRepository.GetDashboardSummary();
                return OperationResult<DashboardSummary>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<DashboardSummary>.Failure(ex.Message);
            }
        }

        public OperationResult<List<MonthlySalesSummary>> GetMonthlySalesTrend(int months = 12)
        {
            try
            {
                List<MonthlySalesSummary> data = _reportRepository.GetMonthlySalesTrend(months);
                return OperationResult<List<MonthlySalesSummary>>.Success(data);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<MonthlySalesSummary>>.Failure(ex.Message);
            }
        }

        public OperationResult<byte[]> ExportToCsv<T>(List<T> data)
        {
            try
            {
                if (data == null || data.Count == 0)
                    return OperationResult<byte[]>.Failure("No data to export.");

                PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsvField(p.Name))));

                foreach (T item in data)
                {
                    IEnumerable<string> values = properties.Select(p =>
                    {
                        object value = p.GetValue(item);
                        return EscapeCsvField(FormatValue(value));
                    });
                    sb.AppendLine(string.Join(",", values));
                }

                byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
                return OperationResult<byte[]>.Success(bytes);
            }
            catch (Exception ex)
            {
                return OperationResult<byte[]>.Failure($"Failed to export CSV: {ex.Message}");
            }
        }

        public OperationResult<byte[]> ExportToExcel<T>(List<T> data)
        {
            try
            {
                if (data == null || data.Count == 0)
                    return OperationResult<byte[]>.Failure("No data to export.");

                PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                List<string> headers = properties.Select(p => p.Name).ToList();

                List<object[]> rows = data
                    .Select(item => properties.Select(p => p.GetValue(item)).ToArray())
                    .ToList();

                byte[] bytes = XlsxWriter.Write(typeof(T).Name, headers, rows);
                return OperationResult<byte[]>.Success(bytes);
            }
            catch (Exception ex)
            {
                return OperationResult<byte[]>.Failure($"Failed to export Excel: {ex.Message}");
            }
        }

        public OperationResult<byte[]> ExportToPdf<T>(string title, List<T> data)
        {
            try
            {
                if (data == null || data.Count == 0)
                    return OperationResult<byte[]>.Failure("No data to export.");

                PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                string[] headers = properties.Select(p => p.Name).ToArray();

                List<string[]> rows = data
                    .Select(item => properties.Select(p => FormatValue(p.GetValue(item))).ToArray())
                    .ToList();

                byte[] bytes = SmartMed.Reports.PdfExporter.ExportTable(title ?? typeof(T).Name, headers, rows);
                return OperationResult<byte[]>.Success(bytes);
            }
            catch (Exception ex)
            {
                return OperationResult<byte[]>.Failure($"Failed to export PDF: {ex.Message}");
            }
        }

        private static string EscapeCsvField(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";

            return value;
        }

        private static string FormatValue(object value)
        {
            if (value == null)
                return "";

            if (value is DateTime dt)
                return dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            if (value is decimal dec)
                return dec.ToString("F2", CultureInfo.InvariantCulture);

            return value.ToString();
        }
    }
}
