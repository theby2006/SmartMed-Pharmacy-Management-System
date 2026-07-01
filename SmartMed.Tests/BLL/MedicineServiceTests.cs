using System;
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
    public class MedicineServiceTests
    {
        private MockMedicineRepository _medicineRepository;
        private MockMedicineCategoryRepository _categoryRepository;
        private IMedicineService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _medicineRepository = new MockMedicineRepository();
            _categoryRepository = new MockMedicineCategoryRepository();
            _service = new MedicineService(_medicineRepository, _categoryRepository);
        }

        [TestMethod]
        public void GetAllMedicines_ShouldReturnSuccess()
        {
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Amoxicillin", DosageForm = DosageForm.Tablet, Unit = "mg" },
                new Medicine { Id = 2, Name = "Paracetamol", DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            OperationResult<List<Medicine>> result = _service.GetAllMedicines();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, result.Data.Count);
        }

        [TestMethod]
        public void GetMedicineById_ShouldReturnSuccess_WhenFound()
        {
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Amoxicillin", DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            OperationResult<Medicine> result = _service.GetMedicineById(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Amoxicillin", result.Data.Name);
        }

        [TestMethod]
        public void GetMedicineById_ShouldReturnFailure_WhenNotFound()
        {
            OperationResult<Medicine> result = _service.GetMedicineById(999);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Medicine not found.", result.Message);
        }

        [TestMethod]
        public void GetMedicinesByCategory_ShouldReturnSuccess()
        {
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, CategoryId = 1, Name = "Amoxicillin", DosageForm = DosageForm.Tablet, Unit = "mg" },
                new Medicine { Id = 2, CategoryId = 2, Name = "Paracetamol", DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            OperationResult<List<Medicine>> result = _service.GetMedicinesByCategory(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void SearchMedicines_ShouldReturnSuccess()
        {
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Amoxicillin", DosageForm = DosageForm.Tablet, Unit = "mg" },
                new Medicine { Id = 2, Name = "Paracetamol", DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            OperationResult<List<Medicine>> result = _service.SearchMedicines("Amox");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void SearchMedicines_ShouldReturnFailure_WhenKeywordIsEmpty()
        {
            OperationResult<List<Medicine>> result = _service.SearchMedicines("");

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void GetLowStockMedicines_ShouldReturnSuccess()
        {
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Amoxicillin", StockQuantity = 5, ReorderLevel = 10, DosageForm = DosageForm.Tablet, Unit = "mg" },
                new Medicine { Id = 2, Name = "Paracetamol", StockQuantity = 50, ReorderLevel = 10, DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            OperationResult<List<Medicine>> result = _service.GetLowStockMedicines();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void GetNearExpiryMedicines_ShouldReturnSuccess()
        {
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Near Expiry", ExpiryDate = DateTime.UtcNow.AddDays(15), DosageForm = DosageForm.Tablet, Unit = "mg" },
                new Medicine { Id = 2, Name = "Far Expiry", ExpiryDate = DateTime.UtcNow.AddDays(365), DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            OperationResult<List<Medicine>> result = _service.GetNearExpiryMedicines();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void AddMedicine_ShouldReturnSuccess_WithValidMedicine()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var medicine = new Medicine
            {
                CategoryId = 1,
                Name = "Test Drug",
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 9.99m,
                ExpiryDate = DateTime.UtcNow.AddMonths(12)
            };

            OperationResult<int> result = _service.AddMedicine(medicine);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_medicineRepository.AddCalled);
        }

        [TestMethod]
        public void AddMedicine_ShouldReturnFailure_WhenMedicineIsNull()
        {
            OperationResult<int> result = _service.AddMedicine(null);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void AddMedicine_ShouldReturnFailure_WhenNameIsEmpty()
        {
            var medicine = new Medicine { DosageForm = DosageForm.Tablet, Unit = "mg" };

            OperationResult<int> result = _service.AddMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void AddMedicine_ShouldReturnFailure_WhenCategoryNotFound()
        {
            var medicine = new Medicine
            {
                CategoryId = 999,
                Name = "Test Drug",
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 9.99m
            };

            OperationResult<int> result = _service.AddMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("category does not exist"));
        }

        [TestMethod]
        public void AddMedicine_ShouldReturnFailure_WhenExpiryDateInPast()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var medicine = new Medicine
            {
                CategoryId = 1,
                Name = "Test Drug",
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 9.99m,
                ExpiryDate = DateTime.UtcNow.AddDays(-1)
            };

            OperationResult<int> result = _service.AddMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void AddMedicine_ShouldReturnFailure_WhenDuplicateNameAndBrand()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Amoxicillin", Brand = "Amoxil", DosageForm = DosageForm.Tablet, Unit = "mg" }
            };
            var medicine = new Medicine
            {
                CategoryId = 1,
                Name = "Amoxicillin",
                Brand = "Amoxil",
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 9.99m
            };

            OperationResult<int> result = _service.AddMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("already exists"));
        }

        [TestMethod]
        public void AddMedicine_ShouldReturnFailure_WhenUnitIsEmpty()
        {
            var medicine = new Medicine
            {
                CategoryId = 1,
                Name = "Test Drug",
                DosageForm = DosageForm.Tablet,
                Unit = "",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 9.99m
            };

            OperationResult<int> result = _service.AddMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void AddMedicine_ShouldReturnFailure_WhenUnitPriceIsZero()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var medicine = new Medicine
            {
                CategoryId = 1,
                Name = "Test Drug",
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 0m
            };

            OperationResult<int> result = _service.AddMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void UpdateMedicine_ShouldReturnSuccess_WithValidMedicine()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Amoxicillin", DosageForm = DosageForm.Tablet, Unit = "mg", CategoryId = 1 }
            };
            var medicine = new Medicine
            {
                Id = 1,
                CategoryId = 1,
                Name = "Amoxicillin Updated",
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 9.99m
            };

            OperationResult result = _service.UpdateMedicine(medicine);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_medicineRepository.UpdateCalled);
        }

        [TestMethod]
        public void UpdateMedicine_ShouldReturnFailure_WhenMedicineIsNull()
        {
            OperationResult result = _service.UpdateMedicine(null);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void UpdateMedicine_ShouldReturnFailure_WhenMedicineNotFound()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var medicine = new Medicine
            {
                Id = 999,
                CategoryId = 1,
                Name = "Non Existent",
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 9.99m
            };

            OperationResult result = _service.UpdateMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Medicine not found.", result.Message);
        }

        [TestMethod]
        public void DeleteMedicine_ShouldReturnSuccess()
        {
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Amoxicillin", DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            OperationResult result = _service.DeleteMedicine(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_medicineRepository.DeleteCalled);
        }

        [TestMethod]
        public void DeleteMedicine_ShouldReturnFailure_WhenMedicineNotFound()
        {
            OperationResult result = _service.DeleteMedicine(999);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Medicine not found.", result.Message);
            Assert.IsFalse(_medicineRepository.DeleteCalled);
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenMedicineRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new MedicineService(null, _categoryRepository));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenCategoryRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new MedicineService(_medicineRepository, null));
        }

        [TestMethod]
        public void AddMedicine_ShouldReturnFailure_WhenStockQuantityIsNegative()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var medicine = new Medicine
            {
                CategoryId = 1,
                Name = "Test Drug",
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = -1,
                ReorderLevel = 10,
                UnitPrice = 9.99m
            };

            OperationResult<int> result = _service.AddMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void AddMedicine_ShouldReturnFailure_WhenReorderLevelIsNegative()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var medicine = new Medicine
            {
                CategoryId = 1,
                Name = "Test Drug",
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = -1,
                UnitPrice = 9.99m
            };

            OperationResult<int> result = _service.AddMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void AddMedicine_ShouldReturnFailure_WhenNameTooLong()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var medicine = new Medicine
            {
                CategoryId = 1,
                Name = new string('X', 201),
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 9.99m
            };

            OperationResult<int> result = _service.AddMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("200"));
        }

        [TestMethod]
        public void AddMedicine_ShouldReturnFailure_WhenInvalidDosageForm()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            var medicine = new Medicine
            {
                CategoryId = 1,
                Name = "Test Drug",
                DosageForm = (DosageForm)999,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 9.99m
            };

            OperationResult<int> result = _service.AddMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("dosage form"));
        }

        [TestMethod]
        public void AddMedicine_ShouldReturnFailure_WhenCategoryIdIsZero()
        {
            var medicine = new Medicine
            {
                CategoryId = 0,
                Name = "Test Drug",
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 9.99m
            };

            OperationResult<int> result = _service.AddMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("Category is required"));
        }

        [TestMethod]
        public void UpdateMedicine_ShouldReturnFailure_WhenDuplicateNameAndBrand()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Amoxicillin", Brand = "Amoxil", DosageForm = DosageForm.Tablet, Unit = "mg", CategoryId = 1 },
                new Medicine { Id = 2, Name = "Azithromycin", Brand = "Zithromax", DosageForm = DosageForm.Tablet, Unit = "mg", CategoryId = 1 }
            };
            var medicine = new Medicine
            {
                Id = 1,
                CategoryId = 1,
                Name = "Azithromycin",
                Brand = "Zithromax",
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 9.99m
            };

            OperationResult result = _service.UpdateMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("already exists"));
        }

        [TestMethod]
        public void UpdateMedicine_ShouldReturnFailure_WhenCategoryNotFound()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Amoxicillin", DosageForm = DosageForm.Tablet, Unit = "mg", CategoryId = 1 }
            };
            var medicine = new Medicine
            {
                Id = 1,
                CategoryId = 999,
                Name = "Amoxicillin",
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 9.99m
            };

            OperationResult result = _service.UpdateMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("category does not exist"));
        }

        [TestMethod]
        public void UpdateMedicine_ShouldReturnFailure_WhenExpiryDateInPast()
        {
            _categoryRepository.Categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = 1, Name = "Antibiotics" }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Amoxicillin", DosageForm = DosageForm.Tablet, Unit = "mg", CategoryId = 1 }
            };
            var medicine = new Medicine
            {
                Id = 1,
                CategoryId = 1,
                Name = "Amoxicillin",
                DosageForm = DosageForm.Tablet,
                Unit = "mg",
                StockQuantity = 100,
                ReorderLevel = 10,
                UnitPrice = 9.99m,
                ExpiryDate = DateTime.UtcNow.AddDays(-1)
            };

            OperationResult result = _service.UpdateMedicine(medicine);

            Assert.IsFalse(result.IsSuccess);
        }
    }
}
