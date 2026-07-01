using System;

namespace SmartMed.Models.Reports
{
    public class SupplierReportRow
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public decimal TotalPurchases { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public int PurchaseCount { get; set; }
    }
}
