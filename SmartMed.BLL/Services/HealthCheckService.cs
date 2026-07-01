using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Diagnostics;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IBackupHistoryRepository _backupHistoryRepository;
        private readonly ILogger _logger;
        private readonly IUserRepository _userRepository;

        public HealthCheckService(
            IDbConnectionFactory connectionFactory,
            IBackupHistoryRepository backupHistoryRepository,
            ILogger logger,
            IUserRepository userRepository)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            Guard.AgainstNull(backupHistoryRepository, nameof(backupHistoryRepository));
            Guard.AgainstNull(logger, nameof(logger));
            Guard.AgainstNull(userRepository, nameof(userRepository));
            _connectionFactory = connectionFactory;
            _backupHistoryRepository = backupHistoryRepository;
            _logger = logger;
            _userRepository = userRepository;
        }

        public OperationResult<HealthCheckResult> RunHealthCheck()
        {
            HealthCheckResult result = new HealthCheckResult
            {
                CheckTimestamp = DateTime.UtcNow
            };

            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT 1", connection))
                    {
                        cmd.ExecuteScalar();
                    }
                }
                sw.Stop();
                result.DatabaseConnectionMs = (int)sw.ElapsedMilliseconds;
                result.DatabaseStatus = "Connected";
                result.IsHealthy = true;
            }
            catch (Exception ex)
            {
                result.DatabaseStatus = $"Failed: {ex.Message}";
                result.DatabaseConnectionMs = -1;
                result.IsHealthy = false;
                _logger.LogError("Health check database connection failed", ex);
            }

            try
            {
                var latest = _backupHistoryRepository.GetLatest();
                result.LastBackupDate = latest?.CreatedDate;
                result.BackupStatus = latest != null ? "Available" : "No backups found";
            }
            catch
            {
                result.BackupStatus = "Check failed";
            }

            try
            {
                var users = _userRepository.GetAll();
                result.TotalUsers = users.Count;
                result.ActiveUsers = users.FindAll(u => u.IsActive).Count;
            }
            catch
            {
            }

            try
            {
                result.RecentErrors = _logger.GetErrorCountSince(DateTime.UtcNow.AddDays(1));
            }
            catch
            {
            }

            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                DriveInfo drive = new DriveInfo(Path.GetPathRoot(appDir));
                if (drive.IsReady)
                    result.DiskSpaceFreeMb = drive.AvailableFreeSpace / (1024 * 1024);
            }
            catch
            {
            }

            return OperationResult<HealthCheckResult>.Success(result);
        }
    }
}
