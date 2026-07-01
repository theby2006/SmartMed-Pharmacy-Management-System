using SmartMed.BLL.Interfaces;

namespace SmartMed.BLL.Services
{
    public class PricingService : IPricingService
    {
        public decimal CalculateSubTotal(int quantity, decimal unitPrice)
        {
            return quantity * unitPrice;
        }

        public decimal CalculateLineTotal(int quantity, decimal sellingPrice, decimal discountPercent, decimal taxPercent)
        {
            decimal subTotal = quantity * sellingPrice;
            decimal discount = subTotal * (discountPercent / 100m);
            decimal afterDiscount = subTotal - discount;
            decimal tax = afterDiscount * (taxPercent / 100m);
            return afterDiscount + tax;
        }

        public decimal CalculateDiscountAmount(decimal subTotal, decimal discountPercent)
        {
            return subTotal * (discountPercent / 100m);
        }

        public decimal CalculateTaxAmount(decimal amountAfterDiscount, decimal taxPercent)
        {
            return amountAfterDiscount * (taxPercent / 100m);
        }

        public decimal CalculateGrandTotal(decimal subTotal, decimal discountAmount, decimal taxAmount)
        {
            return subTotal - discountAmount + taxAmount;
        }

        public decimal CalculateChange(decimal amountPaid, decimal grandTotal)
        {
            return amountPaid - grandTotal;
        }
    }
}
