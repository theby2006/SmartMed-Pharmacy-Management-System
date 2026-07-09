using System;
using SmartMed.Models.Common;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Entities
{
    public class Medicine : BaseEntity
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public DosageForm DosageForm { get; set; }
        public string Strength { get; set; }
        public string Unit { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public string PromotionLabel { get; set; }
        public bool RequiresPrescription { get; set; }
    }
}
