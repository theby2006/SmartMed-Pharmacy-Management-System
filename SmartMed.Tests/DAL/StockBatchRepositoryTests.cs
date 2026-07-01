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
    public class StockBatchRepositoryTests
    {
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => new StockBatchRepository(null));
        }

        [TestMethod]
        public void GetBatch_ShouldThrow_WhenBatchNumberIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IStockBatchRepository repository = new StockBatchRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetBatch(1, null));
        }

        [TestMethod]
        public void GetBatch_ShouldThrow_WhenBatchNumberIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IStockBatchRepository repository = new StockBatchRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetBatch(1, "   "));
        }

        [TestMethod]
        public void Add_ShouldThrow_WhenBatchIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IStockBatchRepository repository = new StockBatchRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Add(null));
        }
    }
}
