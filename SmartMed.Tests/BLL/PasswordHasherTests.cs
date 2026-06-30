using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Common.Exceptions;

namespace SmartMed.Tests.BLL
{
    [TestClass]
    public class PasswordHasherTests
    {
        private IPasswordHasher _passwordHasher;

        [TestInitialize]
        public void TestInitialize()
        {
            _passwordHasher = new PasswordHasher();
        }

        [TestMethod]
        public void HashPassword_ShouldReturnNonEmptyHash()
        {
            string salt = _passwordHasher.GenerateSalt();
            string hash = _passwordHasher.HashPassword("TestP@ss123", salt);

            Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        }

        [TestMethod]
        public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
        {
            string password = "TestP@ss123";
            string salt = _passwordHasher.GenerateSalt();
            string hash = _passwordHasher.HashPassword(password, salt);

            bool result = _passwordHasher.VerifyPassword(password, hash, salt);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
        {
            string salt = _passwordHasher.GenerateSalt();
            string hash = _passwordHasher.HashPassword("CorrectP@ss1", salt);

            bool result = _passwordHasher.VerifyPassword("WrongP@ss1", hash, salt);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GenerateSalt_ShouldReturnNonEmptyBase64String()
        {
            string salt = _passwordHasher.GenerateSalt();

            Assert.IsFalse(string.IsNullOrWhiteSpace(salt));
        }

        [TestMethod]
        public void GenerateSalt_ShouldProduceUniqueValuesPerCall()
        {
            string salt1 = _passwordHasher.GenerateSalt();
            string salt2 = _passwordHasher.GenerateSalt();

            Assert.AreNotEqual(salt1, salt2);
        }

        [TestMethod]
        public void HashPassword_ShouldThrow_WhenPasswordIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                _passwordHasher.HashPassword(null, _passwordHasher.GenerateSalt()));
        }

        [TestMethod]
        public void VerifyPassword_ShouldThrow_WhenPasswordIsNull()
        {
            string salt = _passwordHasher.GenerateSalt();
            string hash = _passwordHasher.HashPassword("TestP@ss123", salt);

            Assert.ThrowsException<ValidationException>(() =>
                _passwordHasher.VerifyPassword(null, hash, salt));
        }
    }
}
