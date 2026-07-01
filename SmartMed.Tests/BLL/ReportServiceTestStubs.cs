using System;
using System.Collections.Generic;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Enums;
using SmartMed.Models.Reports;

namespace SmartMed.Tests.BLL
{
    internal class MockReportRepository : IReportRepository
    {
        public List<DailySalesSummary> DailySales { get; set; } = new List<DailySalesSummary>();
        public List<MonthlySalesSummary> MonthlySales { get; set; } = new List<MonthlySalesSummary>();
        public List<SalesReportRow> SalesReportRows { get; set; } = new List<SalesReportRow>();
        public List<PaymentSummaryRow> PaymentSummaries { get; set; } = new List<PaymentSummaryRow>();
        public List<PurchaseReportRow> PurchaseReportRows { get; set; } = new List<PurchaseReportRow>();
        public List<InventoryReportRow> InventoryRows { get; set; } = new List<InventoryReportRow>();
        public List<StockMovementReportRow> StockMovements { get; set; } = new List<StockMovementReportRow>();
        public List<BatchReportRow> BatchRows { get; set; } = new List<BatchReportRow>();
        public List<MedicineReportRow> MedicineRows { get; set; } = new List<MedicineReportRow>();
        public List<CategorySummaryRow> CategorySummaries { get; set; } = new List<CategorySummaryRow>();
        public List<TopSellingMedicineRow> TopSellingRows { get; set; } = new List<TopSellingMedicineRow>();
        public List<SlowMovingMedicineRow> SlowMovingRows { get; set; } = new List<SlowMovingMedicineRow>();
        public List<SupplierReportRow> SupplierRows { get; set; } = new List<SupplierReportRow>();
        public List<ProfitReportRow> ProfitRows { get; set; } = new List<ProfitReportRow>();
        public DashboardSummary Dashboard { get; set; } = new DashboardSummary();

        public List<DailySalesSummary> GetDailySales(DateTime date) => DailySales;
        public List<DailySalesSummary> GetSalesByDateRange(DateTime from, DateTime to) => DailySales;
        public List<MonthlySalesSummary> GetMonthlySales(int year) => MonthlySales;
        public List<SalesReportRow> GetSalesByCashier(int cashierId, DateTime from, DateTime to) => SalesReportRows;
        public List<SalesReportRow> GetCustomerSales(int? customerId, DateTime from, DateTime to) => SalesReportRows;
        public List<PaymentSummaryRow> GetPaymentSummary(DateTime from, DateTime to) => PaymentSummaries;
        public List<PurchaseReportRow> GetDailyPurchases(DateTime date) => PurchaseReportRows;
        public List<PurchaseReportRow> GetPurchasesBySupplier(int supplierId, DateTime from, DateTime to) => PurchaseReportRows;
        public List<PurchaseReportRow> GetPurchaseHistory(DateTime from, DateTime to) => PurchaseReportRows;
        public List<InventoryReportRow> GetCurrentStock() => InventoryRows;
        public List<InventoryReportRow> GetLowStock(int threshold) => InventoryRows;
        public List<InventoryReportRow> GetNearExpiry(int days) => InventoryRows;
        public List<InventoryReportRow> GetExpiredMedicines() => InventoryRows;
        public List<StockMovementReportRow> GetStockMovements(DateTime from, DateTime to) => StockMovements;
        public List<BatchReportRow> GetBatchReport(int? medicineId) => BatchRows;
        public List<MedicineReportRow> GetMedicineList(int? categoryId) => MedicineRows;
        public List<CategorySummaryRow> GetCategorySummary() => CategorySummaries;
        public List<TopSellingMedicineRow> GetTopSellingMedicines(DateTime from, DateTime to, int topN) => TopSellingRows;
        public List<SlowMovingMedicineRow> GetSlowMovingMedicines(int thresholdDays) => SlowMovingRows;
        public List<SupplierReportRow> GetSupplierList() => SupplierRows;
        public List<ProfitReportRow> GetProfitReport(DateTime from, DateTime to) => ProfitRows;
        public List<SalesReportRow> GetRevenueReport(DateTime from, DateTime to) => SalesReportRows;
        public List<PurchaseReportRow> GetPurchaseCostReport(DateTime from, DateTime to) => PurchaseReportRows;
        public DashboardSummary GetDashboardSummary() => Dashboard;
        public List<MonthlySalesSummary> GetMonthlySalesTrend(int months) => MonthlySales;
    }
}
