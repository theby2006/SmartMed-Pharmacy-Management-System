using System;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Reports
{
    public class SalesReportRow
    {
        public int SaleId { get; set; }
        public string SaleNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public string CashierName { get; set; }
        public string CustomerName { get; set; }
        public int ItemCount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string PaymentMethod { get; set; }
        public SaleStatus Status { get; set; }
    }
}
