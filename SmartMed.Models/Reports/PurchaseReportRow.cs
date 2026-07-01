using System;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Reports
{
    public class PurchaseReportRow
    {
        public int PurchaseId { get; set; }
        public string PurchaseNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string SupplierName { get; set; }
        public string InvoiceNumber { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public PurchaseStatus Status { get; set; }
        public string CreatedBy { get; set; }
    }
}
