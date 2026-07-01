using System;
using System.Collections.Generic;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class SettingService : ISettingService
    {
        private readonly ISettingRepository _settingRepository;

        public SettingService(ISettingRepository settingRepository)
        {
            Guard.AgainstNull(settingRepository, nameof(settingRepository));
            _settingRepository = settingRepository;
        }

        public OperationResult<List<Setting>> GetAllSettings()
        {
            try
            {
                return OperationResult<List<Setting>>.Success(_settingRepository.GetAll());
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<Setting>>.Failure(ex.Message);
            }
        }

        public OperationResult<Setting> GetSettingByKey(string key)
        {
            try
            {
                Setting setting = _settingRepository.GetByKey(key);
                return setting != null
                    ? OperationResult<Setting>.Success(setting)
                    : OperationResult<Setting>.Failure($"Setting '{key}' not found.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult<Setting>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Setting>> GetSettingsByCategory(string category)
        {
            try
            {
                return OperationResult<List<Setting>>.Success(_settingRepository.GetByCategory(category));
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<Setting>>.Failure(ex.Message);
            }
        }

        public OperationResult<string> GetValue(string key, string defaultValue = null)
        {
            try
            {
                Setting setting = _settingRepository.GetByKey(key);
                return OperationResult<string>.Success(setting?.Value ?? defaultValue);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<string>.Failure(ex.Message);
            }
        }

        public OperationResult<int> GetIntValue(string key, int defaultValue = 0)
        {
            try
            {
                Setting setting = _settingRepository.GetByKey(key);
                if (setting != null && int.TryParse(setting.Value, out int value))
                    return OperationResult<int>.Success(value);
                return OperationResult<int>.Success(defaultValue);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<int>.Failure(ex.Message);
            }
        }

        public OperationResult<bool> GetBoolValue(string key, bool defaultValue = false)
        {
            try
            {
                Setting setting = _settingRepository.GetByKey(key);
                if (setting != null && bool.TryParse(setting.Value, out bool value))
                    return OperationResult<bool>.Success(value);
                return OperationResult<bool>.Success(defaultValue);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<bool>.Failure(ex.Message);
            }
        }

        public OperationResult<Setting> AddSetting(string key, string value, string description, string category, bool isSystem = false)
        {
            try
            {
                if (_settingRepository.KeyExists(key))
                    return OperationResult<Setting>.Failure($"A setting with key '{key}' already exists.");

                Setting setting = new Setting
                {
                    Key = key,
                    Value = value,
                    Description = description,
                    Category = category ?? "General",
                    IsSystem = isSystem
                };
                _settingRepository.Add(setting);
                Setting created = _settingRepository.GetByKey(key);
                return OperationResult<Setting>.Success(created);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<Setting>.Failure(ex.Message);
            }
        }

        public OperationResult UpdateSetting(int id, string value)
        {
            try
            {
                Setting setting = _settingRepository.GetById(id);
                if (setting == null)
                    return OperationResult.Failure($"Setting with ID {id} not found.");
                if (setting.IsSystem)
                    return OperationResult.Failure("System settings cannot be modified.");

                setting.Value = value;
                _settingRepository.Update(setting);
                return OperationResult.Success("Setting updated successfully.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult UpdateSetting(string key, string value)
        {
            try
            {
                Setting setting = _settingRepository.GetByKey(key);
                if (setting == null)
                    return OperationResult.Failure($"Setting '{key}' not found.");
                if (setting.IsSystem)
                    return OperationResult.Failure("System settings cannot be modified.");

                setting.Value = value;
                _settingRepository.Update(setting);
                return OperationResult.Success("Setting updated successfully.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult DeleteSetting(int id)
        {
            try
            {
                Setting setting = _settingRepository.GetById(id);
                if (setting == null)
                    return OperationResult.Failure($"Setting with ID {id} not found.");
                if (setting.IsSystem)
                    return OperationResult.Failure("System settings cannot be deleted.");

                _settingRepository.Delete(id);
                return OperationResult.Success("Setting deleted successfully.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }
    }
}
