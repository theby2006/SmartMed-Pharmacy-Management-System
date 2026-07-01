using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Common.Exceptions;
using SmartMed.DAL.Infrastructure;
using SmartMed.DAL.Interfaces;
using SmartMed.DAL.Repositories;
using SmartMed.Models.Entities;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;

namespace SmartMed.Tests.DAL
{
    [TestClass]
    public class MedicineCategoryRepositoryTests
    {
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => new MedicineCategoryRepository(null));
        }

        [TestMethod]
        public void GetByName_ShouldThrow_WhenNameIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IMedicineCategoryRepository repository = new MedicineCategoryRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetByName(null));
        }

        [TestMethod]
        public void GetByName_ShouldThrow_WhenNameIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IMedicineCategoryRepository repository = new MedicineCategoryRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetByName("   "));
        }

        [TestMethod]
        public void Add_ShouldThrow_WhenCategoryIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IMedicineCategoryRepository repository = new MedicineCategoryRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Add(null));
        }

        [TestMethod]
        public void Update_ShouldThrow_WhenCategoryIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IMedicineCategoryRepository repository = new MedicineCategoryRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Update(null));
        }
    }
}
