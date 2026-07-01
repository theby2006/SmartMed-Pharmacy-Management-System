using System;
using System.Collections.Generic;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IMedicineRepository _medicineRepository;
        private readonly IMedicineCategoryRepository _categoryRepository;

        public MedicineService(
            IMedicineRepository medicineRepository,
            IMedicineCategoryRepository categoryRepository)
        {
            Guard.AgainstNull(medicineRepository, nameof(medicineRepository));
            Guard.AgainstNull(categoryRepository, nameof(categoryRepository));
            _medicineRepository = medicineRepository;
            _categoryRepository = categoryRepository;
        }

        public OperationResult<Medicine> GetMedicineById(int id)
        {
            try
            {
                Medicine medicine = _medicineRepository.GetById(id);
                if (medicine == null)
                    return OperationResult<Medicine>.Failure("Medicine not found.");
                return OperationResult<Medicine>.Success(medicine);
            }
            catch (ValidationException ex)
            {
                return OperationResult<Medicine>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Medicine>> GetAllMedicines()
        {
            try
            {
                List<Medicine> medicines = _medicineRepository.GetAll();
                return OperationResult<List<Medicine>>.Success(medicines);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<Medicine>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Medicine>> GetMedicinesByCategory(int categoryId)
        {
            try
            {
                List<Medicine> medicines = _medicineRepository.GetByCategoryId(categoryId);
                return OperationResult<List<Medicine>>.Success(medicines);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<Medicine>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Medicine>> SearchMedicines(string keyword)
        {
            try
            {
                Guard.AgainstNullOrWhiteSpace(keyword, nameof(keyword));
                List<Medicine> medicines = _medicineRepository.Search(keyword);
                return OperationResult<List<Medicine>>.Success(medicines);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<Medicine>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Medicine>> GetLowStockMedicines()
        {
            try
            {
                List<Medicine> medicines = _medicineRepository.GetLowStock();
                return OperationResult<List<Medicine>>.Success(medicines);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<Medicine>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Medicine>> GetNearExpiryMedicines()
        {
            try
            {
                int thresholdDays = Common.Configuration.AppSettings.GetRequiredInt(
                    Common.Constants.ConfigKeys.NearExpiryThresholdDays);
                List<Medicine> medicines = _medicineRepository.GetNearExpiry(thresholdDays);
                return OperationResult<List<Medicine>>.Success(medicines);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<Medicine>>.Failure(ex.Message);
            }
        }

        public OperationResult<int> AddMedicine(Medicine medicine)
        {
            try
            {
                Guard.AgainstNull(medicine, nameof(medicine));
                Guard.AgainstNullOrWhiteSpace(medicine.Name, nameof(medicine.Name));

                if (medicine.Name.Length > 200)
                    return OperationResult<int>.Failure("Medicine name must not exceed 200 characters.");

                if (medicine.Brand != null && medicine.Brand.Length > 200)
                    return OperationResult<int>.Failure("Brand name must not exceed 200 characters.");

                Guard.AgainstNullOrWhiteSpace(medicine.Unit, nameof(medicine.Unit));

                if (medicine.Unit.Length > 50)
                    return OperationResult<int>.Failure("Unit must not exceed 50 characters.");

                if (medicine.Strength != null && medicine.Strength.Length > 50)
                    return OperationResult<int>.Failure("Strength must not exceed 50 characters.");

                if (!Enum.IsDefined(typeof(DosageForm), medicine.DosageForm))
                    return OperationResult<int>.Failure("Invalid dosage form.");

                if (medicine.CategoryId <= 0)
                    return OperationResult<int>.Failure("Category is required.");

                Guard.AgainstNegative(medicine.StockQuantity, nameof(medicine.StockQuantity));
                Guard.AgainstNegative(medicine.ReorderLevel, nameof(medicine.ReorderLevel));
                Guard.AgainstZeroOrNegative(medicine.UnitPrice, nameof(medicine.UnitPrice));

                if (medicine.Description != null && medicine.Description.Length > 500)
                    return OperationResult<int>.Failure("Description must not exceed 500 characters.");

                MedicineCategory category = _categoryRepository.GetById(medicine.CategoryId);
                if (category == null)
                    return OperationResult<int>.Failure("Selected category does not exist.");

                Medicine duplicate = _medicineRepository.GetByNameAndBrand(medicine.Name, medicine.Brand);
                if (duplicate != null)
                    return OperationResult<int>.Failure("A medicine with this name and brand already exists.");

                if (medicine.ExpiryDate.HasValue && medicine.ExpiryDate.Value <= DateTime.UtcNow.Date)
                    return OperationResult<int>.Failure("Expiry date must be in the future.");

                int id = _medicineRepository.Add(medicine);
                return OperationResult<int>.Success(id, "Medicine added successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult<int>.Failure(ex.Message);
            }
        }

        public OperationResult UpdateMedicine(Medicine medicine)
        {
            try
            {
                Guard.AgainstNull(medicine, nameof(medicine));
                Guard.AgainstNullOrWhiteSpace(medicine.Name, nameof(medicine.Name));

                if (medicine.Name.Length > 200)
                    return OperationResult.Failure("Medicine name must not exceed 200 characters.");

                if (medicine.Brand != null && medicine.Brand.Length > 200)
                    return OperationResult.Failure("Brand name must not exceed 200 characters.");

                Guard.AgainstNullOrWhiteSpace(medicine.Unit, nameof(medicine.Unit));

                if (medicine.Unit.Length > 50)
                    return OperationResult.Failure("Unit must not exceed 50 characters.");

                if (medicine.Strength != null && medicine.Strength.Length > 50)
                    return OperationResult.Failure("Strength must not exceed 50 characters.");

                if (!Enum.IsDefined(typeof(DosageForm), medicine.DosageForm))
                    return OperationResult.Failure("Invalid dosage form.");

                if (medicine.CategoryId <= 0)
                    return OperationResult.Failure("Category is required.");

                Guard.AgainstNegative(medicine.StockQuantity, nameof(medicine.StockQuantity));
                Guard.AgainstNegative(medicine.ReorderLevel, nameof(medicine.ReorderLevel));
                Guard.AgainstZeroOrNegative(medicine.UnitPrice, nameof(medicine.UnitPrice));

                if (medicine.Description != null && medicine.Description.Length > 500)
                    return OperationResult.Failure("Description must not exceed 500 characters.");

                Medicine existing = _medicineRepository.GetById(medicine.Id);
                if (existing == null)
                    return OperationResult.Failure("Medicine not found.");

                MedicineCategory category = _categoryRepository.GetById(medicine.CategoryId);
                if (category == null)
                    return OperationResult.Failure("Selected category does not exist.");

                Medicine duplicate = _medicineRepository.GetByNameAndBrand(medicine.Name, medicine.Brand);
                if (duplicate != null && duplicate.Id != medicine.Id)
                    return OperationResult.Failure("A medicine with this name and brand already exists.");

                if (medicine.ExpiryDate.HasValue && medicine.ExpiryDate.Value <= DateTime.UtcNow.Date)
                    return OperationResult.Failure("Expiry date must be in the future.");

                _medicineRepository.Update(medicine);
                return OperationResult.Success("Medicine updated successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult DeleteMedicine(int id)
        {
            try
            {
                Medicine medicine = _medicineRepository.GetById(id);
                if (medicine == null)
                    return OperationResult.Failure("Medicine not found.");

                _medicineRepository.Delete(id);
                return OperationResult.Success("Medicine deleted successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }
    }
}
