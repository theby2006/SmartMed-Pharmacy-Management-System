using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ISessionManager _sessionManager;

        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex PhoneRegex = new Regex(
            @"^\+?[\d\s\-\(\)]{7,20}$", RegexOptions.Compiled);

        private static readonly Regex PinRegex = new Regex(@"^\d{4,8}$", RegexOptions.Compiled);

        public CustomerService(
            ICustomerRepository customerRepository,
            IPasswordHasher passwordHasher,
            IAuditLogRepository auditLogRepository,
            ISessionManager sessionManager)
        {
            Guard.AgainstNull(customerRepository, nameof(customerRepository));
            Guard.AgainstNull(passwordHasher, nameof(passwordHasher));
            Guard.AgainstNull(auditLogRepository, nameof(auditLogRepository));
            Guard.AgainstNull(sessionManager, nameof(sessionManager));

            _customerRepository = customerRepository;
            _passwordHasher = passwordHasher;
            _auditLogRepository = auditLogRepository;
            _sessionManager = sessionManager;
        }

        private string CurrentMachineName => Environment.MachineName;

        private int? CurrentUserId => _sessionManager.CurrentSession?.UserId;

        private string CurrentUsername => _sessionManager.CurrentSession?.Username ?? "System";

        private OperationResult AuthorizeRole(params RoleType[] allowedRoles)
        {
            if (!_sessionManager.IsActive || _sessionManager.CurrentSession == null)
                return OperationResult.Failure("You must be logged in to perform this operation.");

            foreach (RoleType role in allowedRoles)
            {
                if (_sessionManager.HasRole(role))
                    return OperationResult.Success();
            }

            return OperationResult.Failure("You are not authorized to perform this operation.");
        }

        public OperationResult<int> RegisterCustomer(Customer customer, string pin)
        {
            try
            {
                Guard.AgainstNull(customer, nameof(customer));
                Guard.AgainstNullOrWhiteSpace(customer.FullName, nameof(customer.FullName));
                Guard.AgainstNullOrWhiteSpace(customer.PhoneNumber, nameof(customer.PhoneNumber));

                if (customer.FullName.Length > 200)
                    return OperationResult<int>.Failure("Full name must not exceed 200 characters.");

                if (customer.PhoneNumber.Length > 20 || !PhoneRegex.IsMatch(customer.PhoneNumber))
                    return OperationResult<int>.Failure("Invalid phone number format.");

                if (customer.Email != null)
                {
                    if (customer.Email.Length > 200)
                        return OperationResult<int>.Failure("Email must not exceed 200 characters.");
                    if (!EmailRegex.IsMatch(customer.Email))
                        return OperationResult<int>.Failure("Invalid email format.");
                }

                if (customer.Address != null && customer.Address.Length > 500)
                    return OperationResult<int>.Failure("Address must not exceed 500 characters.");

                if (customer.City != null && customer.City.Length > 100)
                    return OperationResult<int>.Failure("City must not exceed 100 characters.");

                if (pin != null && !PinRegex.IsMatch(pin))
                    return OperationResult<int>.Failure("PIN must be 4 to 8 digits.");

                if (_customerRepository.ExistsByPhoneOrEmail(customer.PhoneNumber, customer.Email))
                    return OperationResult<int>.Failure("A customer with this phone number or email already exists.");

                if (!string.IsNullOrEmpty(pin))
                {
                    customer.PinSalt = _passwordHasher.GenerateSalt();
                    customer.PinHash = _passwordHasher.HashPassword(pin, customer.PinSalt);
                }

                int id = _customerRepository.Add(customer);

                _auditLogRepository.Log(CurrentUserId, CurrentUsername, AuditAction.CustomerRegistered,
                    CurrentMachineName, $"Registered customer '{customer.FullName}' ({customer.PhoneNumber})");

                return OperationResult<int>.Success(id, "Customer registered successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult<int>.Failure(ex.Message);
            }
        }

        public OperationResult UpdateProfile(Customer customer)
        {
            try
            {
                Guard.AgainstNull(customer, nameof(customer));
                Guard.AgainstNullOrWhiteSpace(customer.FullName, nameof(customer.FullName));
                Guard.AgainstNullOrWhiteSpace(customer.PhoneNumber, nameof(customer.PhoneNumber));

                bool isSelf = _sessionManager.CurrentSession?.CustomerId == customer.Id;
                OperationResult authResult = AuthorizeRole(RoleType.Administrator);
                if (!isSelf && !authResult.IsSuccess)
                    return authResult;

                if (customer.FullName.Length > 200)
                    return OperationResult.Failure("Full name must not exceed 200 characters.");

                if (customer.PhoneNumber.Length > 20 || !PhoneRegex.IsMatch(customer.PhoneNumber))
                    return OperationResult.Failure("Invalid phone number format.");

                if (customer.Email != null && !EmailRegex.IsMatch(customer.Email))
                    return OperationResult.Failure("Invalid email format.");

                Customer existing = _customerRepository.GetById(customer.Id);
                if (existing == null)
                    return OperationResult.Failure("Customer not found.");

                _customerRepository.Update(customer);

                _auditLogRepository.Log(CurrentUserId, CurrentUsername, AuditAction.CustomerUpdated,
                    CurrentMachineName, $"Updated customer '{customer.FullName}' (Id: {customer.Id})");

                return OperationResult.Success("Customer profile updated successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult<List<Customer>> GetAllCustomers()
        {
            OperationResult authResult = AuthorizeRole(RoleType.Administrator, RoleType.Pharmacist, RoleType.Cashier);
            if (!authResult.IsSuccess)
                return OperationResult<List<Customer>>.Failure(authResult.Message);

            try
            {
                List<Customer> customers = _customerRepository.GetAll();
                return OperationResult<List<Customer>>.Success(customers);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<Customer>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Customer>> SearchCustomers(string keyword)
        {
            OperationResult authResult = AuthorizeRole(RoleType.Administrator, RoleType.Pharmacist, RoleType.Cashier);
            if (!authResult.IsSuccess)
                return OperationResult<List<Customer>>.Failure(authResult.Message);

            try
            {
                Guard.AgainstNullOrWhiteSpace(keyword, nameof(keyword));
                List<Customer> customers = _customerRepository.Search(keyword);
                return OperationResult<List<Customer>>.Success(customers);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<Customer>>.Failure(ex.Message);
            }
        }

        public OperationResult<Customer> GetCustomerById(int id)
        {
            bool isSelf = _sessionManager.CurrentSession?.CustomerId == id;
            OperationResult authResult = AuthorizeRole(RoleType.Administrator, RoleType.Pharmacist, RoleType.Cashier);
            if (!isSelf && !authResult.IsSuccess)
                return OperationResult<Customer>.Failure(authResult.Message);

            Customer customer = _customerRepository.GetById(id);
            if (customer == null)
                return OperationResult<Customer>.Failure("Customer not found.");

            return OperationResult<Customer>.Success(customer);
        }

        public OperationResult DeactivateCustomer(int id)
        {
            OperationResult authResult = AuthorizeRole(RoleType.Administrator);
            if (!authResult.IsSuccess)
                return authResult;

            Customer customer = _customerRepository.GetById(id);
            if (customer == null)
                return OperationResult.Failure("Customer not found.");

            _customerRepository.UpdateActiveStatus(id, false);

            _auditLogRepository.Log(CurrentUserId, CurrentUsername, AuditAction.CustomerDeactivated,
                CurrentMachineName, $"Deactivated customer '{customer.FullName}' (Id: {id})");

            return OperationResult.Success("Customer deactivated successfully.");
        }
    }
}
