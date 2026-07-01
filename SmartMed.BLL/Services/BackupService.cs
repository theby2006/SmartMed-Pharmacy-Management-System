using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class BackupService : IBackupService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IBackupHistoryRepository _backupHistoryRepository;
        private readonly ISettingService _settingService;

        public BackupService(
            IDbConnectionFactory connectionFactory,
            IBackupHistoryRepository backupHistoryRepository,
            ISettingService settingService)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            Guard.AgainstNull(backupHistoryRepository, nameof(backupHistoryRepository));
            Guard.AgainstNull(settingService, nameof(settingService));
            _connectionFactory = connectionFactory;
            _backupHistoryRepository = backupHistoryRepository;
            _settingService = settingService;
        }

        public OperationResult<string> CreateBackup(string customDirectory = null)
        {
            try
            {
                string directory = customDirectory;
                if (string.IsNullOrWhiteSpace(directory))
                {
                    var dirResult = _settingService.GetValue("BackupDirectory", "App_Data\\Backups");
                    if (!dirResult.IsSuccess)
                        return OperationResult<string>.Failure(dirResult.Message);
                    directory = dirResult.Data;
                }

                string fullDir = Path.IsPathRooted(directory) ? directory : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directory);
                Directory.CreateDirectory(fullDir);

                string databaseName;
                string fileName;
                string fullPath;

                using (SqlConnection connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    databaseName = connection.Database;
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    fileName = $"{databaseName}_backup_{timestamp}.bak";
                    fullPath = Path.Combine(fullDir, fileName);

                    string sql = $"BACKUP DATABASE [{databaseName}] TO DISK = @FilePath WITH FORMAT, MEDIANAME = 'SmartMedBackup', NAME = 'Full Backup of {databaseName}'";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@FilePath", fullPath);
                        command.ExecuteNonQuery();
                    }
                }

                FileInfo fi = new FileInfo(fullPath);
                BackupHistory entry = new BackupHistory
                {
                    FileName = fileName,
                    FilePath = fullPath,
                    FileSizeBytes = fi.Length,
                    DatabaseName = databaseName,
                    BackupType = "Full",
                    Status = "Completed"
                };
                _backupHistoryRepository.Add(entry);

                return OperationResult<string>.Success(fullPath, "Backup created successfully.");
            }
            catch (SqlException ex)
            {
                return OperationResult<string>.Failure($"Backup failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return OperationResult<string>.Failure($"Backup failed: {ex.Message}");
            }
        }

        public OperationResult RestoreDatabase(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return OperationResult.Failure("Backup file path is required.");
                if (!File.Exists(filePath))
                    return OperationResult.Failure($"Backup file not found: {filePath}");

                using (SqlConnection connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    string databaseName = connection.Database;

                    string setSingleUser = $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    string restoreSql = $"RESTORE DATABASE [{databaseName}] FROM DISK = @FilePath WITH REPLACE";
                    string setMultiUser = $"ALTER DATABASE [{databaseName}] SET MULTI_USER";

                    using (SqlCommand cmd = new SqlCommand(setSingleUser, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(restoreSql, connection))
                        {
                            cmd.Parameters.AddWithValue("@FilePath", filePath);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    finally
                    {
                        using (SqlCommand cmd = new SqlCommand(setMultiUser, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                return OperationResult.Success("Database restored successfully.");
            }
            catch (SqlException ex)
            {
                return OperationResult.Failure($"Restore failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return OperationResult.Failure($"Restore failed: {ex.Message}");
            }
        }

        public OperationResult<List<BackupHistory>> GetBackupHistory()
        {
            try
            {
                return OperationResult<List<BackupHistory>>.Success(_backupHistoryRepository.GetAll());
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<BackupHistory>>.Failure(ex.Message);
            }
        }

        public OperationResult DeleteBackup(int backupId)
        {
            try
            {
                BackupHistory entry = _backupHistoryRepository.GetById(backupId);
                if (entry == null)
                    return OperationResult.Failure($"Backup entry with ID {backupId} not found.");

                if (File.Exists(entry.FilePath))
                {
                    try { File.Delete(entry.FilePath); }
                    catch { }
                }

                _backupHistoryRepository.Delete(backupId);
                return OperationResult.Success("Backup deleted successfully.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult<string> GetLastBackupInfo()
        {
            try
            {
                BackupHistory latest = _backupHistoryRepository.GetLatest();
                if (latest == null)
                    return OperationResult<string>.Success("No backups found.");
                return OperationResult<string>.Success($"{latest.FileName} ({latest.CreatedDate:yyyy-MM-dd HH:mm}) - {FormatSize(latest.FileSizeBytes)}");
            }
            catch (DataAccessException ex)
            {
                return OperationResult<string>.Failure(ex.Message);
            }
        }

        public OperationResult CleanupOldBackups(int keepCount)
        {
            try
            {
                List<BackupHistory> all = _backupHistoryRepository.GetAll();
                var toDelete = all.OrderByDescending(b => b.CreatedDate).Skip(keepCount).ToList();

                foreach (BackupHistory entry in toDelete)
                {
                    if (File.Exists(entry.FilePath))
                    {
                        try { File.Delete(entry.FilePath); }
                        catch { }
                    }
                    _backupHistoryRepository.Delete(entry.Id);
                }

                return OperationResult.Success($"Cleaned up {toDelete.Count} old backup(s). Keeping last {keepCount}.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1073741824)
                return $"{bytes / 1073741824.0:F2} GB";
            if (bytes >= 1048576)
                return $"{bytes / 1048576.0:F2} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024.0:F2} KB";
            return $"{bytes} B";
        }
    }
}
