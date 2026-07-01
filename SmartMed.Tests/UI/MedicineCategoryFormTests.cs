using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.UI.Forms;

namespace SmartMed.Tests.UI
{
    [TestClass]
    public class MedicineCategoryFormTests
    {
        [TestMethod]
        public void MedicineCategoryForm_ShouldInitializeComponents()
        {
            IMedicineCategoryService categoryService = new StubMedicineCategoryService();
            using (MedicineCategoryForm form = new MedicineCategoryForm(categoryService))
            {
                Assert.IsNotNull(form);
                Assert.IsFalse(string.IsNullOrWhiteSpace(form.Text));
            }
        }
    }
}
