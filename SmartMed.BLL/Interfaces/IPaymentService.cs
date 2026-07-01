using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface IPaymentService
    {
        OperationResult<Payment> GetPaymentBySaleId(int saleId);

        OperationResult<int> ProcessPayment(Payment payment);
    }
}
