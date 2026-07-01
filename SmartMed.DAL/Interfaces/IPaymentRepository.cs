using System.Data;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface IPaymentRepository : IRepository
    {
        Payment GetById(int id);

        Payment GetBySaleId(int saleId);

        int Add(Payment payment);

        int Add(Payment payment, IDbConnection connection, IDbTransaction transaction);
    }
}
