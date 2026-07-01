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
    public class UserRepositoryTests
    {
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => new UserRepository(null));
        }

        [TestMethod]
        public void GetByUsername_ShouldThrow_WhenUsernameIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IUserRepository repository = new UserRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetByUsername(null));
        }

        [TestMethod]
        public void GetByUsername_ShouldThrow_WhenUsernameIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IUserRepository repository = new UserRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetByUsername("   "));
        }
    }
}
