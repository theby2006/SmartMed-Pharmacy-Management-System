using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.UI.Forms;

namespace SmartMed.Tests.UI
{
    [TestClass]
    public class ReportsFormTests
    {
        [TestMethod]
        public void ReportsForm_ShouldInitializeComponents()
        {
            IReportService reportService = new StubReportService();
            using (ReportsForm form = new ReportsForm(reportService))
            {
                Assert.IsNotNull(form);
                Assert.IsFalse(string.IsNullOrWhiteSpace(form.Text));
            }
        }
    }
}
