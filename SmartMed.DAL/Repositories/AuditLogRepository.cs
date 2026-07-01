using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;

namespace SmartMed.DAL.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AuditLogRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public void LogLogin(int userId, string username, string machineName)
        {
            Guard.AgainstNullOrWhiteSpace(username, nameof(username));
            Guard.AgainstNullOrWhiteSpace(machineName, nameof(machineName));

            const string sql = "INSERT INTO AuditLogs (UserId, Username, Action, MachineName, Timestamp, Details) " +
                               "VALUES (@UserId, @Username, 1, @MachineName, @Timestamp, NULL)";

            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@MachineName", machineName);
                    command.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to log login audit entry.", exception);
            }
        }

        public void LogLogout(int? userId, string username, string machineName)
        {
            Guard.AgainstNullOrWhiteSpace(username, nameof(username));
            Guard.AgainstNullOrWhiteSpace(machineName, nameof(machineName));

            const string sql = "INSERT INTO AuditLogs (UserId, Username, Action, MachineName, Timestamp, Details) " +
                               "VALUES (@UserId, @Username, 2, @MachineName, @Timestamp, NULL)";

            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", (object)userId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@MachineName", machineName);
                    command.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to log logout audit entry.", exception);
            }
        }

        public void Log(int? userId, string username, AuditAction action, string machineName, string details)
        {
            Guard.AgainstNullOrWhiteSpace(username, nameof(username));
            Guard.AgainstNullOrWhiteSpace(machineName, nameof(machineName));

            const string sql = "INSERT INTO AuditLogs (UserId, Username, Action, MachineName, Timestamp, Details) " +
                               "VALUES (@UserId, @Username, @Action, @MachineName, @Timestamp, @Details)";

            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", (object)userId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Action", (int)action);
                    command.Parameters.AddWithValue("@MachineName", machineName);
                    command.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Details", details ?? (object)DBNull.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to log audit entry.", exception);
            }
        }

        public void LogFailedAttempt(string username, string machineName, string details)
        {
            Guard.AgainstNullOrWhiteSpace(username, nameof(username));
            Guard.AgainstNullOrWhiteSpace(machineName, nameof(machineName));

            const string sql = "INSERT INTO AuditLogs (UserId, Username, Action, MachineName, Timestamp, Details) " +
                               "VALUES (NULL, @Username, 3, @MachineName, @Timestamp, @Details)";

            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@MachineName", machineName);
                    command.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Details", details ?? (object)DBNull.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to log failed login attempt.", exception);
            }
        }

        public List<AuditLogEntry> GetAll(int limit = 100)
        {
            const string sql = "SELECT TOP (@Limit) Id, UserId, Username, Action, MachineName, Timestamp, Details FROM AuditLogs ORDER BY Timestamp DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Limit", limit);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<AuditLogEntry> results = new List<AuditLogEntry>();
                        while (reader.Read())
                            results.Add(MapAuditLogEntry(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve audit logs.", exception);
            }
        }

        public List<AuditLogEntry> GetByDateRange(DateTime from, DateTime to)
        {
            const string sql = "SELECT Id, UserId, Username, Action, MachineName, Timestamp, Details FROM AuditLogs WHERE Timestamp >= @From AND Timestamp <= @To ORDER BY Timestamp DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@From", from);
                    command.Parameters.AddWithValue("@To", to);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<AuditLogEntry> results = new List<AuditLogEntry>();
                        while (reader.Read())
                            results.Add(MapAuditLogEntry(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve audit logs by date range.", exception);
            }
        }

        public List<AuditLogEntry> GetByAction(AuditAction action)
        {
            const string sql = "SELECT Id, UserId, Username, Action, MachineName, Timestamp, Details FROM AuditLogs WHERE Action = @Action ORDER BY Timestamp DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Action", (int)action);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<AuditLogEntry> results = new List<AuditLogEntry>();
                        while (reader.Read())
                            results.Add(MapAuditLogEntry(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve audit logs by action.", exception);
            }
        }

        public List<AuditLogEntry> GetByUser(int? userId)
        {
            const string sql = "SELECT Id, UserId, Username, Action, MachineName, Timestamp, Details FROM AuditLogs WHERE (@UserId IS NULL OR UserId = @UserId) ORDER BY Timestamp DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", (object)userId ?? DBNull.Value);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<AuditLogEntry> results = new List<AuditLogEntry>();
                        while (reader.Read())
                            results.Add(MapAuditLogEntry(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve audit logs by user.", exception);
            }
        }

        public List<AuditLogEntry> Search(string keyword, int limit = 50)
        {
            Guard.AgainstNullOrWhiteSpace(keyword, nameof(keyword));
            const string sql = "SELECT TOP (@Limit) Id, UserId, Username, Action, MachineName, Timestamp, Details FROM AuditLogs WHERE Username LIKE @Keyword OR Details LIKE @Keyword ORDER BY Timestamp DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                    command.Parameters.AddWithValue("@Limit", limit);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<AuditLogEntry> results = new List<AuditLogEntry>();
                        while (reader.Read())
                            results.Add(MapAuditLogEntry(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to search audit logs.", exception);
            }
        }

        private static AuditLogEntry MapAuditLogEntry(SqlDataReader reader)
        {
            return new AuditLogEntry
            {
                Id = (int)reader["Id"],
                UserId = reader["UserId"] == DBNull.Value ? null : (int?)reader["UserId"],
                Username = (string)reader["Username"],
                Action = (AuditAction)(int)reader["Action"],
                MachineName = (string)reader["MachineName"],
                Timestamp = (DateTime)reader["Timestamp"],
                Details = reader["Details"] == DBNull.Value ? null : (string)reader["Details"]
            };
        }
    }
}
