using System.Collections.Generic;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface IMedicineCategoryRepository : IRepository
    {
        List<MedicineCategory> GetAll();
        MedicineCategory GetById(int id);
        MedicineCategory GetByName(string name);
        int Add(MedicineCategory category);
        void Update(MedicineCategory category);
        void Delete(int id);
    }
}
