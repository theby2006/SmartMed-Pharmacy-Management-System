using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.UI.Forms;

namespace SmartMed.Tests.UI
{
    [TestClass]
    public class CustomerLookupFormTests
    {
        [TestMethod]
        public void CustomerLookupForm_ShouldInitializeComponents()
        {
            IAuthenticationService authService = CreateTestAuthService();
            using (CustomerLookupForm form = new CustomerLookupForm(authService))
            {
                Assert.IsNotNull(form);
                Assert.IsFalse(string.IsNullOrWhiteSpace(form.Text));
            }
        }

        private static IAuthenticationService CreateTestAuthService()
        {
            return new SmartMed.BLL.Services.AuthenticationService(
                new StubUserRepository(),
                new StubPasswordHasher(),
                new StubSessionManager(),
                new StubAuditLogRepository());
        }
    }
}
