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
    public class MedicineRepositoryTests
    {
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => new MedicineRepository(null));
        }

        [TestMethod]
        public void Search_ShouldThrow_WhenKeywordIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IMedicineRepository repository = new MedicineRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Search(null));
        }

        [TestMethod]
        public void Search_ShouldThrow_WhenKeywordIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IMedicineRepository repository = new MedicineRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Search("   "));
        }

        [TestMethod]
        public void Add_ShouldThrow_WhenMedicineIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IMedicineRepository repository = new MedicineRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Add(null));
        }

        [TestMethod]
        public void Update_ShouldThrow_WhenMedicineIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IMedicineRepository repository = new MedicineRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Update(null));
        }

        [TestMethod]
        public void GetByNameAndBrand_ShouldThrow_WhenNameIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IMedicineRepository repository = new MedicineRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetByNameAndBrand(null, "Brand"));
        }

        [TestMethod]
        public void GetByNameAndBrand_ShouldThrow_WhenNameIsWhitespace()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IMedicineRepository repository = new MedicineRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetByNameAndBrand("   ", "Brand"));
        }

        [TestMethod]
        public void GetNearExpiry_ShouldThrow_WhenThresholdDaysIsNegative()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IMedicineRepository repository = new MedicineRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.GetNearExpiry(-1));
        }
    }
}
