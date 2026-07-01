namespace SmartMed.Models.Reports
{
    public class PaymentSummaryRow
    {
        public string PaymentMethod { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }
}
