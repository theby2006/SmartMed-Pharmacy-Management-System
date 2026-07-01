using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;
using SmartMed.Models.Session;

namespace SmartMed.Tests.BLL
{
    [TestClass]
    public class SupplierWorkflowTests
    {
        private MockSupplierRepository _supplierRepository;
        private WorkflowAuditLogRepository _auditLogRepository;
        private WorkflowSessionManager _sessionManager;
        private ISupplierService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _supplierRepository = new MockSupplierRepository();
            _auditLogRepository = new WorkflowAuditLogRepository();
            _sessionManager = new WorkflowSessionManager();
            _service = new SupplierService(_supplierRepository, _auditLogRepository, _sessionManager);
        }

        [TestMethod]
        public void FullSupplierLifecycle_ShouldSucceed()
        {
            // Step 1: Add a new supplier
            var newSupplier = new Supplier
            {
                SupplierCode = "SUP100",
                SupplierName = "Workflow Test Supplier",
                CompanyName = "Test Corp",
                ContactPerson = "Jane Doe",
                PhoneNumber = "+1-555-9999",
                Email = "jane@testcorp.com",
                Address = "100 Test Lane",
                City = "Testville",
                Country = "Testland",
                PostalCode = "12345",
                TaxNumber = "TAX-WF-001",
                Notes = "Workflow test supplier"
            };

            OperationResult<int> addResult = _service.AddSupplier(newSupplier);
            Assert.IsTrue(addResult.IsSuccess, $"Add should succeed: {addResult.Message}");
            Assert.IsTrue(addResult.Data > 0, "Should return valid ID");
            int supplierId = addResult.Data;

            // Verify audit log was written for add
            Assert.IsTrue(_auditLogRepository.Entries.Any(e =>
                e.Action == AuditAction.SupplierAdded &&
                e.Details.Contains("Workflow Test Supplier")), "Add should be audited");

            // Step 2: Retrieve the supplier by ID
            OperationResult<Supplier> getResult = _service.GetSupplierById(supplierId);
            Assert.IsTrue(getResult.IsSuccess);
            Assert.AreEqual("SUP100", getResult.Data.SupplierCode);
            Assert.AreEqual("Workflow Test Supplier", getResult.Data.SupplierName);
            Assert.AreEqual("jane@testcorp.com", getResult.Data.Email);

            // Step 3: Search for the supplier
            OperationResult<List<Supplier>> searchResult = _service.SearchSuppliers("Workflow");
            Assert.IsTrue(searchResult.IsSuccess);
            Assert.AreEqual(1, searchResult.Data.Count);
            Assert.AreEqual(supplierId, searchResult.Data[0].Id);

            // Search by phone number
            searchResult = _service.SearchSuppliers("555-9999");
            Assert.IsTrue(searchResult.IsSuccess);
            Assert.AreEqual(1, searchResult.Data.Count);

            // Step 4: Update the supplier
            getResult.Data.ContactPerson = "Jane Smith";
            getResult.Data.PhoneNumber = "+1-555-8888";
            OperationResult updateResult = _service.UpdateSupplier(getResult.Data);
            Assert.IsTrue(updateResult.IsSuccess, $"Update should succeed: {updateResult.Message}");

            // Verify update was persisted
            OperationResult<Supplier> updatedGet = _service.GetSupplierById(supplierId);
            Assert.IsTrue(updatedGet.IsSuccess);
            Assert.AreEqual("Jane Smith", updatedGet.Data.ContactPerson);

            // Verify audit log was written for update
            Assert.IsTrue(_auditLogRepository.Entries.Any(e =>
                e.Action == AuditAction.SupplierUpdated &&
                e.Details.Contains("Workflow Test Supplier")), "Update should be audited");

            // Step 5: Verify duplicate prevention works
            var duplicateCode = new Supplier
            {
                SupplierCode = "SUP100",
                SupplierName = "Different Name"
            };
            OperationResult<int> dupCodeResult = _service.AddSupplier(duplicateCode);
            Assert.IsFalse(dupCodeResult.IsSuccess, "Duplicate code should be rejected");
            Assert.IsTrue(dupCodeResult.Message.Contains("code already exists"));

            var duplicateName = new Supplier
            {
                SupplierCode = "SUP200",
                SupplierName = "Workflow Test Supplier"
            };
            OperationResult<int> dupNameResult = _service.AddSupplier(duplicateName);
            Assert.IsFalse(dupNameResult.IsSuccess, "Duplicate name should be rejected");
            Assert.IsTrue(dupNameResult.Message.Contains("name already exists"));

            // Step 6: Verify HasPurchases prevents deletion
            _supplierRepository.SuppliersWithPurchases.Add(supplierId);
            OperationResult deleteWithPurchases = _service.DeleteSupplier(supplierId);
            Assert.IsFalse(deleteWithPurchases.IsSuccess, "Delete with purchases should be rejected");
            Assert.IsTrue(deleteWithPurchases.Message.Contains("purchase"));

            // Step 7: Remove purchase dependency and delete
            _supplierRepository.SuppliersWithPurchases.Clear();
            OperationResult deleteResult = _service.DeleteSupplier(supplierId);
            Assert.IsTrue(deleteResult.IsSuccess, $"Delete should succeed: {deleteResult.Message}");

            // Verify delete was audited
            Assert.IsTrue(_auditLogRepository.Entries.Any(e =>
                e.Action == AuditAction.SupplierDeleted &&
                e.Details.Contains("Workflow Test Supplier")), "Delete should be audited");

            // Step 8: Verify supplier is no longer retrievable
            OperationResult<Supplier> afterDelete = _service.GetSupplierById(supplierId);
            Assert.IsFalse(afterDelete.IsSuccess, "Deleted supplier should not be found");
        }
    }

    internal class WorkflowAuditLogRepository : IAuditLogRepository
    {
        public List<(AuditAction Action, string Details)> Entries { get; }
            = new List<(AuditAction, string)>();

        public void LogLogin(int userId, string username, string machineName) { }
        public void LogLogout(int? userId, string username, string machineName) { }
        public void LogFailedAttempt(string username, string machineName, string details) { }

        public void Log(int? userId, string username, AuditAction action, string machineName, string details)
        {
            Entries.Add((action, details));
        }

        public List<AuditLogEntry> GetAll(int limit = 100) => new List<AuditLogEntry>();

        public List<AuditLogEntry> GetByDateRange(DateTime from, DateTime to) => new List<AuditLogEntry>();

        public List<AuditLogEntry> GetByAction(AuditAction action) => new List<AuditLogEntry>();

        public List<AuditLogEntry> GetByUser(int? userId) => new List<AuditLogEntry>();

        public List<AuditLogEntry> Search(string keyword, int limit = 50) => new List<AuditLogEntry>();
    }

    internal class WorkflowSessionManager : ISessionManager
    {
        public SessionContext CurrentSession =>
            new SessionContext { UserId = 1, Username = "wfadmin", DisplayName = "WF Admin" };
        public bool IsActive => true;
        public bool HasRole(RoleType role) => role == RoleType.Administrator;

        public SessionContext StartSession(User user) => CurrentSession;
        public void EndSession() { }
    }
}
