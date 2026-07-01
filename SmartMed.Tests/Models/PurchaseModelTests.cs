using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;

namespace SmartMed.Tests.Models
{
    [TestClass]
    public class PurchaseModelTests
    {
        [TestMethod]
        public void Purchase_ShouldSetAndGetProperties()
        {
            var purchaseDate = DateTime.UtcNow;
            var confirmedDate = DateTime.UtcNow.AddHours(1);
            var purchase = new Purchase
            {
                Id = 1,
                PurchaseNumber = "PO-2024-001",
                PurchaseDate = purchaseDate,
                SupplierId = 5,
                InvoiceNumber = "INV-12345",
                Remarks = "Urgent order",
                CreatedByUserId = 2,
                CreatedDate = purchaseDate,
                Status = PurchaseStatus.Confirmed,
                ConfirmedDate = confirmedDate,
                TotalAmount = 1500.50m,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { Id = 1, MedicineId = 10, BatchNumber = "BATCH-A" }
                }
            };

            Assert.AreEqual(1, purchase.Id);
            Assert.AreEqual("PO-2024-001", purchase.PurchaseNumber);
            Assert.AreEqual(purchaseDate, purchase.PurchaseDate);
            Assert.AreEqual(5, purchase.SupplierId);
            Assert.AreEqual("INV-12345", purchase.InvoiceNumber);
            Assert.AreEqual("Urgent order", purchase.Remarks);
            Assert.AreEqual(2, purchase.CreatedByUserId);
            Assert.AreEqual(purchaseDate, purchase.CreatedDate);
            Assert.AreEqual(PurchaseStatus.Confirmed, purchase.Status);
            Assert.AreEqual(confirmedDate, purchase.ConfirmedDate);
            Assert.AreEqual(1500.50m, purchase.TotalAmount);
            Assert.AreEqual(1, purchase.Items.Count);
        }

        [TestMethod]
        public void Purchase_ShouldDefaultToPendingStatus()
        {
            var purchase = new Purchase();

            Assert.AreEqual(PurchaseStatus.Pending, purchase.Status);
        }

        [TestMethod]
        public void Purchase_ShouldDefaultCreatedDateToUtcNow()
        {
            var purchase = new Purchase();

            Assert.IsTrue(purchase.CreatedDate <= DateTime.UtcNow);
        }

        [TestMethod]
        public void Purchase_ShouldAllowNullOptionalFields()
        {
            var purchase = new Purchase
            {
                PurchaseNumber = "PO-001",
                SupplierId = 1,
                CreatedByUserId = 1
            };

            Assert.IsNull(purchase.InvoiceNumber);
            Assert.IsNull(purchase.Remarks);
            Assert.IsNull(purchase.ConfirmedDate);
        }

        [TestMethod]
        public void Purchase_ShouldDefaultItemsToEmptyList()
        {
            var purchase = new Purchase();

            Assert.IsNotNull(purchase.Items);
            Assert.AreEqual(0, purchase.Items.Count);
        }

        [TestMethod]
        public void PurchaseItem_ShouldSetAndGetProperties()
        {
            var expiryDate = DateTime.UtcNow.AddYears(2);
            var item = new PurchaseItem
            {
                Id = 1,
                PurchaseId = 10,
                MedicineId = 100,
                BatchNumber = "BATCH-2024-A",
                ExpiryDate = expiryDate,
                Quantity = 500,
                PurchasePrice = 12.50m,
                SellingPrice = 25.00m,
                DiscountPercent = 5.00m,
                TaxPercent = 10.00m,
                LineTotal = 5868.75m
            };

            Assert.AreEqual(1, item.Id);
            Assert.AreEqual(10, item.PurchaseId);
            Assert.AreEqual(100, item.MedicineId);
            Assert.AreEqual("BATCH-2024-A", item.BatchNumber);
            Assert.AreEqual(expiryDate, item.ExpiryDate);
            Assert.AreEqual(500, item.Quantity);
            Assert.AreEqual(12.50m, item.PurchasePrice);
            Assert.AreEqual(25.00m, item.SellingPrice);
            Assert.AreEqual(5.00m, item.DiscountPercent);
            Assert.AreEqual(10.00m, item.TaxPercent);
            Assert.AreEqual(5868.75m, item.LineTotal);
        }

