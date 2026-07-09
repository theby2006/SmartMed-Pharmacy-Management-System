using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.Tests.BLL
{
    [TestClass]
    public class CustomerServiceTests
    {
        private MockCustomerRepository _customerRepository;
        private MockPasswordHasher _passwordHasher;
        private MockAuditLogRepository _auditLogRepository;
        private MockSessionManager _sessionManager;
        private ICustomerService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _customerRepository = new MockCustomerRepository();
            _passwordHasher = new MockPasswordHasher();
            _auditLogRepository = new MockAuditLogRepository();
            _sessionManager = new MockSessionManager();
            _service = new CustomerService(_customerRepository, _passwordHasher, _auditLogRepository, _sessionManager);
        }

        private static Customer ValidCustomer()
        {
            return new Customer
            {
                FullName = "Jane Doe",
                PhoneNumber = "0771234567",
                Email = "jane.doe@example.com",
                Address = "12 Lakeview Road",
                City = "Colombo"
            };
        }

        [TestMethod]
        public void RegisterCustomer_ShouldSucceed_WithValidData()
        {
            OperationResult<int> result = _service.RegisterCustomer(ValidCustomer(), "1234");

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_customerRepository.AddCalled);
        }

        [TestMethod]
        public void RegisterCustomer_ShouldSucceed_WithoutPin()
        {
            OperationResult<int> result = _service.RegisterCustomer(ValidCustomer(), null);

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void RegisterCustomer_ShouldFail_WhenFullNameMissing()
        {
            Customer customer = ValidCustomer();
            customer.FullName = "";

            OperationResult<int> result = _service.RegisterCustomer(customer, null);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void RegisterCustomer_ShouldFail_WithInvalidPhoneFormat()
        {
            Customer customer = ValidCustomer();
            customer.PhoneNumber = "abc";

            OperationResult<int> result = _service.RegisterCustomer(customer, null);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void RegisterCustomer_ShouldFail_WithInvalidEmailFormat()
        {
            Customer customer = ValidCustomer();
            customer.Email = "not-an-email";

            OperationResult<int> result = _service.RegisterCustomer(customer, null);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void RegisterCustomer_ShouldFail_WithNonNumericPin()
        {
            OperationResult<int> result = _service.RegisterCustomer(ValidCustomer(), "abcd");

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void RegisterCustomer_ShouldFail_WhenPhoneOrEmailAlreadyExists()
        {
            _customerRepository.Add(ValidCustomer());

            OperationResult<int> result = _service.RegisterCustomer(ValidCustomer(), null);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void RegisterCustomer_ShouldLogAudit_OnSuccess()
        {
            _service.RegisterCustomer(ValidCustomer(), null);

            Assert.IsTrue(_auditLogRepository.LogCalled);
        }

        [TestMethod]
        public void GetAllCustomers_ShouldFail_WhenNotLoggedIn()
        {
            OperationResult<List<Customer>> result = _service.GetAllCustomers();

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void GetAllCustomers_ShouldSucceed_ForAdministrator()
        {
            _sessionManager.StartSession(new User { Id = 1, Username = "admin", DisplayName = "Admin", Role = RoleType.Administrator });

            OperationResult<List<Customer>> result = _service.GetAllCustomers();

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void DeactivateCustomer_ShouldFail_ForNonAdministrator()
        {
            _sessionManager.StartSession(new User { Id = 1, Username = "cashier", DisplayName = "Cashier", Role = RoleType.Cashier });
            int id = _customerRepository.Add(ValidCustomer());

            OperationResult result = _service.DeactivateCustomer(id);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(_customerRepository.UpdateActiveStatusCalled);
        }

        [TestMethod]
        public void DeactivateCustomer_ShouldSucceed_ForAdministrator()
        {
            _sessionManager.StartSession(new User { Id = 1, Username = "admin", DisplayName = "Admin", Role = RoleType.Administrator });
            int id = _customerRepository.Add(ValidCustomer());

            OperationResult result = _service.DeactivateCustomer(id);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_customerRepository.UpdateActiveStatusCalled);
        }

        [TestMethod]
        public void DeactivateCustomer_ShouldFail_WhenCustomerNotFound()
        {
            _sessionManager.StartSession(new User { Id = 1, Username = "admin", DisplayName = "Admin", Role = RoleType.Administrator });

            OperationResult result = _service.DeactivateCustomer(999);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void GetCustomerById_ShouldSucceed_ForSelf()
        {
            int id = _customerRepository.Add(ValidCustomer());
            _sessionManager.StartCustomerSession(new Customer { Id = id, FullName = "Jane Doe", PhoneNumber = "0771234567" });

            OperationResult<Customer> result = _service.GetCustomerById(id);

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void GetCustomerById_ShouldFail_ForDifferentUnauthenticatedCaller()
        {
            int id = _customerRepository.Add(ValidCustomer());

            OperationResult<Customer> result = _service.GetCustomerById(id);

            Assert.IsFalse(result.IsSuccess);
        }
    }
}
