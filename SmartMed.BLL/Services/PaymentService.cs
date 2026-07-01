using System.Data;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            Guard.AgainstNull(paymentRepository, nameof(paymentRepository));
            _paymentRepository = paymentRepository;
        }

        public OperationResult<Payment> GetPaymentBySaleId(int saleId)
        {
            try
            {
                Payment payment = _paymentRepository.GetBySaleId(saleId);
                return payment != null
                    ? OperationResult<Payment>.Success(payment)
                    : OperationResult<Payment>.Failure($"No payment found for sale ID {saleId}.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult<Payment>.Failure(ex.Message);
            }
        }

        public OperationResult<int> ProcessPayment(Payment payment)
        {
            return ProcessPayment(payment, null, null);
        }

        public OperationResult<int> ProcessPayment(Payment payment, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                if (payment.AmountPaid <= 0)
                    return OperationResult<int>.Failure("Amount paid must be greater than zero.");

                if (payment.ChangeAmount < 0)
                    return OperationResult<int>.Failure("Change amount cannot be negative.");

                int paymentId = (connection != null && transaction != null)
                    ? _paymentRepository.Add(payment, connection, transaction)
                    : _paymentRepository.Add(payment);

                return OperationResult<int>.Success(paymentId);
            }
            catch (ValidationException ex)
            {
                return OperationResult<int>.Failure(ex.Message);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<int>.Failure(ex.Message);
            }
        }
    }
}
