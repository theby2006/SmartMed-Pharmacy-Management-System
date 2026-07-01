using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Common.Exceptions;
using SmartMed.DAL.Infrastructure;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;

namespace SmartMed.Tests.BLL
{
    [TestClass]
    public class PurchaseServiceTests
    {
        private MockPurchaseRepository _purchaseRepository;
        private MockPurchaseItemRepository _purchaseItemRepository;
        private MockStockBatchRepository _stockBatchRepository;
        private MockStockMovementRepository _stockMovementRepository;
        private MockMedicineRepository _medicineRepository;
        private MockSupplierRepository _supplierRepository;
        private IDbConnectionFactory _connectionFactory;
        private IPurchaseService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _purchaseRepository = new MockPurchaseRepository();
            _purchaseItemRepository = new MockPurchaseItemRepository();
            _stockBatchRepository = new MockStockBatchRepository();
            _stockMovementRepository = new MockStockMovementRepository();
            _medicineRepository = new MockMedicineRepository();
            _supplierRepository = new MockSupplierRepository();
            _connectionFactory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            _service = new PurchaseService(
                _purchaseRepository, _purchaseItemRepository, _stockBatchRepository,
                _stockMovementRepository, _medicineRepository, _supplierRepository,
                _connectionFactory);
        }

        [TestMethod]
        public void GetAllPurchases_ShouldReturnSuccess()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", SupplierId = 1, CreatedByUserId = 1 },
                new Purchase { Id = 2, PurchaseNumber = "PO-002", SupplierId = 1, CreatedByUserId = 1 }
            };

            OperationResult<List<Purchase>> result = _service.GetAllPurchases();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, result.Data.Count);
        }

        [TestMethod]
        public void GetAllPurchases_ShouldReturnEmptyList_WhenNoPurchases()
        {
            OperationResult<List<Purchase>> result = _service.GetAllPurchases();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Data.Count);
        }

        [TestMethod]
        public void GetPurchaseById_ShouldReturnSuccess_WhenFound()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", SupplierId = 1, CreatedByUserId = 1 }
            };

            OperationResult<Purchase> result = _service.GetPurchaseById(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("PO-001", result.Data.PurchaseNumber);
        }

        [TestMethod]
        public void GetPurchaseById_ShouldReturnFailure_WhenNotFound()
        {
            OperationResult<Purchase> result = _service.GetPurchaseById(999);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Purchase not found.", result.Message);
        }

        [TestMethod]
        public void SearchPurchases_ShouldReturnSuccess()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", InvoiceNumber = "INV-001", SupplierId = 1, CreatedByUserId = 1 },
                new Purchase { Id = 2, PurchaseNumber = "PO-002", InvoiceNumber = "INV-002", SupplierId = 1, CreatedByUserId = 1 }
            };

            OperationResult<List<Purchase>> result = _service.SearchPurchases("PO-001");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void SearchPurchases_ShouldReturnFailure_WhenKeywordIsEmpty()
        {
            OperationResult<List<Purchase>> result = _service.SearchPurchases("");

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void GetPurchasesBySupplier_ShouldReturnSuccess()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", SupplierId = 1, CreatedByUserId = 1 },
                new Purchase { Id = 2, PurchaseNumber = "PO-002", SupplierId = 2, CreatedByUserId = 1 }
            };

            OperationResult<List<Purchase>> result = _service.GetPurchasesBySupplier(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void GetPurchasesByDateRange_ShouldReturnSuccess()
        {
            DateTime baseDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", PurchaseDate = baseDate, SupplierId = 1, CreatedByUserId = 1 },
                new Purchase { Id = 2, PurchaseNumber = "PO-002", PurchaseDate = baseDate.AddDays(10), SupplierId = 1, CreatedByUserId = 1 }
            };

            OperationResult<List<Purchase>> result = _service.GetPurchasesByDateRange(baseDate, baseDate.AddDays(5));

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnSuccess_WithValidPurchase()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Test Supplier", IsActive = true }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Test Medicine", IsActive = true, DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            var purchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                PurchaseDate = DateTime.UtcNow,
                SupplierId = 1,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem
                    {
                        MedicineId = 1,
                        BatchNumber = "BATCH-001",
                        ExpiryDate = DateTime.UtcNow.AddYears(2),
                        Quantity = 100,
                        PurchasePrice = 10.00m,
                        SellingPrice = 25.00m,
                        DiscountPercent = 0,
                        TaxPercent = 0
                    }
                }
            };

            OperationResult<int> result = _service.CreatePurchase(purchase);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_purchaseRepository.AddCalled);
            Assert.IsTrue(_purchaseItemRepository.AddRangeCalled);
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnFailure_WhenPurchaseIsNull()
        {
            OperationResult<int> result = _service.CreatePurchase(null);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnFailure_WhenPurchaseNumberIsEmpty()
        {
            var purchase = new Purchase
            {
                PurchaseNumber = "",
                SupplierId = 1,
                CreatedByUserId = 1
            };

            OperationResult<int> result = _service.CreatePurchase(purchase);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnFailure_WhenDuplicatePurchaseNumber()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", SupplierId = 1, CreatedByUserId = 1 }
            };

            var purchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                SupplierId = 1,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
                }
            };

            OperationResult<int> result = _service.CreatePurchase(purchase);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("already exists"));
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnFailure_WhenSupplierNotFound()
        {
            var purchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                SupplierId = 999,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
                }
            };

            OperationResult<int> result = _service.CreatePurchase(purchase);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("Supplier not found"));
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnFailure_WhenSupplierIsInactive()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Inactive Supplier", IsActive = false }
            };

            var purchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                SupplierId = 1,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
                }
            };

            OperationResult<int> result = _service.CreatePurchase(purchase);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("not active"));
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnFailure_WhenNoItems()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Test Supplier", IsActive = true }
            };

            var purchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                SupplierId = 1,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>()
            };

            OperationResult<int> result = _service.CreatePurchase(purchase);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("at least one item"));
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnFailure_WhenMedicineNotFound()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Test Supplier", IsActive = true }
            };

            var purchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                SupplierId = 1,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { MedicineId = 999, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
                }
            };

            OperationResult<int> result = _service.CreatePurchase(purchase);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("Medicine not found"));
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnFailure_WhenMedicineIsInactive()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Test Supplier", IsActive = true }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Inactive Med", IsActive = false, DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            var purchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                SupplierId = 1,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
                }
            };

            OperationResult<int> result = _service.CreatePurchase(purchase);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("not active"));
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnFailure_WhenExpiryDateIsInPast()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Test Supplier", IsActive = true }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Test Medicine", IsActive = true, DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            var purchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                SupplierId = 1,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddDays(-1), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
                }
            };

            OperationResult<int> result = _service.CreatePurchase(purchase);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("Expiry date"));
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnFailure_WhenQuantityIsZero()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Test Supplier", IsActive = true }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Test Medicine", IsActive = true, DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            var purchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                SupplierId = 1,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 0, PurchasePrice = 10, SellingPrice = 25 }
                }
            };

            OperationResult<int> result = _service.CreatePurchase(purchase);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("greater than zero"));
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnFailure_WhenPurchasePriceIsZero()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Test Supplier", IsActive = true }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Test Medicine", IsActive = true, DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            var purchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                SupplierId = 1,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 0, SellingPrice = 25 }
                }
            };

            OperationResult<int> result = _service.CreatePurchase(purchase);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("greater than zero"));
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnFailure_WhenSellingPriceLessThanPurchasePrice()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Test Supplier", IsActive = true }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Test Medicine", IsActive = true, DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            var purchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                SupplierId = 1,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 25, SellingPrice = 10 }
                }
            };

            OperationResult<int> result = _service.CreatePurchase(purchase);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("cannot be less"));
        }

        [TestMethod]
        public void CreatePurchase_ShouldReturnFailure_WhenDuplicateBatch()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Test Supplier", IsActive = true }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Test Medicine", IsActive = true, DosageForm = DosageForm.Tablet, Unit = "mg" }
            };
            _stockBatchRepository.Batches = new List<StockBatch>
            {
                new StockBatch { Id = 1, MedicineId = 1, BatchNumber = "BATCH-001", IsActive = true, CurrentQuantity = 50 }
            };

            var purchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                SupplierId = 1,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
                }
            };

            OperationResult<int> result = _service.CreatePurchase(purchase);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("already exists"));
        }

        [TestMethod]
        public void ConfirmPurchase_ShouldReturnFailure_WhenPurchaseNotFound()
        {
            OperationResult result = _service.ConfirmPurchase(999);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Purchase not found.", result.Message);
        }

        [TestMethod]
        public void ConfirmPurchase_ShouldReturnFailure_WhenAlreadyConfirmed()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", SupplierId = 1, CreatedByUserId = 1, Status = PurchaseStatus.Confirmed }
            };

            OperationResult result = _service.ConfirmPurchase(1);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("pending"));
        }

        [TestMethod]
        public void ConfirmPurchase_ShouldReturnFailure_WhenNoItems()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", SupplierId = 1, CreatedByUserId = 1, Status = PurchaseStatus.Pending }
            };

            OperationResult result = _service.ConfirmPurchase(1);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("no items"));
        }

        [TestMethod]
        public void ConfirmPurchase_ShouldReturnFailure_WhenSupplierIsInactive()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", SupplierId = 1, CreatedByUserId = 1, Status = PurchaseStatus.Pending }
            };
            _purchaseItemRepository.Items = new List<PurchaseItem>
            {
                new PurchaseItem { Id = 1, PurchaseId = 1, MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
            };
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Inactive Supplier", IsActive = false }
            };

            OperationResult result = _service.ConfirmPurchase(1);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("not active"));
        }

        [TestMethod]
        public void ConfirmPurchase_ShouldReturnFailure_WhenMedicineIsInactive()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", SupplierId = 1, CreatedByUserId = 1, Status = PurchaseStatus.Pending }
            };
            _purchaseItemRepository.Items = new List<PurchaseItem>
            {
                new PurchaseItem { Id = 1, PurchaseId = 1, MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
            };
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Active Supplier", IsActive = true }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Inactive Med", IsActive = false, DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            OperationResult result = _service.ConfirmPurchase(1);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("not active"));
        }

        [TestMethod]
        public void ConfirmPurchase_ShouldReturnFailure_WhenExpiryDateIsInPast()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", SupplierId = 1, CreatedByUserId = 1, Status = PurchaseStatus.Pending }
            };
            _purchaseItemRepository.Items = new List<PurchaseItem>
            {
                new PurchaseItem { Id = 1, PurchaseId = 1, MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddDays(-1), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
            };
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Active Supplier", IsActive = true }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Test Med", IsActive = true, DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            OperationResult result = _service.ConfirmPurchase(1);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("Expiry date"));
        }

        [TestMethod]
        public void ConfirmPurchase_ShouldReturnFailure_WhenDuplicateBatch()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", SupplierId = 1, CreatedByUserId = 1, Status = PurchaseStatus.Pending }
            };
            _purchaseItemRepository.Items = new List<PurchaseItem>
            {
                new PurchaseItem { Id = 1, PurchaseId = 1, MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
            };
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Active Supplier", IsActive = true }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Test Med", IsActive = true, DosageForm = DosageForm.Tablet, Unit = "mg" }
            };
            _stockBatchRepository.Batches = new List<StockBatch>
            {
                new StockBatch { Id = 1, MedicineId = 1, BatchNumber = "BATCH-001", IsActive = true, CurrentQuantity = 50 }
            };

            OperationResult result = _service.ConfirmPurchase(1);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("already exists"));
        }

        [TestMethod]
        public void CancelPurchase_ShouldReturnSuccess()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", SupplierId = 1, CreatedByUserId = 1, Status = PurchaseStatus.Pending }
            };

            OperationResult result = _service.CancelPurchase(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_purchaseRepository.CancelCalled);
        }

        [TestMethod]
        public void CancelPurchase_ShouldReturnFailure_WhenPurchaseNotFound()
        {
            OperationResult result = _service.CancelPurchase(999);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Purchase not found.", result.Message);
        }

        [TestMethod]
        public void CancelPurchase_ShouldReturnFailure_WhenAlreadyConfirmed()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", SupplierId = 1, CreatedByUserId = 1, Status = PurchaseStatus.Confirmed }
            };

            OperationResult result = _service.CancelPurchase(1);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("cannot be cancelled"));
        }

        [TestMethod]
        public void CancelPurchase_ShouldReturnFailure_WhenAlreadyCancelled()
        {
            _purchaseRepository.Purchases = new List<Purchase>
            {
                new Purchase { Id = 1, PurchaseNumber = "PO-001", SupplierId = 1, CreatedByUserId = 1, Status = PurchaseStatus.Cancelled }
            };

            OperationResult result = _service.CancelPurchase(1);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("already cancelled"));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenPurchaseRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new PurchaseService(null, _purchaseItemRepository, _stockBatchRepository,
                    _stockMovementRepository, _medicineRepository, _supplierRepository,
                    _connectionFactory));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenPurchaseItemRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new PurchaseService(_purchaseRepository, null, _stockBatchRepository,
                    _stockMovementRepository, _medicineRepository, _supplierRepository,
                    _connectionFactory));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenStockBatchRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new PurchaseService(_purchaseRepository, _purchaseItemRepository, null,
                    _stockMovementRepository, _medicineRepository, _supplierRepository,
                    _connectionFactory));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenStockMovementRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new PurchaseService(_purchaseRepository, _purchaseItemRepository, _stockBatchRepository,
                    null, _medicineRepository, _supplierRepository,
                    _connectionFactory));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenMedicineRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new PurchaseService(_purchaseRepository, _purchaseItemRepository, _stockBatchRepository,
                    _stockMovementRepository, null, _supplierRepository,
                    _connectionFactory));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenSupplierRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new PurchaseService(_purchaseRepository, _purchaseItemRepository, _stockBatchRepository,
                    _stockMovementRepository, _medicineRepository, null,
                    _connectionFactory));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new PurchaseService(_purchaseRepository, _purchaseItemRepository, _stockBatchRepository,
                    _stockMovementRepository, _medicineRepository, _supplierRepository,
                    null));
        }
    }
}
