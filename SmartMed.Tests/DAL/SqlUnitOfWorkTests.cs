using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Common.Exceptions;
using SmartMed.DAL.Infrastructure;

namespace SmartMed.Tests.DAL
{
    [TestClass]
    public class SqlUnitOfWorkTests
    {
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => new SqlUnitOfWork(null));
        }
    }
}
