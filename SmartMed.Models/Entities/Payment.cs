using SmartMed.Models.Common;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Entities
{
    public class Payment : BaseEntity
    {
        public int SaleId { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public decimal AmountPaid { get; set; }
        public decimal ChangeAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;
        public string TransactionReference { get; set; }
        public int ProcessedByUserId { get; set; }
    }
}
