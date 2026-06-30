using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Models.Common;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;

namespace SmartMed.Tests.Models
{
    [TestClass]
    public class UserModelTests
    {
        [TestMethod]
        public void User_ShouldExtendBaseEntity()
        {
            User user = new User();

            Assert.IsInstanceOfType(user, typeof(BaseEntity));
            Assert.AreEqual(0, user.Id);
            Assert.IsTrue(user.IsActive);
            Assert.IsTrue(user.CreatedDate > DateTime.MinValue);
        }

        [TestMethod]
        public void User_ShouldSetFailedLoginAttempts_DefaultZero()
        {
            User user = new User();

            Assert.AreEqual(0, user.FailedLoginAttempts);
        }

        [TestMethod]
        public void User_ShouldSetLockedUntil_DefaultNull()
        {
            User user = new User();

            Assert.IsNull(user.LockedUntil);
        }

        [TestMethod]
        public void User_ShouldSetLastLogin_DefaultNull()
        {
            User user = new User();

            Assert.IsNull(user.LastLogin);
        }

        [TestMethod]
        public void User_ShouldSetPropertiesCorrectly()
        {
            DateTime now = DateTime.UtcNow;

            User user = new User
            {
                Id = 10,
                Username = "testuser",
                PasswordHash = "hash123",
                PasswordSalt = "salt123",
                DisplayName = "Test User",
                Role = RoleType.Pharmacist,
                Email = "test@example.com",
                FailedLoginAttempts = 3,
                LockedUntil = now,
                LastLogin = now,
                IsActive = true
            };

            Assert.AreEqual(10, user.Id);
            Assert.AreEqual("testuser", user.Username);
            Assert.AreEqual("hash123", user.PasswordHash);
            Assert.AreEqual("salt123", user.PasswordSalt);
            Assert.AreEqual("Test User", user.DisplayName);
            Assert.AreEqual(RoleType.Pharmacist, user.Role);
            Assert.AreEqual("test@example.com", user.Email);
            Assert.AreEqual(3, user.FailedLoginAttempts);
            Assert.AreEqual(now, user.LockedUntil);
            Assert.AreEqual(now, user.LastLogin);
            Assert.IsTrue(user.IsActive);
        }
    }
}
