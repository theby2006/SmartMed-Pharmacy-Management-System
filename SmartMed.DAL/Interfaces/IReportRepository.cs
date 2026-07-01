using System;
using System.Collections.Generic;
using SmartMed.Models.Reports;

namespace SmartMed.DAL.Interfaces
{
    public interface IReportRepository : IRepository
    {
        List<DailySalesSummary> GetDailySales(DateTime date);
        List<DailySalesSummary> GetSalesByDateRange(DateTime from, DateTime to);
        List<MonthlySalesSummary> GetMonthlySales(int year);
        List<SalesReportRow> GetSalesByCashier(int cashierId, DateTime from, DateTime to);
        List<SalesReportRow> GetCustomerSales(int? customerId, DateTime from, DateTime to);
        List<PaymentSummaryRow> GetPaymentSummary(DateTime from, DateTime to);
        List<PurchaseReportRow> GetDailyPurchases(DateTime date);
        List<PurchaseReportRow> GetPurchasesBySupplier(int supplierId, DateTime from, DateTime to);
        List<PurchaseReportRow> GetPurchaseHistory(DateTime from, DateTime to);
        List<InventoryReportRow> GetCurrentStock();
        List<InventoryReportRow> GetLowStock(int threshold);
        List<InventoryReportRow> GetNearExpiry(int days);
        List<InventoryReportRow> GetExpiredMedicines();
        List<StockMovementReportRow> GetStockMovements(DateTime from, DateTime to);
        List<BatchReportRow> GetBatchReport(int? medicineId);
        List<MedicineReportRow> GetMedicineList(int? categoryId);
        List<CategorySummaryRow> GetCategorySummary();
        List<TopSellingMedicineRow> GetTopSellingMedicines(DateTime from, DateTime to, int topN);
        List<SlowMovingMedicineRow> GetSlowMovingMedicines(int thresholdDays);
        List<SupplierReportRow> GetSupplierList();
        List<ProfitReportRow> GetProfitReport(DateTime from, DateTime to);
        List<SalesReportRow> GetRevenueReport(DateTime from, DateTime to);
        List<PurchaseReportRow> GetPurchaseCostReport(DateTime from, DateTime to);
        DashboardSummary GetDashboardSummary();
        List<MonthlySalesSummary> GetMonthlySalesTrend(int months);
    }
}
