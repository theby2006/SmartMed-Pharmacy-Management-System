using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;

namespace SmartMed.Tests.Models
{
    [TestClass]
    public class SaleModelTests
    {
        [TestMethod]
        public void Sale_ShouldSetAndGetProperties()
        {
            var saleDate = DateTime.UtcNow;
            var sale = new Sale
            {
                Id = 1,
                SaleNumber = "SALE-2024-001",
                SaleDate = saleDate,
                CashierId = 2,
                CustomerName = "John Doe",
                CustomerPhone = "555-0100",
                DiscountPercent = 5.00m,
                TaxPercent = 10.00m,
                SubTotal = 100.00m,
                DiscountAmount = 5.00m,
                TaxAmount = 9.50m,
                GrandTotal = 104.50m,
                Status = SaleStatus.Completed,
                Notes = "Test sale",
                IsActive = true,
                CreatedDate = saleDate
            };

            Assert.AreEqual(1, sale.Id);
            Assert.AreEqual("SALE-2024-001", sale.SaleNumber);
            Assert.AreEqual(saleDate, sale.SaleDate);
            Assert.AreEqual(2, sale.CashierId);
            Assert.AreEqual("John Doe", sale.CustomerName);
            Assert.AreEqual("555-0100", sale.CustomerPhone);
            Assert.AreEqual(5.00m, sale.DiscountPercent);
            Assert.AreEqual(10.00m, sale.TaxPercent);
            Assert.AreEqual(100.00m, sale.SubTotal);
            Assert.AreEqual(5.00m, sale.DiscountAmount);
            Assert.AreEqual(9.50m, sale.TaxAmount);
            Assert.AreEqual(104.50m, sale.GrandTotal);
            Assert.AreEqual(SaleStatus.Completed, sale.Status);
            Assert.AreEqual("Test sale", sale.Notes);
            Assert.IsTrue(sale.IsActive);
        }

        [TestMethod]
        public void Sale_ShouldDefaultToPendingStatus()
        {
            var sale = new Sale();

            Assert.AreEqual(SaleStatus.Pending, sale.Status);
        }

        [TestMethod]
        public void Sale_ShouldInheritFromBaseEntity()
        {
            var sale = new Sale();

            Assert.AreEqual(0, sale.Id);
            Assert.IsTrue(sale.IsActive);
            Assert.IsTrue(sale.CreatedDate <= DateTime.UtcNow);
        }

        [TestMethod]
        public void Sale_ShouldAllowNullOptionalFields()
        {
            var sale = new Sale
            {
                SaleNumber = "SALE-001",
                CashierId = 1
            };

            Assert.IsNull(sale.CustomerName);
            Assert.IsNull(sale.CustomerPhone);
            Assert.IsNull(sale.Notes);
        }

        [TestMethod]
        public void SaleItem_ShouldSetAndGetProperties()
        {
            var expiryDate = DateTime.UtcNow.AddYears(2);
            var item = new SaleItem
            {
                Id = 1,
                SaleId = 10,
                MedicineId = 100,
                StockBatchId = 50,
                BatchNumber = "BATCH-A",
                ExpiryDate = expiryDate,
                Quantity = 5,
                SellingPrice = 25.00m,
                DiscountPercent = 0,
                TaxPercent = 5.00m,
                LineTotal = 131.25m,
                IsActive = true
            };

            Assert.AreEqual(1, item.Id);
            Assert.AreEqual(10, item.SaleId);
            Assert.AreEqual(100, item.MedicineId);
            Assert.AreEqual(50, item.StockBatchId);
            Assert.AreEqual("BATCH-A", item.BatchNumber);
            Assert.AreEqual(expiryDate, item.ExpiryDate);
            Assert.AreEqual(5, item.Quantity);
            Assert.AreEqual(25.00m, item.SellingPrice);
            Assert.AreEqual(0, item.DiscountPercent);
            Assert.AreEqual(5.00m, item.TaxPercent);
            Assert.AreEqual(131.25m, item.LineTotal);
            Assert.IsTrue(item.IsActive);
        }

