using System;
using System.Collections.Generic;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class MedicineCategoryService : IMedicineCategoryService
    {
        private readonly IMedicineCategoryRepository _categoryRepository;
        private readonly IMedicineRepository _medicineRepository;

        public MedicineCategoryService(
            IMedicineCategoryRepository categoryRepository,
            IMedicineRepository medicineRepository)
        {
            Guard.AgainstNull(categoryRepository, nameof(categoryRepository));
            Guard.AgainstNull(medicineRepository, nameof(medicineRepository));
            _categoryRepository = categoryRepository;
            _medicineRepository = medicineRepository;
        }

        public OperationResult<List<MedicineCategory>> GetAllCategories()
        {
            try
            {
                List<MedicineCategory> categories = _categoryRepository.GetAll();
                return OperationResult<List<MedicineCategory>>.Success(categories);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<MedicineCategory>>.Failure(ex.Message);
            }
        }

        public OperationResult<MedicineCategory> GetCategoryById(int id)
        {
            try
            {
                MedicineCategory category = _categoryRepository.GetById(id);
                if (category == null)
                    return OperationResult<MedicineCategory>.Failure("Category not found.");
                return OperationResult<MedicineCategory>.Success(category);
            }
            catch (ValidationException ex)
            {
                return OperationResult<MedicineCategory>.Failure(ex.Message);
            }
        }

        public OperationResult<int> AddCategory(MedicineCategory category)
        {
            try
            {
                Guard.AgainstNull(category, nameof(category));
                Guard.AgainstNullOrWhiteSpace(category.Name, nameof(category.Name));

                if (category.Name.Length < 2 || category.Name.Length > 100)
                    return OperationResult<int>.Failure("Category name must be between 2 and 100 characters.");

                if (category.Description != null && category.Description.Length > 500)
                    return OperationResult<int>.Failure("Category description must not exceed 500 characters.");

                MedicineCategory existing = _categoryRepository.GetByName(category.Name);
                if (existing != null)
                    return OperationResult<int>.Failure("A category with this name already exists.");

                int id = _categoryRepository.Add(category);
                return OperationResult<int>.Success(id, "Category added successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult<int>.Failure(ex.Message);
            }
        }

        public OperationResult UpdateCategory(MedicineCategory category)
        {
            try
            {
                Guard.AgainstNull(category, nameof(category));
                Guard.AgainstNullOrWhiteSpace(category.Name, nameof(category.Name));

                if (category.Name.Length < 2 || category.Name.Length > 100)
                    return OperationResult.Failure("Category name must be between 2 and 100 characters.");

                if (category.Description != null && category.Description.Length > 500)
                    return OperationResult.Failure("Category description must not exceed 500 characters.");

                MedicineCategory existing = _categoryRepository.GetByName(category.Name);
                if (existing != null && existing.Id != category.Id)
                    return OperationResult.Failure("A category with this name already exists.");

                _categoryRepository.Update(category);
                return OperationResult.Success("Category updated successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult DeleteCategory(int id)
        {
            try
            {
                MedicineCategory category = _categoryRepository.GetById(id);
                if (category == null)
                    return OperationResult.Failure("Category not found.");

                List<Medicine> medicines = _medicineRepository.GetByCategoryId(id);
                if (medicines.Count > 0)
                    return OperationResult.Failure(
                        "Cannot delete category because it has associated medicines. Remove or reassign the medicines first.");

                _categoryRepository.Delete(id);
                return OperationResult.Success("Category deleted successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }
    }
}
