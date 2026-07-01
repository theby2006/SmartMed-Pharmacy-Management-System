using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.UI.Forms;

namespace SmartMed.Tests.UI
{
    [TestClass]
    public class MedicineFormTests
    {
        [TestMethod]
        public void MedicineForm_ShouldInitializeComponents()
        {
            IMedicineService medicineService = new StubMedicineService();
            IMedicineCategoryService categoryService = new StubMedicineCategoryService();
            using (MedicineForm form = new MedicineForm(medicineService, categoryService))
            {
                Assert.IsNotNull(form);
                Assert.IsFalse(string.IsNullOrWhiteSpace(form.Text));
            }
        }
    }
}
