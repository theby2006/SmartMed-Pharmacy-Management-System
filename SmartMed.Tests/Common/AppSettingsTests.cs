using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;
using SmartMed.Common.Exceptions;

namespace SmartMed.Tests.Common
{
    [TestClass]
    public class AppSettingsTests
    {
        [TestMethod]
        public void GetRequiredString_ShouldReturnConfiguredValue()
        {
            string value = AppSettings.GetRequiredString("ValidString");

            Assert.AreEqual("ConfiguredValue", value);
        }

        [TestMethod]
        public void GetRequiredString_ShouldThrow_WhenKeyIsMissing()
        {
            Assert.ThrowsException<ConfigurationException>(() => AppSettings.GetRequiredString("MissingKey"));
        }

        [TestMethod]
        public void GetRequiredInt_ShouldThrow_WhenValueIsInvalid()
        {
            Assert.ThrowsException<ConfigurationException>(() => AppSettings.GetRequiredInt("InvalidInt"));
        }

        [TestMethod]
        public void GetRequiredBool_ShouldReturnConfiguredBoolean()
        {
            bool value = AppSettings.GetRequiredBool("ValidBool");

            Assert.IsTrue(value);
        }

        [TestMethod]
        public void GetConnectionString_ShouldReturnConfiguredConnectionString()
        {
            string value = AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName);

            StringAssert.Contains(value, "Initial Catalog=SmartMedDb");
        }
    }
}
