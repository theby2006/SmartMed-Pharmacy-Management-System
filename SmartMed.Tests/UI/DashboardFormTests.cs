using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.UI.Forms;

namespace SmartMed.Tests.UI
{
    [TestClass]
    public class DashboardFormTests
    {
        [TestMethod]
        public void DashboardForm_ShouldInitializeComponents()
        {
            IReportService reportService = new StubReportService();
            using (DashboardForm form = new DashboardForm(reportService))
            {
                Assert.IsNotNull(form);
                Assert.IsFalse(string.IsNullOrWhiteSpace(form.Text));
            }
        }
    }
}
