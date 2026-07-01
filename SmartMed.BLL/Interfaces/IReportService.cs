using System;
using System.Collections.Generic;
using SmartMed.Models.Reports;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface IReportService
    {
        OperationResult<List<DailySalesSummary>> GetDailySales(DateTime date);
        OperationResult<List<DailySalesSummary>> GetSalesByDateRange(DateTime from, DateTime to);
        OperationResult<List<MonthlySalesSummary>> GetMonthlySales(int year);
        OperationResult<List<SalesReportRow>> GetSalesReport(DateTime from, DateTime to, int? cashierId = null);
        OperationResult<List<SalesReportRow>> GetCustomerSales(int? customerId, DateTime from, DateTime to);
        OperationResult<List<PaymentSummaryRow>> GetPaymentSummary(DateTime from, DateTime to);
        OperationResult<List<PurchaseReportRow>> GetPurchaseReport(DateTime from, DateTime to, int? supplierId = null);
        OperationResult<List<PurchaseReportRow>> GetDailyPurchases(DateTime date);
        OperationResult<List<InventoryReportRow>> GetCurrentStockReport();
        OperationResult<List<InventoryReportRow>> GetLowStockReport(int threshold);
        OperationResult<List<InventoryReportRow>> GetNearExpiryReport(int days);
        OperationResult<List<InventoryReportRow>> GetExpiredMedicinesReport();
        OperationResult<List<StockMovementReportRow>> GetStockMovementReport(DateTime from, DateTime to);
        OperationResult<List<BatchReportRow>> GetBatchReport(int? medicineId = null);
        OperationResult<List<MedicineReportRow>> GetMedicineList(int? categoryId = null);
        OperationResult<List<CategorySummaryRow>> GetCategorySummary();
        OperationResult<List<TopSellingMedicineRow>> GetTopSellingMedicines(DateTime from, DateTime to, int topN = 10);
        OperationResult<List<SlowMovingMedicineRow>> GetSlowMovingMedicines(int thresholdDays);
        OperationResult<List<SupplierReportRow>> GetSupplierList();
        OperationResult<List<ProfitReportRow>> GetProfitReport(DateTime from, DateTime to);
        OperationResult<List<SalesReportRow>> GetRevenueReport(DateTime from, DateTime to);
        OperationResult<List<PurchaseReportRow>> GetPurchaseCostReport(DateTime from, DateTime to);
        OperationResult<DashboardSummary> GetDashboardSummary();
        OperationResult<List<MonthlySalesSummary>> GetMonthlySalesTrend(int months = 12);
        OperationResult<byte[]> ExportToCsv<T>(List<T> data);
        OperationResult<byte[]> ExportToExcel<T>(List<T> data);
    }
}
