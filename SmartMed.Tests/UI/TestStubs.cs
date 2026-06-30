using System;
using SmartMed.BLL.Interfaces;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Session;

namespace SmartMed.Tests.UI
{
    internal class StubUserRepository : IUserRepository
    {
        public User GetById(int userId) => null;
        public User GetByUsername(string username) => null;
        public void IncrementFailedAttempts(int userId) { }
        public void ResetFailedAttempts(int userId) { }
        public void SetLockedUntil(int userId, DateTime? lockedUntil) { }
        public void UpdateLastLogin(int userId, DateTime loginTime) { }
    }

    internal class StubPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password, string salt) => "hash";
        public bool VerifyPassword(string password, string hash, string salt) => false;
        public string GenerateSalt() => "salt";
    }

    internal class StubSessionManager : ISessionManager
    {
        public SessionContext StartSession(User user) => new SessionContext { UserId = user.Id };
        public void EndSession() { }
        public SessionContext CurrentSession => null;
        public bool IsActive => false;
        public bool HasRole(RoleType role) => false;
    }

    internal class StubAuditLogRepository : IAuditLogRepository
    {
        public void LogLogin(int userId, string username, string machineName) { }
        public void LogLogout(int? userId, string username, string machineName) { }
        public void LogFailedAttempt(string username, string machineName, string details) { }
    }
}
