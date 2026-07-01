namespace SmartMed.BLL.Interfaces
{
    public interface IPricingService
    {
        decimal CalculateSubTotal(int quantity, decimal unitPrice);

        decimal CalculateLineTotal(int quantity, decimal sellingPrice, decimal discountPercent, decimal taxPercent);

        decimal CalculateDiscountAmount(decimal subTotal, decimal discountPercent);

        decimal CalculateTaxAmount(decimal amountAfterDiscount, decimal taxPercent);

        decimal CalculateGrandTotal(decimal subTotal, decimal discountAmount, decimal taxAmount);

        decimal CalculateChange(decimal amountPaid, decimal grandTotal);
    }
}
