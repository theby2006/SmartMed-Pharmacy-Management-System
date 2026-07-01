namespace SmartMed.Models.Reports
{
    public class DashboardSummary
    {
        public int TodaySalesCount { get; set; }
        public decimal TodaySalesRevenue { get; set; }
        public decimal TodayProfit { get; set; }
        public int TotalMedicines { get; set; }
        public int LowStockCount { get; set; }
        public int ExpiredCount { get; set; }
        public int NearExpiryCount { get; set; }
        public decimal TotalSalesMonth { get; set; }
        public decimal TotalPurchasesMonth { get; set; }
        public int TotalActiveSuppliers { get; set; }
    }
}
