using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Models.Enums;

namespace SmartMed.Tests.Models
{
    [TestClass]
    public class DosageFormModelTests
    {
        [TestMethod]
        public void DosageForm_ShouldHaveExpectedValues()
        {
            Assert.AreEqual(1, (int)DosageForm.Tablet);
            Assert.AreEqual(2, (int)DosageForm.Capsule);
            Assert.AreEqual(3, (int)DosageForm.Syrup);
            Assert.AreEqual(4, (int)DosageForm.Injection);
            Assert.AreEqual(5, (int)DosageForm.Cream);
            Assert.AreEqual(6, (int)DosageForm.Ointment);
            Assert.AreEqual(7, (int)DosageForm.Drops);
            Assert.AreEqual(8, (int)DosageForm.Inhaler);
            Assert.AreEqual(9, (int)DosageForm.Spray);
            Assert.AreEqual(10, (int)DosageForm.Solution);
            Assert.AreEqual(11, (int)DosageForm.Suspension);
            Assert.AreEqual(12, (int)DosageForm.Powder);
            Assert.AreEqual(13, (int)DosageForm.Granules);
            Assert.AreEqual(14, (int)DosageForm.Patch);
            Assert.AreEqual(15, (int)DosageForm.Gel);
            Assert.AreEqual(16, (int)DosageForm.Lotion);
            Assert.AreEqual(17, (int)DosageForm.Suppository);
            Assert.AreEqual(18, (int)DosageForm.Implant);
            Assert.AreEqual(19, (int)DosageForm.Device);
            Assert.AreEqual(20, (int)DosageForm.Other);
        }

        [TestMethod]
        public void DosageForm_ShouldBeValidEnum()
        {
            foreach (DosageForm form in Enum.GetValues(typeof(DosageForm)))
            {
                Assert.IsTrue(Enum.IsDefined(typeof(DosageForm), form));
            }
        }
    }
}
