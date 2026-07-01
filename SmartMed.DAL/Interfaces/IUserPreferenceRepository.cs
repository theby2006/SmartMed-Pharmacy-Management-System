using System.Collections.Generic;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface IUserPreferenceRepository : IRepository
    {
        List<UserPreference> GetByUserId(int userId);
        UserPreference GetByUserAndKey(int userId, string key);
        void SetPreference(int userId, string key, string value);
        void DeletePreference(int userId, string key);
    }
}
