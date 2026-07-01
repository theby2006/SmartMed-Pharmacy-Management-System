using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Repositories
{
    public class ErrorLogRepository : IErrorLogRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ErrorLogRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public List<ErrorLog> GetAll(int limit = 100)
        {
            const string sql = "SELECT TOP (@Limit) Id, [Level], [Message], [Exception], [Source], StackTrace, MachineName, UserId, CreatedDate FROM ErrorLogs ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Limit", limit);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<ErrorLog> results = new List<ErrorLog>();
                        while (reader.Read())
                            results.Add(MapErrorLog(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve error logs.", exception);
            }
        }

        public ErrorLog GetById(int id)
        {
            const string sql = "SELECT Id, [Level], [Message], [Exception], [Source], StackTrace, MachineName, UserId, CreatedDate FROM ErrorLogs WHERE Id = @Id";
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
                            return MapErrorLog(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve error log by Id.", exception);
            }
            return null;
        }

        public void Add(ErrorLog entry)
        {
            Guard.AgainstNull(entry, nameof(entry));
            const string sql = "INSERT INTO ErrorLogs ([Level], [Message], [Exception], [Source], StackTrace, MachineName, UserId, CreatedDate) VALUES (@Level, @Message, @Exception, @Source, @StackTrace, @MachineName, @UserId, GETUTCDATE())";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Level", entry.Level ?? "Error");
                    command.Parameters.AddWithValue("@Message", entry.Message);
                    command.Parameters.AddWithValue("@Exception", (object)entry.Exception ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Source", (object)entry.Source ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StackTrace", (object)entry.StackTrace ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MachineName", (object)entry.MachineName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UserId", (object)entry.UserId ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add error log entry.", exception);
            }
        }

        public List<ErrorLog> Search(string keyword, int limit = 50)
        {
            Guard.AgainstNullOrWhiteSpace(keyword, nameof(keyword));
            const string sql = "SELECT TOP (@Limit) Id, [Level], [Message], [Exception], [Source], StackTrace, MachineName, UserId, CreatedDate FROM ErrorLogs WHERE [Message] LIKE @Keyword OR [Exception] LIKE @Keyword ORDER BY CreatedDate DESC";
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
                        List<ErrorLog> results = new List<ErrorLog>();
                        while (reader.Read())
                            results.Add(MapErrorLog(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to search error logs.", exception);
            }
        }

        public int GetErrorCountSince(DateTime since)
        {
            const string sql = "SELECT COUNT(1) FROM ErrorLogs WHERE CreatedDate >= @Since";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Since", since);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to count error logs.", exception);
            }
        }

        private static ErrorLog MapErrorLog(SqlDataReader reader)
        {
            return new ErrorLog
            {
                Id = (int)reader["Id"],
                Level = (string)reader["Level"],
                Message = (string)reader["Message"],
                Exception = reader["Exception"] == DBNull.Value ? null : (string)reader["Exception"],
                Source = reader["Source"] == DBNull.Value ? null : (string)reader["Source"],
                StackTrace = reader["StackTrace"] == DBNull.Value ? null : (string)reader["StackTrace"],
                MachineName = reader["MachineName"] == DBNull.Value ? null : (string)reader["MachineName"],
                UserId = reader["UserId"] == DBNull.Value ? null : (int?)reader["UserId"],
                CreatedDate = (DateTime)reader["CreatedDate"]
            };
        }
    }
}
