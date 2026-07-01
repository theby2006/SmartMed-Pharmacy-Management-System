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
    public class StockMovementRepositoryTests
    {
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => new StockMovementRepository(null));
        }

        [TestMethod]
        public void GetByReference_ShouldThrow_WhenReferenceTypeIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IStockMovementRepository repository = new StockMovementRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetByReference(null, 1));
        }

        [TestMethod]
        public void GetByReference_ShouldThrow_WhenReferenceTypeIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IStockMovementRepository repository = new StockMovementRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetByReference("   ", 1));
        }

        [TestMethod]
        public void Add_ShouldThrow_WhenMovementIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IStockMovementRepository repository = new StockMovementRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Add(null));
        }
    }
}
