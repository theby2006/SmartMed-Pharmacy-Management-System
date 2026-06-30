using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Models.Results;

namespace SmartMed.Tests.Models
{
    [TestClass]
    public class OperationResultTests
    {
        [TestMethod]
        public void Success_ShouldSetIsSuccessToTrue()
        {
            OperationResult result = OperationResult.Success("Saved");

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.IsFailure);
            Assert.AreEqual("Saved", result.Message);
        }

        [TestMethod]
        public void Failure_ShouldSetIsSuccessToFalse()
        {
            OperationResult result = OperationResult.Failure("Failed");

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual("Failed", result.Message);
        }

        [TestMethod]
        public void GenericSuccess_ShouldPreserveData()
        {
            OperationResult<int> result = OperationResult<int>.Success(10, "Loaded");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(10, result.Data);
            Assert.AreEqual("Loaded", result.Message);
        }

        [TestMethod]
        public void GenericFailure_ShouldReturnDefaultData()
        {
            OperationResult<int> result = OperationResult<int>.Failure("Failed");

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(0, result.Data);
        }
    }
}
