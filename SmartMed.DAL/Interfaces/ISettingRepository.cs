using System.Collections.Generic;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface ISettingRepository : IRepository
    {
        List<Setting> GetAll();
        Setting GetById(int id);
        Setting GetByKey(string key);
        List<Setting> GetByCategory(string category);
        void Add(Setting setting);
        void Update(Setting setting);
        void Delete(int id);
        bool KeyExists(string key, int? excludeId = null);
    }
}
