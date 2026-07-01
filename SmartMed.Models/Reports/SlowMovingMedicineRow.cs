namespace SmartMed.Models.Reports
{
    public class SlowMovingMedicineRow
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; }
        public string CategoryName { get; set; }
        public int StockQuantity { get; set; }
        public int DaysSinceLastSale { get; set; }
    }
}
