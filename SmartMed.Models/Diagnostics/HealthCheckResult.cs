using System;

namespace SmartMed.Models.Diagnostics
{
    public class HealthCheckResult
    {
        public bool IsHealthy { get; set; }
        public string DatabaseStatus { get; set; }
        public int DatabaseConnectionMs { get; set; }
        public string BackupStatus { get; set; }
        public DateTime? LastBackupDate { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int RecentErrors { get; set; }
        public long DiskSpaceFreeMb { get; set; }
        public DateTime CheckTimestamp { get; set; }
    }
}
