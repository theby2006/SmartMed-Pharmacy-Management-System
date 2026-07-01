using System.Collections.Generic;
using System.Data;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface IMedicineRepository : IRepository
    {
        Medicine GetById(int id);
        List<Medicine> GetAll();
        List<Medicine> GetByCategoryId(int categoryId);
        List<Medicine> Search(string keyword);
        List<Medicine> GetLowStock();
        List<Medicine> GetNearExpiry(int thresholdDays);
        Medicine GetByNameAndBrand(string name, string brand);
        int Add(Medicine medicine);
        void Update(Medicine medicine);
        void Delete(int id);
        void SetStockQuantity(int medicineId, int quantity);
        void SetStockQuantity(int medicineId, int quantity, IDbConnection connection, IDbTransaction transaction);
        void UpdateStockQuantity(int medicineId, int delta);
        void UpdateStockQuantity(int medicineId, int delta, IDbConnection connection, IDbTransaction transaction);
    }
}