        [TestMethod]
        public void PurchaseItem_ShouldDefaultIsActiveToTrue()
        {
            var item = new PurchaseItem();

            Assert.IsTrue(item.IsActive);
        }

        [TestMethod]
        public void StockBatch_ShouldExtendBaseEntity()
        {
            var batch = new StockBatch();

            Assert.AreEqual(0, batch.Id);
            Assert.IsTrue(batch.IsActive);
        }

        [TestMethod]
        public void StockBatch_ShouldSetAndGetProperties()
        {
            var expiryDate = DateTime.UtcNow.AddYears(1);
            var batch = new StockBatch
            {
                Id = 1,
                MedicineId = 100,
                BatchNumber = "BATCH-001",
                ExpiryDate = expiryDate,
                PurchasePrice = 10.00m,
                SellingPrice = 22.00m,
                CurrentQuantity = 450,
                InitialQuantity = 500,
                PurchaseItemId = 5
            };

            Assert.AreEqual(1, batch.Id);
            Assert.AreEqual(100, batch.MedicineId);
            Assert.AreEqual("BATCH-001", batch.BatchNumber);
            Assert.AreEqual(expiryDate, batch.ExpiryDate);
            Assert.AreEqual(10.00m, batch.PurchasePrice);
            Assert.AreEqual(22.00m, batch.SellingPrice);
            Assert.AreEqual(450, batch.CurrentQuantity);
            Assert.AreEqual(500, batch.InitialQuantity);
            Assert.AreEqual(5, batch.PurchaseItemId);
            Assert.IsTrue(batch.IsActive);
        }

        [TestMethod]
        public void StockBatch_ShouldDefaultCurrentQuantityToZero()
        {
            var batch = new StockBatch();

            Assert.AreEqual(0, batch.CurrentQuantity);
        }

        [TestMethod]
        public void StockMovement_ShouldSetAndGetProperties()
        {
            var movement = new StockMovement
            {
                Id = 1,
                StockBatchId = 10,
                MedicineId = 100,
                MovementType = MovementType.StockIn,
                Quantity = 500,
                ReferenceType = "Purchase",
                ReferenceId = 42,
                UnitPrice = 12.50m,
                TotalAmount = 6250.00m,
                CreatedByUserId = 2,
                Notes = "Initial stock from purchase"
            };

            Assert.AreEqual(1, movement.Id);
            Assert.AreEqual(10, movement.StockBatchId);
            Assert.AreEqual(100, movement.MedicineId);
            Assert.AreEqual(MovementType.StockIn, movement.MovementType);
            Assert.AreEqual(500, movement.Quantity);
            Assert.AreEqual("Purchase", movement.ReferenceType);
            Assert.AreEqual(42, movement.ReferenceId);
            Assert.AreEqual(12.50m, movement.UnitPrice);
            Assert.AreEqual(6250.00m, movement.TotalAmount);
            Assert.AreEqual(2, movement.CreatedByUserId);
            Assert.AreEqual("Initial stock from purchase", movement.Notes);
        }

        [TestMethod]
        public void StockMovement_ShouldDefaultCreatedDateToUtcNow()
        {
            var movement = new StockMovement();

            Assert.IsTrue(movement.CreatedDate <= DateTime.UtcNow);
        }

        [TestMethod]
        public void StockMovement_ShouldAllowNullNotes()
        {
            var movement = new StockMovement();

            Assert.IsNull(movement.Notes);
        }

        [TestMethod]
        public void MovementType_ShouldHaveCorrectValues()
        {
            Assert.AreEqual(1, (int)MovementType.StockIn);
            Assert.AreEqual(2, (int)MovementType.StockOut);
        }

        [TestMethod]
        public void PurchaseStatus_ShouldHaveCorrectValues()
        {
            Assert.AreEqual(1, (int)PurchaseStatus.Pending);
            Assert.AreEqual(2, (int)PurchaseStatus.Confirmed);
            Assert.AreEqual(3, (int)PurchaseStatus.Cancelled);
        }

        [TestMethod]
        public void BatchStatus_ShouldHaveCorrectValues()
        {
            Assert.AreEqual(1, (int)BatchStatus.Active);
            Assert.AreEqual(2, (int)BatchStatus.Depleted);
            Assert.AreEqual(3, (int)BatchStatus.Expired);
            Assert.AreEqual(4, (int)BatchStatus.Discontinued);
        }
    }
}
