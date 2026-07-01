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
    public class SupplierRepositoryTests
    {
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => new SupplierRepository(null));
        }

        [TestMethod]
        public void GetBySupplierCode_ShouldThrow_WhenCodeIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            ISupplierRepository repository = new SupplierRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetBySupplierCode(null));
        }

        [TestMethod]
        public void GetBySupplierCode_ShouldThrow_WhenCodeIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            ISupplierRepository repository = new SupplierRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetBySupplierCode("   "));
        }

        [TestMethod]
        public void GetByName_ShouldThrow_WhenNameIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            ISupplierRepository repository = new SupplierRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetByName(null));
        }

        [TestMethod]
        public void GetByName_ShouldThrow_WhenNameIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            ISupplierRepository repository = new SupplierRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetByName("   "));
        }

        [TestMethod]
        public void Add_ShouldThrow_WhenSupplierIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            ISupplierRepository repository = new SupplierRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Add(null));
        }

        [TestMethod]
        public void Update_ShouldThrow_WhenSupplierIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            ISupplierRepository repository = new SupplierRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Update(null));
        }

        [TestMethod]
        public void Search_ShouldThrow_WhenKeywordIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            ISupplierRepository repository = new SupplierRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Search(null));
        }

        [TestMethod]
        public void Search_ShouldThrow_WhenKeywordIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            ISupplierRepository repository = new SupplierRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Search("   "));
        }
    }
}
