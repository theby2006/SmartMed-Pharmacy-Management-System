using System.Collections.Generic;
using System.Data;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface IOrderItemRepository : IRepository
    {
        List<OrderItem> GetByOrderId(int orderId);

        void AddRange(List<OrderItem> items, IDbConnection connection, IDbTransaction transaction);
    }
}
