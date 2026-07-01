using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Models.Entities;

namespace SmartMed.Tests.Models
{
    [TestClass]
    public class MedicineCategoryModelTests
    {
        [TestMethod]
        public void MedicineCategory_ShouldExtendBaseEntity()
        {
            var category = new MedicineCategory();

            Assert.AreEqual(0, category.Id);
            Assert.IsTrue(category.IsActive);
        }

        [TestMethod]
        public void MedicineCategory_ShouldSetAndGetProperties()
        {
            var category = new MedicineCategory
            {
                Id = 1,
                Name = "Antibiotics",
                Description = "Antibacterial medications"
            };

            Assert.AreEqual(1, category.Id);
            Assert.AreEqual("Antibiotics", category.Name);
            Assert.AreEqual("Antibacterial medications", category.Description);
        }

        [TestMethod]
        public void MedicineCategory_ShouldAllowNullDescription()
        {
            var category = new MedicineCategory
            {
                Name = "Test Category"
            };

            Assert.IsNull(category.Description);
        }

        [TestMethod]
        public void MedicineCategory_ShouldSetDefaultIsActive()
        {
            var category = new MedicineCategory();

            Assert.IsTrue(category.IsActive);
        }
    }
}
