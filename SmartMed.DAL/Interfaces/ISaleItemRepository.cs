using System.Collections.Generic;
using System.Data;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface ISaleItemRepository : IRepository
    {
        List<SaleItem> GetBySaleId(int saleId);

        int Add(SaleItem item);

        int Add(SaleItem item, IDbConnection connection, IDbTransaction transaction);

        void AddRange(List<SaleItem> items);

        void AddRange(List<SaleItem> items, IDbConnection connection, IDbTransaction transaction);
    }
}
