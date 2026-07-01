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
    public class PaymentRepositoryTests
    {
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => new PaymentRepository(null));
        }

        [TestMethod]
        public void Add_ShouldThrow_WhenPaymentIsNull()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            IPaymentRepository repository = new PaymentRepository(factory);

            Assert.ThrowsException<ValidationException>(() => repository.Add(null));
        }
    }
}
