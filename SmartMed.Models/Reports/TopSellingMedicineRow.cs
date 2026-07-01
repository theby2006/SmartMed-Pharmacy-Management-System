namespace SmartMed.Models.Reports
{
    public class TopSellingMedicineRow
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; }
        public string CategoryName { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int Rank { get; set; }
    }
}
