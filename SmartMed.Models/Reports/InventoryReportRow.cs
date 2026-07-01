using System;

namespace SmartMed.Models.Reports
{
    public class InventoryReportRow
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; }
        public string CategoryName { get; set; }
        public string BatchNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int CurrentQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int DaysUntilExpiry { get; set; }
    }
}
