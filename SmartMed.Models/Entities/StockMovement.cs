using System;
using SmartMed.Models.Common;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Entities
{
    public class StockMovement : BaseEntity
    {
        public int StockBatchId { get; set; }

        public int MedicineId { get; set; }

        public MovementType MovementType { get; set; }

        public int Quantity { get; set; }

        public string ReferenceType { get; set; }

        public int ReferenceId { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalAmount { get; set; }

        public int CreatedByUserId { get; set; }

        public string Notes { get; set; }
    }
}
