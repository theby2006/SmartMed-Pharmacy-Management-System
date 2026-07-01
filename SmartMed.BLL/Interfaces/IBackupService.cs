using System.Collections.Generic;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface IBackupService
    {
        OperationResult<string> CreateBackup(string customDirectory = null);
        OperationResult RestoreDatabase(string filePath);
        OperationResult<List<BackupHistory>> GetBackupHistory();
        OperationResult DeleteBackup(int backupId);
        OperationResult<string> GetLastBackupInfo();
        OperationResult CleanupOldBackups(int keepCount);
    }
}
