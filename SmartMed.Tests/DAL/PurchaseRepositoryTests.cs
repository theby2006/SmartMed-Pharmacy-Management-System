using System;
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
    public class PurchaseRepositoryTests
    {
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => new PurchaseRepository(null));
        }

        [TestMethod]
        public void GetByPurchaseNumber_ShouldThrow_WhenNumberIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IPurchaseRepository repository = new PurchaseRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetByPurchaseNumber(null));
        }

        [TestMethod]
        public void GetByPurchaseNumber_ShouldThrow_WhenNumberIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IPurchaseRepository repository = new PurchaseRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetByPurchaseNumber("   "));
        }

        [TestMethod]
        public void Search_ShouldThrow_WhenKeywordIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IPurchaseRepository repository = new PurchaseRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Search(null));
        }

        [TestMethod]
        public void Search_ShouldThrow_WhenKeywordIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IPurchaseRepository repository = new PurchaseRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Search("   "));
        }

        [TestMethod]
        public void Add_ShouldThrow_WhenPurchaseIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IPurchaseRepository repository = new PurchaseRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Add(null));
        }

        [TestMethod]
        public void Update_ShouldThrow_WhenPurchaseIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IPurchaseRepository repository = new PurchaseRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Update(null));
        }
    }
}
