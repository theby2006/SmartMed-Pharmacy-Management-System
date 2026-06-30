using System;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;

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
    }
}
