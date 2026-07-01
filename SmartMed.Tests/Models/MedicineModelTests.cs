using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;

namespace SmartMed.Tests.Models
{
    [TestClass]
    public class MedicineModelTests
    {
        [TestMethod]
        public void Medicine_ShouldExtendBaseEntity()
        {
            var medicine = new Medicine();

            Assert.AreEqual(0, medicine.Id);
            Assert.IsTrue(medicine.IsActive);
        }

        [TestMethod]
        public void Medicine_ShouldSetAndGetProperties()
        {
            var expiryDate = DateTime.UtcNow.AddMonths(12);
            var medicine = new Medicine
            {
                Id = 1,
                CategoryId = 1,
                Name = "Amoxicillin",
                Brand = "Amoxil",
                DosageForm = DosageForm.Tablet,
                Strength = "500mg",
                Unit = "mg",
                StockQuantity = 500,
                ReorderLevel = 50,
                UnitPrice = 12.50m,
                ExpiryDate = expiryDate,
                Description = "Broad-spectrum antibiotic"
            };

            Assert.AreEqual(1, medicine.Id);
            Assert.AreEqual(1, medicine.CategoryId);
            Assert.AreEqual("Amoxicillin", medicine.Name);
            Assert.AreEqual("Amoxil", medicine.Brand);
            Assert.AreEqual(DosageForm.Tablet, medicine.DosageForm);
            Assert.AreEqual("500mg", medicine.Strength);
            Assert.AreEqual("mg", medicine.Unit);
            Assert.AreEqual(500, medicine.StockQuantity);
            Assert.AreEqual(50, medicine.ReorderLevel);
            Assert.AreEqual(12.50m, medicine.UnitPrice);
            Assert.AreEqual(expiryDate, medicine.ExpiryDate);
            Assert.AreEqual("Broad-spectrum antibiotic", medicine.Description);
        }

        [TestMethod]
        public void Medicine_ShouldAllowNullBrand()
        {
            var medicine = new Medicine
            {
                Name = "Generic Drug",
                DosageForm = DosageForm.Tablet,
                Unit = "mg"
            };

            Assert.IsNull(medicine.Brand);
        }

        [TestMethod]
        public void Medicine_ShouldAllowNullExpiryDate()
        {
            var medicine = new Medicine
            {
                Name = "Test Drug",
                DosageForm = DosageForm.Syrup,
                Unit = "ml"
            };

            Assert.IsNull(medicine.ExpiryDate);
        }

        [TestMethod]
        public void Medicine_ShouldSetDefaultStockValues()
        {
            var medicine = new Medicine();

            Assert.AreEqual(0, medicine.StockQuantity);
            Assert.AreEqual(0, medicine.ReorderLevel);
            Assert.AreEqual(0m, medicine.UnitPrice);
        }
    }
}
