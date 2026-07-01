using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.UI.Forms;

namespace SmartMed.Tests.UI
{
    [TestClass]
    public class SupplierFormTests
    {
        [TestMethod]
        public void SupplierForm_ShouldInitializeComponents()
        {
            ISupplierService supplierService = new StubSupplierService();
            using (SupplierForm form = new SupplierForm(supplierService))
            {
                Assert.IsNotNull(form);
                Assert.IsFalse(string.IsNullOrWhiteSpace(form.Text));
            }
        }
    }
}
