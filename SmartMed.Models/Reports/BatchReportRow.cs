using System;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Reports
{
    public class BatchReportRow
    {
        public int BatchId { get; set; }
        public string BatchNumber { get; set; }
        public string MedicineName { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public BatchStatus BatchStatus { get; set; }
    }
}
