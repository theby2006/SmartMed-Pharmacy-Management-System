using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Common.Exceptions;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;
using SmartMed.Models.Session;
using System;
using System.Collections.Generic;

namespace SmartMed.Tests.BLL
{
    [TestClass]
    public class AuthenticationServiceTests
    {
        private MockUserRepository _userRepository;
        private MockPasswordHasher _passwordHasher;
        private MockSessionManager _sessionManager;
        private MockAuditLogRepository _auditLogRepository;
        private IAuthenticationService _authService;

        private User _activeAdminUser;
        private User _inactiveUser;
        private User _lockedUser;
        private User _customerUser;

        [TestInitialize]
        public void TestInitialize()
        {
            _activeAdminUser = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = Convert.ToBase64String(new byte[32]),
                PasswordSalt = Convert.ToBase64String(new byte[16]),
                DisplayName = "Admin User",
                Role = RoleType.Administrator,
                IsActive = true,
                FailedLoginAttempts = 0,
                LockedUntil = null
            };

            _inactiveUser = new User
            {
                Id = 2,
                Username = "disabled",
                PasswordHash = Convert.ToBase64String(new byte[32]),
                PasswordSalt = Convert.ToBase64String(new byte[16]),
                DisplayName = "Disabled User",
                Role = RoleType.Pharmacist,
                IsActive = false,
                FailedLoginAttempts = 0,
                LockedUntil = null
            };

            _lockedUser = new User
            {
                Id = 3,
                Username = "locked",
                PasswordHash = Convert.ToBase64String(new byte[32]),
                PasswordSalt = Convert.ToBase64String(new byte[16]),
                DisplayName = "Locked User",
                Role = RoleType.Cashier,
                IsActive = true,
                FailedLoginAttempts = 5,
                LockedUntil = DateTime.UtcNow.AddMinutes(15)
            };

            _customerUser = new User
            {
                Id = 4,
                Username = "customer@test.com",
                PasswordHash = Convert.ToBase64String(new byte[32]),
                PasswordSalt = Convert.ToBase64String(new byte[16]),
                DisplayName = "Test Customer",
                Role = RoleType.Customer,
                IsActive = true,
                FailedLoginAttempts = 0,
                LockedUntil = null
            };

            _userRepository = new MockUserRepository(_activeAdminUser, _inactiveUser, _lockedUser, _customerUser);
            _passwordHasher = new MockPasswordHasher();
            _sessionManager = new MockSessionManager();
            _auditLogRepository = new MockAuditLogRepository();

            _authService = new AuthenticationService(
                _userRepository,
                _passwordHasher,
                _sessionManager,
                _auditLogRepository);
        }

