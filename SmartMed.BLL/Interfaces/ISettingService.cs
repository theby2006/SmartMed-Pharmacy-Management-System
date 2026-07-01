using System.Collections.Generic;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface ISettingService
    {
        OperationResult<List<Setting>> GetAllSettings();
        OperationResult<Setting> GetSettingByKey(string key);
        OperationResult<List<Setting>> GetSettingsByCategory(string category);
        OperationResult<string> GetValue(string key, string defaultValue = null);
        OperationResult<int> GetIntValue(string key, int defaultValue = 0);
        OperationResult<bool> GetBoolValue(string key, bool defaultValue = false);
        OperationResult<Setting> AddSetting(string key, string value, string description, string category, bool isSystem = false);
        OperationResult UpdateSetting(int id, string value);
        OperationResult UpdateSetting(string key, string value);
        OperationResult DeleteSetting(int id);
    }
}