        [TestMethod]
        public void SaleItem_ShouldInheritFromBaseEntity()
        {
            var item = new SaleItem();

            Assert.AreEqual(0, item.Id);
            Assert.IsTrue(item.IsActive);
        }

        [TestMethod]
        public void SaleItem_ShouldDefaultIsActiveToTrue()
        {
            var item = new SaleItem();

            Assert.IsTrue(item.IsActive);
        }

        [TestMethod]
        public void SaleItem_ShouldDefaultCreatedDateToUtcNow()
        {
            var item = new SaleItem();

            Assert.IsTrue(item.CreatedDate <= DateTime.UtcNow);
        }

        [TestMethod]
        public void Payment_ShouldSetAndGetProperties()
        {
            var payment = new Payment
            {
                Id = 1,
                SaleId = 10,
                PaymentMethod = PaymentMethod.Cash,
                AmountPaid = 200.00m,
                ChangeAmount = 95.50m,
                PaymentStatus = PaymentStatus.Paid,
                TransactionReference = "TXN-001",
                ProcessedByUserId = 2,
                IsActive = true
            };

            Assert.AreEqual(1, payment.Id);
            Assert.AreEqual(10, payment.SaleId);
            Assert.AreEqual(PaymentMethod.Cash, payment.PaymentMethod);
            Assert.AreEqual(200.00m, payment.AmountPaid);
            Assert.AreEqual(95.50m, payment.ChangeAmount);
            Assert.AreEqual(PaymentStatus.Paid, payment.PaymentStatus);
            Assert.AreEqual("TXN-001", payment.TransactionReference);
            Assert.AreEqual(2, payment.ProcessedByUserId);
            Assert.IsTrue(payment.IsActive);
        }

        [TestMethod]
        public void Payment_ShouldDefaultPaymentMethodToCash()
        {
            var payment = new Payment();

            Assert.AreEqual(PaymentMethod.Cash, payment.PaymentMethod);
        }

        [TestMethod]
        public void Payment_ShouldDefaultPaymentStatusToPaid()
        {
            var payment = new Payment();

            Assert.AreEqual(PaymentStatus.Paid, payment.PaymentStatus);
        }

        [TestMethod]
        public void Payment_ShouldInheritFromBaseEntity()
        {
            var payment = new Payment();

            Assert.AreEqual(0, payment.Id);
            Assert.IsTrue(payment.IsActive);
        }

        [TestMethod]
        public void Payment_ShouldAllowNullTransactionReference()
        {
            var payment = new Payment();

            Assert.IsNull(payment.TransactionReference);
        }

        [TestMethod]
        public void SaleStatus_ShouldHaveCorrectValues()
        {
            Assert.AreEqual(1, (int)SaleStatus.Pending);
            Assert.AreEqual(2, (int)SaleStatus.Completed);
            Assert.AreEqual(3, (int)SaleStatus.Cancelled);
        }

        [TestMethod]
        public void PaymentMethod_ShouldHaveCorrectValues()
        {
            Assert.AreEqual(1, (int)PaymentMethod.Cash);
            Assert.AreEqual(2, (int)PaymentMethod.Card);
            Assert.AreEqual(3, (int)PaymentMethod.QR);
            Assert.AreEqual(4, (int)PaymentMethod.Online);
        }

        [TestMethod]
        public void PaymentStatus_ShouldHaveCorrectValues()
        {
            Assert.AreEqual(1, (int)PaymentStatus.Pending);
            Assert.AreEqual(2, (int)PaymentStatus.Paid);
            Assert.AreEqual(3, (int)PaymentStatus.Refunded);
        }

        [TestMethod]
        public void AuditAction_ShouldIncludeSaleValues()
        {
            Assert.AreEqual(7, (int)AuditAction.SaleCreated);
            Assert.AreEqual(8, (int)AuditAction.SaleCompleted);
            Assert.AreEqual(9, (int)AuditAction.SaleCancelled);
        }
    }
}
