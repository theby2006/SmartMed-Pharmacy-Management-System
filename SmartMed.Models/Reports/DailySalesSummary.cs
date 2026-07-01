using System;

namespace SmartMed.Models.Reports
{
    public class DailySalesSummary
    {
        public DateTime Date { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal AverageTransactionValue { get; set; }
    }
}
