using System;
using SmartMed.Models.Common;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Entities
{
    public class Sale : BaseEntity
    {
        public string SaleNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public int CashierId { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public SaleStatus Status { get; set; } = SaleStatus.Pending;
        public string Notes { get; set; }
    }
}
