using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Models;
using SmartMed.BLL.Services;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.Tests.BLL
{
    [TestClass]
    public class MedicineSearchServiceTests
    {
        private MockMedicineRepository _medicineRepository;
        private IMedicineSearchService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _medicineRepository = new MockMedicineRepository
            {
                Medicines = new List<Medicine>
                {
                    new Medicine { Id = 1, Name = "Amoxicillin", Brand = "Amoxil", CategoryId = 1, DosageForm = DosageForm.Tablet, Unit = "mg", UnitPrice = 12.50m },
                    new Medicine { Id = 2, Name = "Paracetamol", Brand = "Panadol", CategoryId = 2, DosageForm = DosageForm.Tablet, Unit = "mg", UnitPrice = 4.50m },
                    new Medicine { Id = 3, Name = "Ibuprofen", Brand = "Nurofen", CategoryId = 2, DosageForm = DosageForm.Tablet, Unit = "mg", UnitPrice = 6.00m },
                    new Medicine { Id = 4, Name = "Cetirizine", Brand = "Zyrtec", CategoryId = 3, DosageForm = DosageForm.Tablet, Unit = "mg", UnitPrice = 7.25m },
                    new Medicine { Id = 5, Name = "Amoxil Junior", Brand = null, CategoryId = 1, DosageForm = DosageForm.Syrup, Unit = "ml", UnitPrice = 12.50m }
                }
            };

            _service = new MedicineSearchService(_medicineRepository);
        }

        [TestMethod]
        public void LinearSearchByName_ShouldMatchNameSubstring_CaseInsensitive()
        {
            List<Medicine> results = _service.LinearSearchByName(_medicineRepository.Medicines, "amox");

            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public void LinearSearchByName_ShouldMatchBrand()
        {
            List<Medicine> results = _service.LinearSearchByName(_medicineRepository.Medicines, "panadol");

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Paracetamol", results[0].Name);
        }

        [TestMethod]
        public void LinearSearchByName_ShouldReturnEmpty_WhenNoMatch()
        {
            List<Medicine> results = _service.LinearSearchByName(_medicineRepository.Medicines, "nonexistent");

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void LinearSearchByName_ShouldReturnEmpty_ForEmptyList()
        {
            List<Medicine> results = _service.LinearSearchByName(new List<Medicine>(), "amox");

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void LinearSearchByName_ShouldReturnEmpty_ForNullOrBlankTerm()
        {
            Assert.AreEqual(0, _service.LinearSearchByName(_medicineRepository.Medicines, "").Count);
            Assert.AreEqual(0, _service.LinearSearchByName(_medicineRepository.Medicines, null).Count);
        }

        [TestMethod]
        public void FilterByCategory_ShouldReturnOnlyMatchingCategory()
        {
            List<Medicine> results = _service.FilterByCategory(_medicineRepository.Medicines, 2);

            Assert.AreEqual(2, results.Count);
            foreach (Medicine m in results)
                Assert.AreEqual(2, m.CategoryId);
        }

        [TestMethod]
        public void FilterByCategory_ShouldReturnEmpty_WhenCategoryHasNoMedicines()
        {
            List<Medicine> results = _service.FilterByCategory(_medicineRepository.Medicines, 999);

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void FilterByPriceRange_ShouldReturnInclusiveBounds()
        {
            List<Medicine> results = _service.FilterByPriceRange(_medicineRepository.Medicines, 4.50m, 7.25m);

            Assert.AreEqual(3, results.Count);
        }

        [TestMethod]
        public void FilterByPriceRange_ShouldReturnEmpty_WhenNoneInRange()
        {
            List<Medicine> results = _service.FilterByPriceRange(_medicineRepository.Medicines, 100m, 200m);

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void SortByPrice_ShouldReturnAscendingOrder()
        {
            List<Medicine> sorted = _service.SortByPrice(_medicineRepository.Medicines);

            for (int i = 1; i < sorted.Count; i++)
                Assert.IsTrue(sorted[i - 1].UnitPrice <= sorted[i].UnitPrice);
        }

        [TestMethod]
        public void SortByPrice_ShouldNotMutateOriginalList()
        {
            List<Medicine> original = new List<Medicine>(_medicineRepository.Medicines);

            _service.SortByPrice(_medicineRepository.Medicines);

            CollectionAssert.AreEqual(original, _medicineRepository.Medicines);
        }

        [TestMethod]
        public void BinarySearchByPrice_ShouldFindExactMatch()
        {
            List<Medicine> sorted = _service.SortByPrice(_medicineRepository.Medicines);

            List<Medicine> results = _service.BinarySearchByPrice(sorted, 6.00m);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Ibuprofen", results[0].Name);
        }

        [TestMethod]
        public void BinarySearchByPrice_ShouldFindAllDuplicatePrices()
        {
            List<Medicine> sorted = _service.SortByPrice(_medicineRepository.Medicines);

            List<Medicine> results = _service.BinarySearchByPrice(sorted, 12.50m);

            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public void BinarySearchByPrice_ShouldReturnEmpty_WhenPriceNotPresent()
        {
            List<Medicine> sorted = _service.SortByPrice(_medicineRepository.Medicines);

            List<Medicine> results = _service.BinarySearchByPrice(sorted, 99.99m);

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void BinarySearchByPrice_ShouldReturnEmpty_WhenTargetBelowMinimum()
        {
            List<Medicine> sorted = _service.SortByPrice(_medicineRepository.Medicines);

            List<Medicine> results = _service.BinarySearchByPrice(sorted, 0.01m);

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void BinarySearchByPrice_ShouldReturnEmpty_WhenTargetAboveMaximum()
        {
            List<Medicine> sorted = _service.SortByPrice(_medicineRepository.Medicines);

            List<Medicine> results = _service.BinarySearchByPrice(sorted, 1000m);

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void BinarySearchByPrice_ShouldReturnEmpty_ForEmptyList()
        {
            List<Medicine> results = _service.BinarySearchByPrice(new List<Medicine>(), 5.00m);

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void Search_ShouldCombineNameCategoryAndPriceFilters()
        {
            MedicineSearchCriteria criteria = new MedicineSearchCriteria
            {
                NameTerm = "a",
                CategoryId = 1
            };

            OperationResult<List<Medicine>> result = _service.Search(criteria);

            Assert.IsTrue(result.IsSuccess);
            foreach (Medicine m in result.Data)
                Assert.AreEqual(1, m.CategoryId);
        }

        [TestMethod]
        public void Search_ShouldUseBinarySearchPath_WhenOnlyExactPriceGiven()
        {
            MedicineSearchCriteria criteria = new MedicineSearchCriteria
            {
                ExactPrice = 12.50m
            };

            OperationResult<List<Medicine>> result = _service.Search(criteria);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, result.Data.Count);
        }

        [TestMethod]
        public void Search_ShouldReturnAll_WhenCriteriaEmpty()
        {
            OperationResult<List<Medicine>> result = _service.Search(new MedicineSearchCriteria());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(_medicineRepository.Medicines.Count, result.Data.Count);
        }
    }
}
