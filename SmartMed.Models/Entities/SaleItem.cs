using System;
using SmartMed.Models.Common;

namespace SmartMed.Models.Entities
{
    public class SaleItem : BaseEntity
    {
        public int SaleId { get; set; }
        public int MedicineId { get; set; }
        public int StockBatchId { get; set; }
        public string BatchNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal LineTotal { get; set; }
    }
}
