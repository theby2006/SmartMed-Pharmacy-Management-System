using System.Collections.Generic;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;

namespace SmartMed.DAL.Interfaces
{
    public interface IAuditLogRepository
    {
        void LogLogin(int userId, string username, string machineName);

        void LogLogout(int? userId, string username, string machineName);

        void LogFailedAttempt(string username, string machineName, string details);

        void Log(int? userId, string username, AuditAction action, string machineName, string details);

        List<AuditLogEntry> GetAll(int limit = 100);

        List<AuditLogEntry> GetByDateRange(System.DateTime from, System.DateTime to);

        List<AuditLogEntry> GetByAction(AuditAction action);

        List<AuditLogEntry> GetByUser(int? userId);

        List<AuditLogEntry> Search(string keyword, int limit = 50);
    }
}
