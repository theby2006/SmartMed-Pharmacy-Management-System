using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Models.Results;

namespace SmartMed.Tests.BLL
{
    [TestClass]
    public class PrescriptionServiceTests
    {
        private MockOrderRepository _orderRepository;
        private MockAuditLogRepository _auditLogRepository;
        private MockSessionManager _sessionManager;
        private IPrescriptionService _service;
        private string _tempFile;

        [TestInitialize]
        public void TestInitialize()
        {
            _orderRepository = new MockOrderRepository();
            _auditLogRepository = new MockAuditLogRepository();
            _sessionManager = new MockSessionManager();
            _service = new PrescriptionService(_orderRepository, _auditLogRepository, _sessionManager);
            _tempFile = null;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (_tempFile != null && File.Exists(_tempFile))
                File.Delete(_tempFile);
        }

        [TestMethod]
        public void UploadPrescription_ShouldFail_WhenSourceFileDoesNotExist()
        {
            OperationResult<string> result = _service.UploadPrescription(1, @"C:\does\not\exist.jpg");

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void UploadPrescription_ShouldFail_WhenExtensionNotAllowed()
        {
            _tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");
            File.WriteAllText(_tempFile, "not an image");

            OperationResult<string> result = _service.UploadPrescription(1, _tempFile);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Message, "jpg");
        }

        [TestMethod]
        public void UploadPrescription_ShouldFail_WhenFileExceedsMaxSize()
        {
            _tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".jpg");
            byte[] oversized = new byte[5 * 1024 * 1024 + 1];
            File.WriteAllBytes(_tempFile, oversized);

            OperationResult<string> result = _service.UploadPrescription(1, _tempFile);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Message, "5 MB");
        }

        [TestMethod]
        public void UploadPrescription_ShouldSucceed_ForValidJpgFile()
        {
            _tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".jpg");
            File.WriteAllBytes(_tempFile, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });

            OperationResult<string> result = _service.UploadPrescription(1, _tempFile);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_orderRepository.UpdatePrescriptionPathCalled);
            Assert.IsTrue(_auditLogRepository.LogCalled);
        }

        [TestMethod]
        public void UploadPrescription_ShouldFail_WhenSourcePathIsBlank()
        {
            OperationResult<string> result = _service.UploadPrescription(1, "");

            Assert.IsFalse(result.IsSuccess);
        }
    }
}
