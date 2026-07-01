using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Models.Entities;

namespace SmartMed.Tests.Models
{
    [TestClass]
    public class SupplierModelTests
    {
        [TestMethod]
        public void Supplier_ShouldExtendBaseEntity()
        {
            var supplier = new Supplier();

            Assert.AreEqual(0, supplier.Id);
            Assert.IsTrue(supplier.IsActive);
        }

        [TestMethod]
        public void Supplier_ShouldSetAndGetProperties()
        {
            var supplier = new Supplier
            {
                Id = 1,
                SupplierCode = "SUP001",
                SupplierName = "MediHealth Distributors",
                CompanyName = "MediHealth Corp",
                ContactPerson = "John Smith",
                PhoneNumber = "+1-555-0101",
                Email = "john.smith@medihealth.com",
                Address = "123 Health Ave",
                City = "New York",
                Country = "USA",
                PostalCode = "10001",
                TaxNumber = "TAX-001",
                Notes = "Primary pharmaceutical distributor"
            };

            Assert.AreEqual(1, supplier.Id);
            Assert.AreEqual("SUP001", supplier.SupplierCode);
            Assert.AreEqual("MediHealth Distributors", supplier.SupplierName);
            Assert.AreEqual("MediHealth Corp", supplier.CompanyName);
            Assert.AreEqual("John Smith", supplier.ContactPerson);
            Assert.AreEqual("+1-555-0101", supplier.PhoneNumber);
            Assert.AreEqual("john.smith@medihealth.com", supplier.Email);
            Assert.AreEqual("123 Health Ave", supplier.Address);
            Assert.AreEqual("New York", supplier.City);
            Assert.AreEqual("USA", supplier.Country);
            Assert.AreEqual("10001", supplier.PostalCode);
            Assert.AreEqual("TAX-001", supplier.TaxNumber);
            Assert.AreEqual("Primary pharmaceutical distributor", supplier.Notes);
        }

        [TestMethod]
        public void Supplier_ShouldAllowNullOptionalFields()
        {
            var supplier = new Supplier
            {
                SupplierCode = "SUP002",
                SupplierName = "Test Supplier"
            };

            Assert.IsNull(supplier.CompanyName);
            Assert.IsNull(supplier.ContactPerson);
            Assert.IsNull(supplier.PhoneNumber);
            Assert.IsNull(supplier.Email);
            Assert.IsNull(supplier.Address);
            Assert.IsNull(supplier.City);
            Assert.IsNull(supplier.Country);
            Assert.IsNull(supplier.PostalCode);
            Assert.IsNull(supplier.TaxNumber);
            Assert.IsNull(supplier.Notes);
        }

        [TestMethod]
        public void Supplier_ShouldSetDefaultBaseEntityValues()
        {
            var supplier = new Supplier();

            Assert.IsTrue(supplier.IsActive);
            Assert.AreNotEqual(default, supplier.CreatedDate);
            Assert.IsNull(supplier.UpdatedDate);
        }
    }
}
