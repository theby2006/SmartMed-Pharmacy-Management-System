using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Repositories
{
    public class PerformanceLogRepository : IPerformanceLogRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PerformanceLogRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public void Add(PerformanceLog entry)
        {
            Guard.AgainstNull(entry, nameof(entry));
            const string sql = "INSERT INTO PerformanceLogs (OperationName, DurationMs, IsSlow, MachineName, CreatedDate) VALUES (@OperationName, @DurationMs, @IsSlow, @MachineName, GETUTCDATE())";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@OperationName", entry.OperationName);
                    command.Parameters.AddWithValue("@DurationMs", entry.DurationMs);
                    command.Parameters.AddWithValue("@IsSlow", entry.IsSlow);
                    command.Parameters.AddWithValue("@MachineName", (object)entry.MachineName ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add performance log entry.", exception);
            }
        }

        public List<PerformanceLog> GetRecentSlowOperations(int thresholdMs, int limit = 20)
        {
            const string sql = "SELECT TOP (@Limit) Id, OperationName, DurationMs, IsSlow, MachineName, CreatedDate FROM PerformanceLogs WHERE DurationMs >= @ThresholdMs ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ThresholdMs", thresholdMs);
                    command.Parameters.AddWithValue("@Limit", limit);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<PerformanceLog> results = new List<PerformanceLog>();
                        while (reader.Read())
                            results.Add(MapPerformanceLog(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve slow operations.", exception);
            }
        }

        public List<PerformanceLog> GetByOperation(string operationName, int limit = 50)
        {
            Guard.AgainstNullOrWhiteSpace(operationName, nameof(operationName));
            const string sql = "SELECT TOP (@Limit) Id, OperationName, DurationMs, IsSlow, MachineName, CreatedDate FROM PerformanceLogs WHERE OperationName = @OperationName ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@OperationName", operationName);
                    command.Parameters.AddWithValue("@Limit", limit);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<PerformanceLog> results = new List<PerformanceLog>();
                        while (reader.Read())
                            results.Add(MapPerformanceLog(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve performance logs by operation.", exception);
            }
        }

        public double GetAverageDuration(string operationName, DateTime since)
        {
            const string sql = "SELECT AVG(CAST(DurationMs AS FLOAT)) FROM PerformanceLogs WHERE OperationName = @OperationName AND CreatedDate >= @Since";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@OperationName", operationName);
                    command.Parameters.AddWithValue("@Since", since);
                    connection.Open();
                    object result = command.ExecuteScalar();
                    return result == DBNull.Value ? 0 : (double)result;
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to calculate average duration.", exception);
            }
        }

        private static PerformanceLog MapPerformanceLog(SqlDataReader reader)
        {
            return new PerformanceLog
            {
                Id = (int)reader["Id"],
                OperationName = (string)reader["OperationName"],
                DurationMs = (int)reader["DurationMs"],
                IsSlow = (bool)reader["IsSlow"],
                MachineName = reader["MachineName"] == DBNull.Value ? null : (string)reader["MachineName"],
                CreatedDate = (DateTime)reader["CreatedDate"]
            };
        }
    }
}
