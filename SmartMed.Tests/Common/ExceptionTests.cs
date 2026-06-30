using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Common.Exceptions;

namespace SmartMed.Tests.Common
{
    [TestClass]
    public class ExceptionTests
    {
        [TestMethod]
        public void DataAccessException_ShouldPreserveInnerException()
        {
            var innerException = new InvalidOperationException("Inner failure.");
            var exception = new DataAccessException("Outer failure.", innerException);

            Assert.AreEqual("Outer failure.", exception.Message);
            Assert.AreSame(innerException, exception.InnerException);
        }

        [TestMethod]
        public void ConfigurationException_ShouldInheritFromAppException()
        {
            var exception = new ConfigurationException("Missing setting.");

            Assert.IsInstanceOfType(exception, typeof(AppException));
        }

        [TestMethod]
        public void AuthenticationException_ShouldInheritFromAppException()
        {
            var exception = new AuthenticationException("Login failed.");

            Assert.IsInstanceOfType(exception, typeof(AppException));
            Assert.AreEqual("Login failed.", exception.Message);
        }
    }
}
