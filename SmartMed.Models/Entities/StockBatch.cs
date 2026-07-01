using System;
using SmartMed.Models.Common;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Entities
{
    public class StockBatch : BaseEntity
    {
        public int MedicineId { get; set; }

        public string BatchNumber { get; set; }

        public DateTime ExpiryDate { get; set; }

        public decimal PurchasePrice { get; set; }

        public decimal SellingPrice { get; set; }

        public int CurrentQuantity { get; set; }

        public int InitialQuantity { get; set; }

        public int PurchaseItemId { get; set; }

        public BatchStatus Status { get; set; } = BatchStatus.Active;
    }
}
