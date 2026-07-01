using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SettingRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public List<Setting> GetAll()
        {
            const string sql = "SELECT Id, [Key], [Value], [Description], Category, IsSystem, IsActive, CreatedDate, UpdatedDate FROM Settings WHERE IsActive = 1 ORDER BY Category, [Key]";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Setting> results = new List<Setting>();
                        while (reader.Read())
                            results.Add(MapSetting(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve settings.", exception);
            }
        }

        public Setting GetById(int id)
        {
            const string sql = "SELECT Id, [Key], [Value], [Description], Category, IsSystem, IsActive, CreatedDate, UpdatedDate FROM Settings WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapSetting(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve setting by Id.", exception);
            }
            return null;
        }

        public Setting GetByKey(string key)
        {
            Guard.AgainstNullOrWhiteSpace(key, nameof(key));
            const string sql = "SELECT Id, [Key], [Value], [Description], Category, IsSystem, IsActive, CreatedDate, UpdatedDate FROM Settings WHERE [Key] = @Key";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Key", key);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapSetting(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve setting by key.", exception);
            }
            return null;
        }

        public List<Setting> GetByCategory(string category)
        {
            Guard.AgainstNullOrWhiteSpace(category, nameof(category));
            const string sql = "SELECT Id, [Key], [Value], [Description], Category, IsSystem, IsActive, CreatedDate, UpdatedDate FROM Settings WHERE Category = @Category AND IsActive = 1 ORDER BY [Key]";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Category", category);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Setting> results = new List<Setting>();
                        while (reader.Read())
                            results.Add(MapSetting(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve settings by category.", exception);
            }
        }

        public void Add(Setting setting)
        {
            Guard.AgainstNull(setting, nameof(setting));
            const string sql = "INSERT INTO Settings ([Key], [Value], [Description], Category, IsSystem, CreatedDate) VALUES (@Key, @Value, @Description, @Category, @IsSystem, GETUTCDATE())";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Key", setting.Key);
                    command.Parameters.AddWithValue("@Value", (object)setting.Value ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Description", (object)setting.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Category", setting.Category ?? "General");
                    command.Parameters.AddWithValue("@IsSystem", setting.IsSystem);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add setting.", exception);
            }
        }

        public void Update(Setting setting)
        {
            Guard.AgainstNull(setting, nameof(setting));
            const string sql = "UPDATE Settings SET [Value] = @Value, [Description] = @Description, Category = @Category, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", setting.Id);
                    command.Parameters.AddWithValue("@Value", (object)setting.Value ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Description", (object)setting.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Category", setting.Category ?? "General");
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update setting.", exception);
            }
        }

        public void Delete(int id)
        {
            const string sql = "UPDATE Settings SET IsActive = 0, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to delete setting.", exception);
            }
        }

        public bool KeyExists(string key, int? excludeId = null)
        {
            Guard.AgainstNullOrWhiteSpace(key, nameof(key));
            string sql = "SELECT COUNT(1) FROM Settings WHERE [Key] = @Key AND IsActive = 1";
            if (excludeId.HasValue)
                sql += " AND Id != @ExcludeId";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Key", key);
                    if (excludeId.HasValue)
                        command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
                    connection.Open();
                    return (int)command.ExecuteScalar() > 0;
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to check setting key existence.", exception);
            }
        }

        private static Setting MapSetting(SqlDataReader reader)
        {
            return new Setting
            {
                Id = (int)reader["Id"],
                Key = (string)reader["Key"],
                Value = reader["Value"] == DBNull.Value ? null : (string)reader["Value"],
                Description = reader["Description"] == DBNull.Value ? null : (string)reader["Description"],
                Category = (string)reader["Category"],
                IsSystem = (bool)reader["IsSystem"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };
        }
    }
}
