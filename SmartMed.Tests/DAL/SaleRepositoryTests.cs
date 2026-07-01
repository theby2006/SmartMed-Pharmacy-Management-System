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
    public class SaleRepositoryTests
    {
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => new SaleRepository(null));
        }

        [TestMethod]
        public void GetBySaleNumber_ShouldThrow_WhenNumberIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            ISaleRepository repository = new SaleRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetBySaleNumber(null));
        }

        [TestMethod]
        public void GetBySaleNumber_ShouldThrow_WhenNumberIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            ISaleRepository repository = new SaleRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetBySaleNumber("   "));
        }

        [TestMethod]
        public void Search_ShouldThrow_WhenKeywordIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            ISaleRepository repository = new SaleRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Search(null));
        }

        [TestMethod]
        public void Search_ShouldThrow_WhenKeywordIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            ISaleRepository repository = new SaleRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Search("   "));
        }

        [TestMethod]
        public void Add_ShouldThrow_WhenSaleIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            ISaleRepository repository = new SaleRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Add((SmartMed.Models.Entities.Sale)null));
        }
    }
}
