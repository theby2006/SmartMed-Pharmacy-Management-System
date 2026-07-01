using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Common.Exceptions;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.Tests.BLL
{
    [TestClass]
    public class SupplierServiceTests
    {
        private MockSupplierRepository _supplierRepository;
        private MockAuditLogRepository _auditLogRepository;
        private MockSessionManager _sessionManager;
        private ISupplierService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _supplierRepository = new MockSupplierRepository();
            _auditLogRepository = new MockAuditLogRepository();
            _sessionManager = new MockSessionManager();
            _service = new SupplierService(_supplierRepository, _auditLogRepository, _sessionManager);
        }

        [TestMethod]
        public void GetAllSuppliers_ShouldReturnSuccess()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Supplier A" },
                new Supplier { Id = 2, SupplierCode = "SUP002", SupplierName = "Supplier B" }
            };

            OperationResult<List<Supplier>> result = _service.GetAllSuppliers();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, result.Data.Count);
        }

        [TestMethod]
        public void GetAllSuppliers_ShouldReturnEmptyList_WhenNoSuppliers()
        {
            OperationResult<List<Supplier>> result = _service.GetAllSuppliers();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Data.Count);
        }

        [TestMethod]
        public void GetSupplierById_ShouldReturnSuccess_WhenFound()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Supplier A" }
            };

            OperationResult<Supplier> result = _service.GetSupplierById(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("SUP001", result.Data.SupplierCode);
        }

        [TestMethod]
        public void GetSupplierById_ShouldReturnFailure_WhenNotFound()
        {
            OperationResult<Supplier> result = _service.GetSupplierById(999);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Supplier not found.", result.Message);
        }

        [TestMethod]
        public void SearchSuppliers_ShouldReturnSuccess()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "MediHealth" },
                new Supplier { Id = 2, SupplierCode = "SUP002", SupplierName = "PharmaPlus" }
            };

            OperationResult<List<Supplier>> result = _service.SearchSuppliers("Pharma");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void SearchSuppliers_ShouldReturnFailure_WhenKeywordIsEmpty()
        {
            OperationResult<List<Supplier>> result = _service.SearchSuppliers("");

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void AddSupplier_ShouldReturnSuccess_WithValidSupplier()
        {
            var supplier = new Supplier
            {
                SupplierCode = "SUP003",
                SupplierName = "BioCare Solutions",
                PhoneNumber = "+1-555-0100",
                Email = "test@biocare.com"
            };

            OperationResult<int> result = _service.AddSupplier(supplier);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_supplierRepository.AddCalled);
        }

        [TestMethod]
        public void AddSupplier_ShouldReturnFailure_WhenSupplierIsNull()
        {
            OperationResult<int> result = _service.AddSupplier(null);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void AddSupplier_ShouldReturnFailure_WhenSupplierCodeIsEmpty()
        {
            var supplier = new Supplier
            {
                SupplierCode = "",
                SupplierName = "Test Supplier"
            };

            OperationResult<int> result = _service.AddSupplier(supplier);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void AddSupplier_ShouldReturnFailure_WhenSupplierNameIsEmpty()
        {
            var supplier = new Supplier
            {
                SupplierCode = "SUP004",
                SupplierName = ""
            };

            OperationResult<int> result = _service.AddSupplier(supplier);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void AddSupplier_ShouldReturnFailure_WhenSupplierCodeTooLong()
        {
            var supplier = new Supplier
            {
                SupplierCode = new string('X', 51),
                SupplierName = "Test Supplier"
            };

            OperationResult<int> result = _service.AddSupplier(supplier);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("50"));
        }

        [TestMethod]
        public void AddSupplier_ShouldReturnFailure_WhenSupplierNameTooLong()
        {
            var supplier = new Supplier
            {
                SupplierCode = "SUP005",
                SupplierName = new string('X', 201)
            };

            OperationResult<int> result = _service.AddSupplier(supplier);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("200"));
        }

        [TestMethod]
        public void AddSupplier_ShouldReturnFailure_WhenEmailInvalidFormat()
        {
            var supplier = new Supplier
            {
                SupplierCode = "SUP006",
                SupplierName = "Test Supplier",
                Email = "not-an-email"
            };

            OperationResult<int> result = _service.AddSupplier(supplier);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("email"));
        }

        [TestMethod]
        public void AddSupplier_ShouldReturnFailure_WhenPhoneInvalidFormat()
        {
            var supplier = new Supplier
            {
                SupplierCode = "SUP007",
                SupplierName = "Test Supplier",
                PhoneNumber = "abc"
            };

            OperationResult<int> result = _service.AddSupplier(supplier);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("phone"));
        }

        [TestMethod]
        public void AddSupplier_ShouldReturnFailure_WhenDuplicateSupplierCode()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Existing" }
            };
            var supplier = new Supplier
            {
                SupplierCode = "SUP001",
                SupplierName = "New Supplier"
            };

            OperationResult<int> result = _service.AddSupplier(supplier);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("code already exists"));
        }

        [TestMethod]
        public void AddSupplier_ShouldReturnFailure_WhenDuplicateSupplierName()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Existing Supplier" }
            };
            var supplier = new Supplier
            {
                SupplierCode = "SUP002",
                SupplierName = "Existing Supplier"
            };

            OperationResult<int> result = _service.AddSupplier(supplier);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("name already exists"));
        }

        [TestMethod]
        public void UpdateSupplier_ShouldReturnSuccess_WithValidSupplier()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "MediHealth" }
            };
            var supplier = new Supplier
            {
                Id = 1,
                SupplierCode = "SUP001",
                SupplierName = "MediHealth Updated"
            };

            OperationResult result = _service.UpdateSupplier(supplier);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_supplierRepository.UpdateCalled);
        }

        [TestMethod]
        public void UpdateSupplier_ShouldReturnFailure_WhenSupplierIsNull()
        {
            OperationResult result = _service.UpdateSupplier(null);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void UpdateSupplier_ShouldReturnFailure_WhenSupplierNotFound()
        {
            var supplier = new Supplier
            {
                Id = 999,
                SupplierCode = "SUP999",
                SupplierName = "Non Existent"
            };

            OperationResult result = _service.UpdateSupplier(supplier);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Supplier not found.", result.Message);
        }

        [TestMethod]
        public void UpdateSupplier_ShouldReturnFailure_WhenDuplicateSupplierCode()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Supplier A" },
                new Supplier { Id = 2, SupplierCode = "SUP002", SupplierName = "Supplier B" }
            };
            var supplier = new Supplier
            {
                Id = 1,
                SupplierCode = "SUP002",
                SupplierName = "Supplier A"
            };

            OperationResult result = _service.UpdateSupplier(supplier);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("code already exists"));
        }

        [TestMethod]
        public void UpdateSupplier_ShouldReturnFailure_WhenDuplicateSupplierName()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Supplier A" },
                new Supplier { Id = 2, SupplierCode = "SUP002", SupplierName = "Supplier B" }
            };
            var supplier = new Supplier
            {
                Id = 1,
                SupplierCode = "SUP001",
                SupplierName = "Supplier B"
            };

            OperationResult result = _service.UpdateSupplier(supplier);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("name already exists"));
        }

        [TestMethod]
        public void UpdateSupplier_ShouldAllowSameCode_ForSameSupplier()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Supplier A" }
            };
            var supplier = new Supplier
            {
                Id = 1,
                SupplierCode = "SUP001",
                SupplierName = "Supplier A Updated"
            };

            OperationResult result = _service.UpdateSupplier(supplier);

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void DeleteSupplier_ShouldReturnSuccess()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Supplier A" }
            };

            OperationResult result = _service.DeleteSupplier(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_supplierRepository.DeleteCalled);
        }

        [TestMethod]
        public void DeleteSupplier_ShouldReturnFailure_WhenSupplierNotFound()
        {
            OperationResult result = _service.DeleteSupplier(999);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Supplier not found.", result.Message);
            Assert.IsFalse(_supplierRepository.DeleteCalled);
        }

        [TestMethod]
        public void DeleteSupplier_ShouldReturnFailure_WhenSupplierHasPurchases()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Supplier A" }
            };
            _supplierRepository.SuppliersWithPurchases = new List<int> { 1 };

            OperationResult result = _service.DeleteSupplier(1);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("purchase"));
            Assert.IsFalse(_supplierRepository.DeleteCalled);
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new SupplierService(null, _auditLogRepository, _sessionManager));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenAuditLogRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new SupplierService(_supplierRepository, null, _sessionManager));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenSessionManagerIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new SupplierService(_supplierRepository, _auditLogRepository, null));
        }

        [TestMethod]
        public void AddSupplier_ShouldLogAudit_OnSuccess()
        {
            var supplier = new Supplier
            {
                SupplierCode = "SUP003",
                SupplierName = "BioCare Solutions",
                PhoneNumber = "+1-555-0100",
                Email = "test@biocare.com"
            };

            _service.AddSupplier(supplier);

            Assert.IsTrue(_auditLogRepository.LogCalled);
            Assert.AreEqual(AuditAction.SupplierAdded, _auditLogRepository.LoggedAction);
        }

        [TestMethod]
        public void UpdateSupplier_ShouldLogAudit_OnSuccess()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "MediHealth" }
            };
            var supplier = new Supplier
            {
                Id = 1,
                SupplierCode = "SUP001",
                SupplierName = "MediHealth Updated"
            };

            _service.UpdateSupplier(supplier);

            Assert.IsTrue(_auditLogRepository.LogCalled);
            Assert.AreEqual(AuditAction.SupplierUpdated, _auditLogRepository.LoggedAction);
        }

        [TestMethod]
        public void DeleteSupplier_ShouldLogAudit_OnSuccess()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Supplier A" }
            };

            _service.DeleteSupplier(1);

            Assert.IsTrue(_auditLogRepository.LogCalled);
            Assert.AreEqual(AuditAction.SupplierDeleted, _auditLogRepository.LoggedAction);
        }
    }
}
