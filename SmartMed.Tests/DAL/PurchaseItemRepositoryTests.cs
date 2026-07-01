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
    public class PurchaseItemRepositoryTests
    {
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => new PurchaseItemRepository(null));
        }

        [TestMethod]
        public void Add_ShouldThrow_WhenItemIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IPurchaseItemRepository repository = new PurchaseItemRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Add(null));
        }

        [TestMethod]
        public void AddRange_ShouldThrow_WhenItemsIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IPurchaseItemRepository repository = new PurchaseItemRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.AddRange(null));
        }
    }
}
