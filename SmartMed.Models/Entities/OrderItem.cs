using SmartMed.Models.Common;

namespace SmartMed.Models.Entities
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }

        public int MedicineId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal DiscountPercent { get; set; }

        public decimal LineTotal { get; set; }
    }
}
