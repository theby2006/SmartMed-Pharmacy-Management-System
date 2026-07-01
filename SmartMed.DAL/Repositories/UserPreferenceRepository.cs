using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Repositories
{
    public class UserPreferenceRepository : IUserPreferenceRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserPreferenceRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public List<UserPreference> GetByUserId(int userId)
        {
            const string sql = "SELECT Id, UserId, [Key], [Value], IsActive, CreatedDate, UpdatedDate FROM UserPreferences WHERE UserId = @UserId AND IsActive = 1";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<UserPreference> results = new List<UserPreference>();
                        while (reader.Read())
                            results.Add(MapUserPreference(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve user preferences.", exception);
            }
        }

        public UserPreference GetByUserAndKey(int userId, string key)
        {
            Guard.AgainstNullOrWhiteSpace(key, nameof(key));
            const string sql = "SELECT Id, UserId, [Key], [Value], IsActive, CreatedDate, UpdatedDate FROM UserPreferences WHERE UserId = @UserId AND [Key] = @Key AND IsActive = 1";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Key", key);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapUserPreference(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve user preference.", exception);
            }
            return null;
        }

        public void SetPreference(int userId, string key, string value)
        {
            Guard.AgainstNullOrWhiteSpace(key, nameof(key));
            const string sql = "MERGE UserPreferences AS target USING (SELECT @UserId AS UserId, @Key AS [Key]) AS source " +
                               "ON target.UserId = source.UserId AND target.[Key] = source.[Key] " +
                               "WHEN MATCHED THEN UPDATE SET [Value] = @Value, UpdatedDate = GETUTCDATE() " +
                               "WHEN NOT MATCHED THEN INSERT (UserId, [Key], [Value], CreatedDate) VALUES (@UserId, @Key, @Value, GETUTCDATE());";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Key", key);
                    command.Parameters.AddWithValue("@Value", (object)value ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to set user preference.", exception);
            }
        }

        public void DeletePreference(int userId, string key)
        {
            Guard.AgainstNullOrWhiteSpace(key, nameof(key));
            const string sql = "DELETE FROM UserPreferences WHERE UserId = @UserId AND [Key] = @Key";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Key", key);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to delete user preference.", exception);
            }
        }

        private static UserPreference MapUserPreference(SqlDataReader reader)
        {
            return new UserPreference
            {
                Id = (int)reader["Id"],
                UserId = (int)reader["UserId"],
                Key = (string)reader["Key"],
                Value = reader["Value"] == DBNull.Value ? null : (string)reader["Value"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };
        }
    }
}
