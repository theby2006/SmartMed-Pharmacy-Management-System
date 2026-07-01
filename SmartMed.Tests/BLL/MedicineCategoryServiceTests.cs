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
    public class MedicineCategoryServiceTests
    {
        private MockMedicineCategoryRepository _categoryRepository;
        private MockMedicineRepository _medicineRepository;
        private IMedicineCategoryService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _categoryRepository = new MockMedicineCategoryRepository();
            _medicineRepository = new MockMedicineRepository();
            _service = new MedicineCategoryService(_categoryRepository, _medicineRepository);
        }

        [TestMethod]
        public void GetAllCategories_ShouldReturnSuccess()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" },
                new MedicineCategory { Id = 2, Name = "Analgesics" }
            };

            OperationResult<List<MedicineCategory>> result = _service.GetAllCategories();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, result.Data.Count);
        }

        [TestMethod]
        public void GetCategoryById_ShouldReturnSuccess_WhenFound()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };

            OperationResult<MedicineCategory> result = _service.GetCategoryById(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Antibiotics", result.Data.Name);
        }

        [TestMethod]
        public void GetCategoryById_ShouldReturnFailure_WhenNotFound()
        {
            OperationResult<MedicineCategory> result = _service.GetCategoryById(999);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Category not found.", result.Message);
        }

        [TestMethod]
        public void AddCategory_ShouldReturnSuccess_WithValidCategory()
        {
            var category = new MedicineCategory { Name = "Antifungals", Description = "Fungal infection treatments" };

            OperationResult<int> result = _service.AddCategory(category);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_categoryRepository.AddCalled);
        }

        [TestMethod]
        public void AddCategory_ShouldReturnFailure_WhenCategoryIsNull()
        {
            OperationResult<int> result = _service.AddCategory(null);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void AddCategory_ShouldReturnFailure_WhenNameIsEmpty()
        {
            var category = new MedicineCategory { Name = "" };

            OperationResult<int> result = _service.AddCategory(category);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void AddCategory_ShouldReturnFailure_WhenNameTooShort()
        {
            var category = new MedicineCategory { Name = "A" };

            OperationResult<int> result = _service.AddCategory(category);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("between 2 and 100"));
        }

        [TestMethod]
        public void AddCategory_ShouldReturnFailure_WhenNameTooLong()
        {
            var category = new MedicineCategory { Name = new string('X', 101) };

            OperationResult<int> result = _service.AddCategory(category);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("between 2 and 100"));
        }

        [TestMethod]
        public void AddCategory_ShouldReturnFailure_WhenDescriptionTooLong()
        {
            var category = new MedicineCategory { Name = "Test", Description = new string('X', 501) };

            OperationResult<int> result = _service.AddCategory(category);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("500 characters"));
        }

        [TestMethod]
        public void AddCategory_ShouldReturnFailure_WhenDuplicateName()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var category = new MedicineCategory { Name = "Antibiotics" };

            OperationResult<int> result = _service.AddCategory(category);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("already exists"));
        }

        [TestMethod]
        public void UpdateCategory_ShouldReturnSuccess_WithValidCategory()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var category = new MedicineCategory { Id = 1, Name = "Updated Antibiotics" };

            OperationResult result = _service.UpdateCategory(category);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_categoryRepository.UpdateCalled);
        }

        [TestMethod]
        public void UpdateCategory_ShouldReturnFailure_WhenCategoryIsNull()
        {
            OperationResult result = _service.UpdateCategory(null);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void UpdateCategory_ShouldReturnFailure_WhenNameIsEmpty()
        {
            var category = new MedicineCategory { Id = 1, Name = "" };

            OperationResult result = _service.UpdateCategory(category);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void UpdateCategory_ShouldReturnFailure_WhenDuplicateName()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" },
                new MedicineCategory { Id = 2, Name = "Analgesics" }
            };
            var category = new MedicineCategory { Id = 1, Name = "Analgesics" };

            OperationResult result = _service.UpdateCategory(category);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("already exists"));
        }

        [TestMethod]
        public void UpdateCategory_ShouldAllowSameName_ForSameCategory()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var category = new MedicineCategory { Id = 1, Name = "Antibiotics" };

            OperationResult result = _service.UpdateCategory(category);

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void DeleteCategory_ShouldReturnSuccess()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };

            OperationResult result = _service.DeleteCategory(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_categoryRepository.DeleteCalled);
        }

        [TestMethod]
        public void DeleteCategory_ShouldReturnFailure_WhenCategoryHasMedicines()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, CategoryId = 1, Name = "Amoxicillin", DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            OperationResult result = _service.DeleteCategory(1);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("associated medicines"));
            Assert.IsFalse(_categoryRepository.DeleteCalled);
        }

        [TestMethod]
        public void DeleteCategory_ShouldReturnFailure_WhenCategoryNotFound()
        {
            OperationResult result = _service.DeleteCategory(999);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Category not found.", result.Message);
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenCategoryRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new MedicineCategoryService(null, _medicineRepository));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenMedicineRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new MedicineCategoryService(_categoryRepository, null));
        }

        [TestMethod]
        public void UpdateCategory_ShouldReturnFailure_WhenNameTooShort()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var category = new MedicineCategory { Id = 1, Name = "A" };

            OperationResult result = _service.UpdateCategory(category);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("between 2 and 100"));
        }

        [TestMethod]
        public void UpdateCategory_ShouldReturnFailure_WhenNameTooLong()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var category = new MedicineCategory { Id = 1, Name = new string('X', 101) };

            OperationResult result = _service.UpdateCategory(category);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("between 2 and 100"));
        }

        [TestMethod]
        public void UpdateCategory_ShouldReturnFailure_WhenDescriptionTooLong()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var category = new MedicineCategory { Id = 1, Name = "Test", Description = new string('X', 501) };

            OperationResult result = _service.UpdateCategory(category);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("500 characters"));
        }

        [TestMethod]
        public void GetAllCategories_ShouldReturnEmptyList_WhenNoCategories()
        {
            OperationResult<List<MedicineCategory>> result = _service.GetAllCategories();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Data.Count);
        }
    }
}
