namespace SmartMed.Models.Reports
{
    public class CategorySummaryRow
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int MedicineCount { get; set; }
        public int TotalStockQuantity { get; set; }
        public decimal TotalStockValue { get; set; }
    }
}
