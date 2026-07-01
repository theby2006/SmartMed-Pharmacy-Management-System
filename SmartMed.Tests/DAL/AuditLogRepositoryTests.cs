using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Common.Exceptions;
using SmartMed.DAL.Infrastructure;
using SmartMed.DAL.Interfaces;
using SmartMed.DAL.Repositories;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;

namespace SmartMed.Tests.DAL
{
    [TestClass]
    public class AuditLogRepositoryTests
    {
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => new AuditLogRepository(null));
        }

        [TestMethod]
        public void LogLogin_ShouldThrow_WhenUsernameIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IAuditLogRepository repository = new AuditLogRepository(factory);

            Assert.ThrowsException<ValidationException>(() =>
                repository.LogLogin(1, null, "MACHINE"));
        }

        [TestMethod]
        public void LogLogin_ShouldThrow_WhenMachineNameIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IAuditLogRepository repository = new AuditLogRepository(factory);

            Assert.ThrowsException<ValidationException>(() =>
                repository.LogLogin(1, "admin", null));
        }

        [TestMethod]
        public void LogLogout_ShouldThrow_WhenUsernameIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IAuditLogRepository repository = new AuditLogRepository(factory);

            Assert.ThrowsException<ValidationException>(() =>
                repository.LogLogout(1, "   ", "MACHINE"));
        }
    }
}