        [TestMethod]
        public void LoginAdmin_ShouldReturnSuccess_WithValidCredentials()
        {
            OperationResult<SessionContext> result = _authService.LoginAdmin("admin", "correct");

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data.IsAuthenticated);
            Assert.AreEqual("admin", result.Data.Username);
        }

        [TestMethod]
        public void LoginAdmin_ShouldReturnFailure_WithInvalidPassword()
        {
            OperationResult<SessionContext> result = _authService.LoginAdmin("admin", "wrong");

            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
        }

        [TestMethod]
        public void LoginAdmin_ShouldReturnFailure_ForUnknownUsername()
        {
            OperationResult<SessionContext> result = _authService.LoginAdmin("unknown", "password");

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("Invalid username or password"));
        }

        [TestMethod]
        public void LoginAdmin_ShouldReturnFailure_ForInactiveUser()
        {
            OperationResult<SessionContext> result = _authService.LoginAdmin("disabled", "password");

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("disabled"));
        }

        [TestMethod]
        public void LoginAdmin_ShouldReturnFailure_ForLockedAccount()
        {
            OperationResult<SessionContext> result = _authService.LoginAdmin("locked", "password");

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("locked"));
        }

        [TestMethod]
        public void LoginAdmin_ShouldIncrementFailedAttempts_OnFailedPassword()
        {
            _authService.LoginAdmin("admin", "wrong");

            Assert.IsTrue(_userRepository.IncrementFailedAttemptsCalled);
        }

        [TestMethod]
        public void LoginAdmin_ShouldLockAccount_AfterMaxFailedAttempts()
        {
            _activeAdminUser.FailedLoginAttempts = 4;

            _authService.LoginAdmin("admin", "wrong");

            Assert.IsTrue(_userRepository.SetLockedUntilCalled);
        }

        [TestMethod]
        public void LoginAdmin_ShouldResetFailedAttempts_OnSuccessfulLogin()
        {
            _authService.LoginAdmin("admin", "correct");

            Assert.IsTrue(_userRepository.ResetFailedAttemptsCalled);
        }

        [TestMethod]
        public void LoginAdmin_ShouldLogAudit_OnSuccessfulLogin()
        {
            _authService.LoginAdmin("admin", "correct");

            Assert.IsTrue(_auditLogRepository.LogLoginCalled);
        }

        [TestMethod]
        public void LoginAdmin_ShouldLogAudit_OnFailedLogin()
        {
            _authService.LoginAdmin("admin", "wrong");

            Assert.IsTrue(_auditLogRepository.LogFailedAttemptCalled);
        }

        [TestMethod]
        public void LoginAdmin_ShouldAutoUnlock_WhenLockExpired()
        {
            _lockedUser.LockedUntil = DateTime.UtcNow.AddMinutes(-5);

            OperationResult<SessionContext> result = _authService.LoginAdmin("locked", "correct");

            Assert.IsTrue(_userRepository.SetLockedUntilCalled);
        }

        [TestMethod]
        public void Logout_ShouldClearSessionAndLogAudit()
        {
            _authService.LoginAdmin("admin", "correct");

            OperationResult result = _authService.Logout();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(_authService.IsAuthenticated);
            Assert.IsTrue(_auditLogRepository.LogLogoutCalled);
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenUserRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new AuthenticationService(null, _passwordHasher, _sessionManager, _auditLogRepository));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenPasswordHasherIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new AuthenticationService(_userRepository, null, _sessionManager, _auditLogRepository));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenSessionManagerIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new AuthenticationService(_userRepository, _passwordHasher, null, _auditLogRepository));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenAuditLogRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new AuthenticationService(_userRepository, _passwordHasher, _sessionManager, null));
        }

        [TestMethod]
        public void LoginCustomer_ShouldReturnSuccess_WithValidIdentifier()
        {
            _customerUser.Role = RoleType.Customer;

            OperationResult<SessionContext> result = _authService.LoginCustomer("customer@test.com");

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void LoginCustomer_ShouldReturnFailure_ForUnknownIdentifier()
        {
            OperationResult<SessionContext> result = _authService.LoginCustomer("unknown@test.com");

            Assert.IsFalse(result.IsSuccess);
        }
    }

    internal class MockUserRepository : IUserRepository
    {
        private readonly User _activeAdmin;
        private readonly User _inactive;
        private readonly User _locked;
        private readonly User _customer;

        public bool IncrementFailedAttemptsCalled { get; private set; }
        public bool ResetFailedAttemptsCalled { get; private set; }
        public bool SetLockedUntilCalled { get; private set; }
        public bool UpdateLastLoginCalled { get; private set; }

        public MockUserRepository(User activeAdmin, User inactive, User locked, User customer)
        {
            _activeAdmin = activeAdmin;
            _inactive = inactive;
            _locked = locked;
            _customer = customer;
        }

        public User GetById(int userId)
        {
            if (userId == _activeAdmin.Id) return _activeAdmin;
            if (userId == _inactive.Id) return _inactive;
            if (userId == _locked.Id) return _locked;
            if (userId == _customer.Id) return _customer;
            return null;
        }

        public User GetByUsername(string username)
        {
            if (username == _activeAdmin.Username) return _activeAdmin;
            if (username == _inactive.Username) return _inactive;
            if (username == _locked.Username) return _locked;
            if (username == _customer.Username) return _customer;
            return null;
        }

        public void IncrementFailedAttempts(int userId)
        {
            IncrementFailedAttemptsCalled = true;
        }

        public void ResetFailedAttempts(int userId)
        {
            ResetFailedAttemptsCalled = true;
        }

        public void SetLockedUntil(int userId, DateTime? lockedUntil)
        {
            SetLockedUntilCalled = true;
        }

        public void UpdateLastLogin(int userId, DateTime loginTime)
        {
            UpdateLastLoginCalled = true;
        }

        public List<User> GetAll() => new List<User>();

        public List<User> Search(string keyword) => new List<User>();

        public int Add(User user) => 0;

        public void Update(User user) { }

        public void UpdatePassword(int userId, string passwordHash, string passwordSalt) { }

        public void UpdateActiveStatus(int userId, bool isActive) { }
    }

    internal class MockPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password, string salt)
        {
            return Convert.ToBase64String(new byte[32]);
        }

        public bool VerifyPassword(string password, string hash, string salt)
        {
            return password == "correct";
        }

        public string GenerateSalt()
        {
            return Convert.ToBase64String(new byte[16]);
        }
    }
}
