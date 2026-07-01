using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
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
    public class PurchaseWorkflowTests
    {
        private MockPurchaseRepository _purchaseRepository;
        private MockPurchaseItemRepository _purchaseItemRepository;
        private MockStockBatchRepository _stockBatchRepository;
        private MockStockMovementRepository _stockMovementRepository;
        private MockMedicineRepository _medicineRepository;
        private MockSupplierRepository _supplierRepository;
        private IDbConnectionFactory _connectionFactory;
        private IPurchaseService _purchaseService;
        private IInventoryService _inventoryService;

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

            _purchaseService = new PurchaseService(
                _purchaseRepository, _purchaseItemRepository, _stockBatchRepository,
                _stockMovementRepository, _medicineRepository, _supplierRepository,
                _connectionFactory);

            _inventoryService = new InventoryService(
                _stockBatchRepository, _stockMovementRepository, _medicineRepository, _connectionFactory);
        }

        [TestMethod]
        public void FullPurchaseLifecycle_ShouldSucceed()
        {
            // Arrange: seed supplier and medicine
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Id = 1,
                    SupplierCode = "SUP001",
                    SupplierName = "Pharma Wholesale",
                    IsActive = true
                }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine
                {
                    Id = 1,
                    Name = "Amoxicillin 500mg",
                    IsActive = true,
                    DosageForm = DosageForm.Tablet,
                    Unit = "mg",
                    StockQuantity = 0
                },
                new Medicine
                {
                    Id = 2,
                    Name = "Paracetamol 250mg",
                    IsActive = true,
                    DosageForm = DosageForm.Tablet,
                    Unit = "mg",
                    StockQuantity = 0
                }
            };

            // Step 1: Create a purchase draft
            var newPurchase = new Purchase
            {
                PurchaseNumber = "PO-2024-001",
                PurchaseDate = DateTime.UtcNow,
                SupplierId = 1,
                InvoiceNumber = "INV-2024-001",
                Remarks = "Monthly stock order",
                CreatedByUserId = 2,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem
                    {
                        MedicineId = 1,
                        BatchNumber = "AMOX-BATCH-001",
                        ExpiryDate = DateTime.UtcNow.AddYears(2),
                        Quantity = 500,
                        PurchasePrice = 5.00m,
                        SellingPrice = 12.50m,
                        DiscountPercent = 0,
                        TaxPercent = 0
                    },
                    new PurchaseItem
                    {
                        MedicineId = 2,
                        BatchNumber = "PARA-BATCH-001",
                        ExpiryDate = DateTime.UtcNow.AddYears(2),
                        Quantity = 1000,
                        PurchasePrice = 2.00m,
                        SellingPrice = 5.00m,
                        DiscountPercent = 5,
                        TaxPercent = 10
                    }
                }
            };

            OperationResult<int> createResult = _purchaseService.CreatePurchase(newPurchase);
            Assert.IsTrue(createResult.IsSuccess, $"Create should succeed: {createResult.Message}");
            Assert.IsTrue(createResult.Data > 0, "Should return valid purchase ID");
            Assert.IsTrue(_purchaseRepository.AddCalled, "Purchase should be added to repository");
            Assert.IsTrue(_purchaseItemRepository.AddRangeCalled, "Items should be added");
            int purchaseId = createResult.Data;

            // Step 2: Retrieve the purchase by ID
            OperationResult<Purchase> getResult = _purchaseService.GetPurchaseById(purchaseId);
            Assert.IsTrue(getResult.IsSuccess);
            Assert.AreEqual("PO-2024-001", getResult.Data.PurchaseNumber);
            Assert.AreEqual(2, getResult.Data.Items.Count);
            Assert.AreEqual(PurchaseStatus.Pending, getResult.Data.Status);

            // Step 3: Verify initial stock is zero
            OperationResult<int> stockBefore = _inventoryService.GetMedicineStock(1);
            Assert.IsTrue(stockBefore.IsSuccess);
            Assert.AreEqual(0, stockBefore.Data);

            // Step 4: Verify purchase is in the list
            OperationResult<List<Purchase>> allResult = _purchaseService.GetAllPurchases();
            Assert.IsTrue(allResult.IsSuccess);
            Assert.AreEqual(1, allResult.Data.Count);

            // Step 5: Verify duplicate purchase number is rejected
            var duplicatePurchase = new Purchase
            {
                PurchaseNumber = "PO-2024-001",
                SupplierId = 1,
                CreatedByUserId = 2,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { MedicineId = 1, BatchNumber = "BATCH-002", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
                }
            };
            OperationResult<int> dupResult = _purchaseService.CreatePurchase(duplicatePurchase);
            Assert.IsFalse(dupResult.IsSuccess, "Duplicate purchase number should be rejected");

            // Step 6: Verify duplicate batch is rejected
            var purchaseWithDupBatch = new Purchase
            {
                PurchaseNumber = "PO-2024-002",
                SupplierId = 1,
                CreatedByUserId = 2,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { MedicineId = 1, BatchNumber = "AMOX-BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
                }
            };
            OperationResult<int> dupBatchResult = _purchaseService.CreatePurchase(purchaseWithDupBatch);
            Assert.IsFalse(dupBatchResult.IsSuccess, "Duplicate batch should be rejected");

            // Step 7: Cancel the purchase
            OperationResult cancelResult = _purchaseService.CancelPurchase(purchaseId);
            Assert.IsTrue(cancelResult.IsSuccess, "Cancel should succeed");

            // Verify status changed
            Purchase cancelledPurchase = _purchaseRepository.GetById(purchaseId);
            Assert.AreEqual(PurchaseStatus.Cancelled, cancelledPurchase.Status);

            // Step 8: Verify cancelled purchase cannot be confirmed
            OperationResult confirmAfterCancel = _purchaseService.ConfirmPurchase(purchaseId);
            Assert.IsFalse(confirmAfterCancel.IsSuccess, "Cancelled purchase should not be confirmable");
        }

        [TestMethod]
        public void CreateAndCancel_ShouldAllowNewPurchase_AfterCancel()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Test Supplier", IsActive = true }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Test Med", IsActive = true, DosageForm = DosageForm.Tablet, Unit = "mg" }
            };

            // Create
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
            OperationResult<int> createResult = _purchaseService.CreatePurchase(purchase);
            Assert.IsTrue(createResult.IsSuccess);

            // Cancel
            OperationResult cancelResult = _purchaseService.CancelPurchase(createResult.Data);
            Assert.IsTrue(cancelResult.IsSuccess);

            // Verify the same purchase number cannot be reused for a new purchase
            var secondPurchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                SupplierId = 1,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { MedicineId = 1, BatchNumber = "BATCH-002", ExpiryDate = DateTime.UtcNow.AddYears(2), Quantity = 100, PurchasePrice = 10, SellingPrice = 25 }
                }
            };
            OperationResult<int> dupResult = _purchaseService.CreatePurchase(secondPurchase);
            Assert.IsFalse(dupResult.IsSuccess, "Purchase number must remain unique even after cancellation");
        }

        [TestMethod]
        public void GetAllSuppliersForDropdown_ShouldShowAllActiveSuppliers()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Active Supplier A", IsActive = true },
                new Supplier { Id = 2, SupplierCode = "SUP002", SupplierName = "Active Supplier B", IsActive = true },
                new Supplier { Id = 3, SupplierCode = "SUP003", SupplierName = "Inactive Supplier", IsActive = false }
            };

            List<Supplier> suppliers = _supplierRepository.GetAll();

            Assert.AreEqual(3, suppliers.Count);

            List<Supplier> activeSuppliers = suppliers.FindAll(s => s.IsActive);
            Assert.AreEqual(2, activeSuppliers.Count);
        }

        [TestMethod]
        public void PurchaseService_MultipleItems_ShouldCalculateCorrectLineTotals()
        {
            _supplierRepository.Suppliers = new List<Supplier>
            {
                new Supplier { Id = 1, SupplierCode = "SUP001", SupplierName = "Test Supplier", IsActive = true }
            };
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Med A", IsActive = true, DosageForm = DosageForm.Tablet, Unit = "mg" },
                new Medicine { Id = 2, Name = "Med B", IsActive = true, DosageForm = DosageForm.Capsule, Unit = "mg" }
            };

            var purchase = new Purchase
            {
                PurchaseNumber = "PO-MULTI-001",
                SupplierId = 1,
                CreatedByUserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem
                    {
                        MedicineId = 1,
                        BatchNumber = "BATCH-A",
                        ExpiryDate = DateTime.UtcNow.AddYears(2),
                        Quantity = 100,
                        PurchasePrice = 10.00m,
                        SellingPrice = 20.00m,
                        DiscountPercent = 10,
                        TaxPercent = 5
                    },
                    new PurchaseItem
                    {
                        MedicineId = 2,
                        BatchNumber = "BATCH-B",
                        ExpiryDate = DateTime.UtcNow.AddYears(1),
                        Quantity = 200,
                        PurchasePrice = 5.00m,
                        SellingPrice = 12.00m,
                        DiscountPercent = 0,
                        TaxPercent = 0
                    }
                }
            };

            OperationResult<int> result = _purchaseService.CreatePurchase(purchase);

            Assert.IsTrue(result.IsSuccess);

            Purchase saved = _purchaseRepository.GetById(result.Data);
            Assert.IsNotNull(saved);

            List<PurchaseItem> items = _purchaseItemRepository.Items;
            Assert.AreEqual(2, items.Count);

            // Item 1: Qty=100, Price=10, Discount=10%, Tax=5%
            // Subtotal = 100 * 10 = 1000
            // Discount = 1000 * 0.10 = 100
            // After discount = 900
            // Tax = 900 * 0.05 = 45
            // LineTotal = 900 + 45 = 945
            PurchaseItem itemA = items.First(i => i.BatchNumber == "BATCH-A");
            Assert.AreEqual(945.00m, itemA.LineTotal);

            // Item 2: Qty=200, Price=5, Discount=0%, Tax=0%
            // Subtotal = 200 * 5 = 1000
            // LineTotal = 1000
            PurchaseItem itemB = items.First(i => i.BatchNumber == "BATCH-B");
            Assert.AreEqual(1000.00m, itemB.LineTotal);
        }
    }
}
