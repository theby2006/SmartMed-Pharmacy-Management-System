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
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ISessionManager _sessionManager;

        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex PhoneRegex = new Regex(
            @"^\+?[\d\s\-\(\)]{7,20}$", RegexOptions.Compiled);

        public SupplierService(ISupplierRepository supplierRepository,
            IAuditLogRepository auditLogRepository, ISessionManager sessionManager)
        {
            Guard.AgainstNull(supplierRepository, nameof(supplierRepository));
            Guard.AgainstNull(auditLogRepository, nameof(auditLogRepository));
            Guard.AgainstNull(sessionManager, nameof(sessionManager));
            _supplierRepository = supplierRepository;
            _auditLogRepository = auditLogRepository;
            _sessionManager = sessionManager;
        }

        private string CurrentMachineName => Environment.MachineName;

        private int? CurrentUserId => _sessionManager.CurrentSession?.UserId;

        private string CurrentUsername => _sessionManager.CurrentSession?.Username ?? "System";

        public OperationResult<List<Supplier>> GetAllSuppliers()
        {
            try
            {
                List<Supplier> suppliers = _supplierRepository.GetAll();
                return OperationResult<List<Supplier>>.Success(suppliers);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<Supplier>>.Failure(ex.Message);
            }
        }

        public OperationResult<Supplier> GetSupplierById(int id)
        {
            try
            {
                Supplier supplier = _supplierRepository.GetById(id);
                if (supplier == null)
                    return OperationResult<Supplier>.Failure("Supplier not found.");
                return OperationResult<Supplier>.Success(supplier);
            }
            catch (ValidationException ex)
            {
                return OperationResult<Supplier>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Supplier>> SearchSuppliers(string keyword)
        {
            try
            {
                Guard.AgainstNullOrWhiteSpace(keyword, nameof(keyword));
                List<Supplier> suppliers = _supplierRepository.Search(keyword);
                return OperationResult<List<Supplier>>.Success(suppliers);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<Supplier>>.Failure(ex.Message);
            }
        }

        public OperationResult<int> AddSupplier(Supplier supplier)
        {
            try
            {
                Guard.AgainstNull(supplier, nameof(supplier));
                Guard.AgainstNullOrWhiteSpace(supplier.SupplierCode, nameof(supplier.SupplierCode));
                Guard.AgainstNullOrWhiteSpace(supplier.SupplierName, nameof(supplier.SupplierName));

                if (supplier.SupplierCode.Length > 50)
                    return OperationResult<int>.Failure("Supplier code must not exceed 50 characters.");

                if (supplier.SupplierName.Length > 200)
                    return OperationResult<int>.Failure("Supplier name must not exceed 200 characters.");

                if (supplier.CompanyName != null && supplier.CompanyName.Length > 200)
                    return OperationResult<int>.Failure("Company name must not exceed 200 characters.");

                if (supplier.ContactPerson != null && supplier.ContactPerson.Length > 100)
                    return OperationResult<int>.Failure("Contact person must not exceed 100 characters.");

                if (supplier.PhoneNumber != null)
                {
                    if (supplier.PhoneNumber.Length > 20)
                        return OperationResult<int>.Failure("Phone number must not exceed 20 characters.");
                    if (!PhoneRegex.IsMatch(supplier.PhoneNumber))
                        return OperationResult<int>.Failure("Invalid phone number format.");
                }

                if (supplier.Email != null)
                {
                    if (supplier.Email.Length > 100)
                        return OperationResult<int>.Failure("Email must not exceed 100 characters.");
                    if (!EmailRegex.IsMatch(supplier.Email))
                        return OperationResult<int>.Failure("Invalid email format.");
                }

                if (supplier.Address != null && supplier.Address.Length > 500)
                    return OperationResult<int>.Failure("Address must not exceed 500 characters.");

                if (supplier.City != null && supplier.City.Length > 100)
                    return OperationResult<int>.Failure("City must not exceed 100 characters.");

                if (supplier.Country != null && supplier.Country.Length > 100)
                    return OperationResult<int>.Failure("Country must not exceed 100 characters.");

                if (supplier.PostalCode != null && supplier.PostalCode.Length > 20)
                    return OperationResult<int>.Failure("Postal code must not exceed 20 characters.");

                if (supplier.TaxNumber != null && supplier.TaxNumber.Length > 50)
                    return OperationResult<int>.Failure("Tax number must not exceed 50 characters.");

                if (supplier.Notes != null && supplier.Notes.Length > 1000)
                    return OperationResult<int>.Failure("Notes must not exceed 1000 characters.");

                Supplier existingByCode = _supplierRepository.GetBySupplierCode(supplier.SupplierCode);
                if (existingByCode != null)
                    return OperationResult<int>.Failure("A supplier with this code already exists.");

                Supplier existingByName = _supplierRepository.GetByName(supplier.SupplierName);
                if (existingByName != null)
                    return OperationResult<int>.Failure("A supplier with this name already exists.");

                int id = _supplierRepository.Add(supplier);

                _auditLogRepository.Log(CurrentUserId, CurrentUsername, AuditAction.SupplierAdded,
                    CurrentMachineName, $"Added supplier '{supplier.SupplierName}' (Code: {supplier.SupplierCode})");

                return OperationResult<int>.Success(id, "Supplier added successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult<int>.Failure(ex.Message);
            }
        }

        public OperationResult UpdateSupplier(Supplier supplier)
        {
            try
            {
                Guard.AgainstNull(supplier, nameof(supplier));
                Guard.AgainstNullOrWhiteSpace(supplier.SupplierCode, nameof(supplier.SupplierCode));
                Guard.AgainstNullOrWhiteSpace(supplier.SupplierName, nameof(supplier.SupplierName));

                if (supplier.SupplierCode.Length > 50)
                    return OperationResult.Failure("Supplier code must not exceed 50 characters.");

                if (supplier.SupplierName.Length > 200)
                    return OperationResult.Failure("Supplier name must not exceed 200 characters.");

                if (supplier.CompanyName != null && supplier.CompanyName.Length > 200)
                    return OperationResult.Failure("Company name must not exceed 200 characters.");

                if (supplier.ContactPerson != null && supplier.ContactPerson.Length > 100)
                    return OperationResult.Failure("Contact person must not exceed 100 characters.");

                if (supplier.PhoneNumber != null)
                {
                    if (supplier.PhoneNumber.Length > 20)
                        return OperationResult.Failure("Phone number must not exceed 20 characters.");
                    if (!PhoneRegex.IsMatch(supplier.PhoneNumber))
                        return OperationResult.Failure("Invalid phone number format.");
                }

                if (supplier.Email != null)
                {
                    if (supplier.Email.Length > 100)
                        return OperationResult.Failure("Email must not exceed 100 characters.");
                    if (!EmailRegex.IsMatch(supplier.Email))
                        return OperationResult.Failure("Invalid email format.");
                }

                if (supplier.Address != null && supplier.Address.Length > 500)
                    return OperationResult.Failure("Address must not exceed 500 characters.");

                if (supplier.City != null && supplier.City.Length > 100)
                    return OperationResult.Failure("City must not exceed 100 characters.");

                if (supplier.Country != null && supplier.Country.Length > 100)
                    return OperationResult.Failure("Country must not exceed 100 characters.");

                if (supplier.PostalCode != null && supplier.PostalCode.Length > 20)
                    return OperationResult.Failure("Postal code must not exceed 20 characters.");

                if (supplier.TaxNumber != null && supplier.TaxNumber.Length > 50)
                    return OperationResult.Failure("Tax number must not exceed 50 characters.");

                if (supplier.Notes != null && supplier.Notes.Length > 1000)
                    return OperationResult.Failure("Notes must not exceed 1000 characters.");

                Supplier existing = _supplierRepository.GetById(supplier.Id);
                if (existing == null)
                    return OperationResult.Failure("Supplier not found.");

                Supplier existingByCode = _supplierRepository.GetBySupplierCode(supplier.SupplierCode);
                if (existingByCode != null && existingByCode.Id != supplier.Id)
                    return OperationResult.Failure("A supplier with this code already exists.");

                Supplier existingByName = _supplierRepository.GetByName(supplier.SupplierName);
                if (existingByName != null && existingByName.Id != supplier.Id)
                    return OperationResult.Failure("A supplier with this name already exists.");

                _supplierRepository.Update(supplier);

                _auditLogRepository.Log(CurrentUserId, CurrentUsername, AuditAction.SupplierUpdated,
                    CurrentMachineName, $"Updated supplier '{supplier.SupplierName}' (Code: {supplier.SupplierCode})");

                return OperationResult.Success("Supplier updated successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult DeleteSupplier(int id)
        {
            try
            {
                Supplier supplier = _supplierRepository.GetById(id);
                if (supplier == null)
                    return OperationResult.Failure("Supplier not found.");

                if (_supplierRepository.HasPurchases(id))
                    return OperationResult.Failure(
                        "Cannot delete supplier with existing purchase records. Remove or reassign the purchases first.");

                string name = supplier.SupplierName;
                string code = supplier.SupplierCode;

                _supplierRepository.Delete(id);

                _auditLogRepository.Log(CurrentUserId, CurrentUsername, AuditAction.SupplierDeleted,
                    CurrentMachineName, $"Deleted supplier '{name}' (Code: {code})");

                return OperationResult.Success("Supplier deleted successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }
    }
}
