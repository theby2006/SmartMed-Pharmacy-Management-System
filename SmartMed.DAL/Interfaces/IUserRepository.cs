using System;
using System.Collections.Generic;
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

        List<User> GetAll();

        List<User> Search(string keyword);

        int Add(User user);

        void Update(User user);

        void UpdatePassword(int userId, string passwordHash, string passwordSalt);

        void UpdateActiveStatus(int userId, bool isActive);
    }
}
