using System;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface IUserRepository : IRepository
    {
        User GetById(int userId);

        User GetByUsername(string username);

        void IncrementFailedAttempts(int userId);

        void ResetFailedAttempts(int userId);

        void SetLockedUntil(int userId, DateTime? lockedUntil);

        void UpdateLastLogin(int userId, DateTime loginTime);
    }
}
