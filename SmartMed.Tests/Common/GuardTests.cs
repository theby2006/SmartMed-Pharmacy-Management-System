using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;

namespace SmartMed.Tests.Common
{
    [TestClass]
    public class GuardTests
    {
        [TestMethod]
        public void AgainstNull_ShouldThrow_WhenValueIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => Guard.AgainstNull(null, "value"));
        }

        [TestMethod]
        public void AgainstNullOrWhiteSpace_ShouldThrow_WhenValueIsWhitespace()
        {
            Assert.ThrowsException<ValidationException>(() => Guard.AgainstNullOrWhiteSpace("  ", "text"));
        }

        [TestMethod]
        public void AgainstNegative_ShouldThrow_WhenValueIsNegative()
        {
            Assert.ThrowsException<ValidationException>(() => Guard.AgainstNegative(-1, "count"));
        }

        [TestMethod]
        public void AgainstZeroOrNegative_ShouldThrow_WhenIntValueIsZero()
        {
            Assert.ThrowsException<ValidationException>(() => Guard.AgainstZeroOrNegative(0, "count"));
        }

        [TestMethod]
        public void AgainstZeroOrNegative_ShouldThrow_WhenDecimalValueIsNegative()
        {
            Assert.ThrowsException<ValidationException>(() => Guard.AgainstZeroOrNegative(-2.5m, "amount"));
        }
    }
}
