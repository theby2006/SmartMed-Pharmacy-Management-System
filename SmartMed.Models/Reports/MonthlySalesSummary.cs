namespace SmartMed.Models.Reports
{
    public class MonthlySalesSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
    }
}
