using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;
using SmartMed.DAL.Infrastructure;
using SmartMed.DAL.Interfaces;

namespace SmartMed.Tests.UI
{
    [TestClass]
    public class BootstrapperArchitectureTests
    {
        [TestMethod]
        public void StartupDependencies_ShouldCompose_ThroughAbstractions()
        {
            string connectionString = AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName);
            IDbConnectionFactory connectionFactory = new SqlConnectionFactory(connectionString);
            IStartupDiagnosticsService startupDiagnosticsService = new StartupDiagnosticsService(connectionFactory);

            var result = startupDiagnosticsService.BuildContext();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual("SqlConnection", result.Data.DataProviderName);
        }
    }
}
