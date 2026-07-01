using System.Collections.Generic;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface ISupplierRepository : IRepository
    {
        List<Supplier> GetAll();
        Supplier GetById(int id);
        Supplier GetBySupplierCode(string supplierCode);
        Supplier GetByName(string supplierName);
        List<Supplier> Search(string keyword);
        int Add(Supplier supplier);
        void Update(Supplier supplier);
        void Delete(int id);
        bool HasPurchases(int supplierId);
    }
}
