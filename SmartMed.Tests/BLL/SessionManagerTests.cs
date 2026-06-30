using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Session;

namespace SmartMed.Tests.BLL
{
    [TestClass]
    public class SessionManagerTests
    {
        private ISessionManager _sessionManager;

        private User _adminUser;

        [TestInitialize]
        public void TestInitialize()
        {
            _sessionManager = new SessionManager();
            _adminUser = new User
            {
                Id = 1,
                Username = "admin",
                DisplayName = "Admin User",
                Role = RoleType.Administrator
            };
        }

        [TestMethod]
        public void StartSession_ShouldPopulateSessionContext()
        {
            SessionContext context = _sessionManager.StartSession(_adminUser);

            Assert.IsNotNull(context);
            Assert.AreEqual(_adminUser.Id, context.UserId);
            Assert.AreEqual(_adminUser.Username, context.Username);
            Assert.AreEqual(_adminUser.DisplayName, context.DisplayName);
            Assert.AreEqual(_adminUser.Role, context.Role);
            Assert.IsTrue(context.IsAuthenticated);
            Assert.IsNotNull(context.LoginTimeUtc);
            Assert.IsNotNull(context.LastActivityTimeUtc);
        }

        [TestMethod]
        public void EndSession_ShouldClearSession()
        {
            _sessionManager.StartSession(_adminUser);
            _sessionManager.EndSession();

            Assert.IsNull(_sessionManager.CurrentSession);
            Assert.IsFalse(_sessionManager.IsActive);
        }

        [TestMethod]
        public void HasRole_ShouldReturnTrue_WhenRoleMatches()
        {
            _sessionManager.StartSession(_adminUser);

            bool result = _sessionManager.HasRole(RoleType.Administrator);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HasRole_ShouldReturnFalse_WhenRoleDoesNotMatch()
        {
            _sessionManager.StartSession(_adminUser);

            bool result = _sessionManager.HasRole(RoleType.Cashier);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void StartSession_ShouldReplacePriorSession()
        {
            _sessionManager.StartSession(_adminUser);

            User cashierUser = new User
            {
                Id = 2,
                Username = "cashier1",
                DisplayName = "Cashier User",
                Role = RoleType.Cashier
            };

            SessionContext context = _sessionManager.StartSession(cashierUser);

            Assert.AreEqual(2, context.UserId);
            Assert.AreEqual(RoleType.Cashier, context.Role);
        }
    }
}
