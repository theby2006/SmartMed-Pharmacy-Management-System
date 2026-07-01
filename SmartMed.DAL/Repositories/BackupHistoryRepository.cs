using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Repositories
{
    public class BackupHistoryRepository : IBackupHistoryRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public BackupHistoryRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public List<BackupHistory> GetAll()
        {
            const string sql = "SELECT Id, FileName, FilePath, FileSizeBytes, DatabaseName, BackupType, Status, ErrorMessage, CreatedByUserId, IsActive, CreatedDate, UpdatedDate FROM BackupHistory ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<BackupHistory> results = new List<BackupHistory>();
                        while (reader.Read())
                            results.Add(MapBackupHistory(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve backup history.", exception);
            }
        }

        public BackupHistory GetById(int id)
        {
            const string sql = "SELECT Id, FileName, FilePath, FileSizeBytes, DatabaseName, BackupType, Status, ErrorMessage, CreatedByUserId, IsActive, CreatedDate, UpdatedDate FROM BackupHistory WHERE Id = @Id";
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
                            return MapBackupHistory(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve backup history by Id.", exception);
            }
            return null;
        }

        public BackupHistory GetLatest()
        {
            const string sql = "SELECT TOP 1 Id, FileName, FilePath, FileSizeBytes, DatabaseName, BackupType, Status, ErrorMessage, CreatedByUserId, IsActive, CreatedDate, UpdatedDate FROM BackupHistory WHERE Status = 'Completed' ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapBackupHistory(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve latest backup.", exception);
            }
            return null;
        }

        public void Add(BackupHistory entry)
        {
            Guard.AgainstNull(entry, nameof(entry));
            const string sql = "INSERT INTO BackupHistory (FileName, FilePath, FileSizeBytes, DatabaseName, BackupType, Status, ErrorMessage, CreatedByUserId, CreatedDate) VALUES (@FileName, @FilePath, @FileSizeBytes, @DatabaseName, @BackupType, @Status, @ErrorMessage, @CreatedByUserId, GETUTCDATE())";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@FileName", entry.FileName);
                    command.Parameters.AddWithValue("@FilePath", entry.FilePath);
                    command.Parameters.AddWithValue("@FileSizeBytes", entry.FileSizeBytes);
                    command.Parameters.AddWithValue("@DatabaseName", entry.DatabaseName);
                    command.Parameters.AddWithValue("@BackupType", entry.BackupType ?? "Full");
                    command.Parameters.AddWithValue("@Status", entry.Status ?? "Completed");
                    command.Parameters.AddWithValue("@ErrorMessage", (object)entry.ErrorMessage ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedByUserId", (object)entry.CreatedByUserId ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add backup history entry.", exception);
            }
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM BackupHistory WHERE Id = @Id";
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
                throw new DataAccessException("Failed to delete backup history entry.", exception);
            }
        }

        public void DeleteOlderThan(int keepCount)
        {
            const string sql = "DELETE FROM BackupHistory WHERE Id NOT IN (SELECT TOP (@KeepCount) Id FROM BackupHistory ORDER BY CreatedDate DESC)";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@KeepCount", keepCount);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to delete old backup history entries.", exception);
            }
        }

        private static BackupHistory MapBackupHistory(SqlDataReader reader)
        {
            return new BackupHistory
            {
                Id = (int)reader["Id"],
                FileName = (string)reader["FileName"],
                FilePath = (string)reader["FilePath"],
                FileSizeBytes = (long)reader["FileSizeBytes"],
                DatabaseName = (string)reader["DatabaseName"],
                BackupType = (string)reader["BackupType"],
                Status = (string)reader["Status"],
                ErrorMessage = reader["ErrorMessage"] == DBNull.Value ? null : (string)reader["ErrorMessage"],
                CreatedByUserId = reader["CreatedByUserId"] == DBNull.Value ? null : (int?)reader["CreatedByUserId"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };
        }
    }
}
