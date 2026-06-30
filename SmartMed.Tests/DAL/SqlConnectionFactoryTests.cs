using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Common.Exceptions;
using SmartMed.DAL.Infrastructure;
using SmartMed.DAL.Interfaces;

namespace SmartMed.Tests.DAL
{
    [TestClass]
    public class SqlConnectionFactoryTests
    {
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionStringIsEmpty()
        {
            Assert.ThrowsException<ValidationException>(() => new SqlConnectionFactory(string.Empty));
        }

        [TestMethod]
        public void CreateConnection_ShouldReturnSqlConnection()
        {
            IDbConnectionFactory factory = new SqlConnectionFactory(
                "Data Source=.;Initial Catalog=SmartMedDb;Integrated Security=True");

            SqlConnection connection = factory.CreateConnection();

            Assert.IsNotNull(connection);
            Assert.AreEqual(ConnectionState.Closed, connection.State);
        }
    }
}
