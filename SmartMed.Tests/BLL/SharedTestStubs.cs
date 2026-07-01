using System;
using System.Collections.Generic;
using SmartMed.BLL.Interfaces;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Session;

namespace SmartMed.Tests.BLL
{
    internal class MockSessionManager : ISessionManager
    {
        private SessionContext _currentSession;

        public SessionContext CurrentSession => _currentSession;

        public bool IsActive => _currentSession != null;

        public bool HasRole(RoleType role)
        {
            return _currentSession != null && _currentSession.Role == role;
        }

        public SessionContext StartSession(User user)
        {
            _currentSession = new SessionContext
            {
                UserId = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Role = user.Role,
                LoginTimeUtc = DateTime.UtcNow,
                LastActivityTimeUtc = DateTime.UtcNow
            };
            return _currentSession;
        }

        public void EndSession()
        {
            _currentSession = null;
        }
    }

    internal class MockAuditLogRepository : IAuditLogRepository
    {
        public bool LogLoginCalled { get; set; }
        public bool LogLogoutCalled { get; set; }
        public bool LogFailedAttemptCalled { get; set; }
        public bool LogCalled { get; set; }
        public AuditAction? LoggedAction { get; set; }
        public string LoggedDetails { get; set; }

        public void LogLogin(int userId, string username, string machineName)
        {
            LogLoginCalled = true;
        }

        public void LogLogout(int? userId, string username, string machineName)
        {
            LogLogoutCalled = true;
        }

        public void LogFailedAttempt(string username, string machineName, string details)
        {
            LogFailedAttemptCalled = true;
        }

        public void Log(int? userId, string username, AuditAction action, string machineName, string details)
        {
            LogCalled = true;
            LoggedAction = action;
            LoggedDetails = details;
        }

        public List<AuditLogEntry> GetAll(int limit = 100) => new List<AuditLogEntry>();
        public List<AuditLogEntry> GetByDateRange(DateTime from, DateTime to) => new List<AuditLogEntry>();
        public List<AuditLogEntry> GetByAction(AuditAction action) => new List<AuditLogEntry>();
        public List<AuditLogEntry> GetByUser(int? userId) => new List<AuditLogEntry>();
        public List<AuditLogEntry> Search(string keyword, int limit = 50) => new List<AuditLogEntry>();
    }
}
