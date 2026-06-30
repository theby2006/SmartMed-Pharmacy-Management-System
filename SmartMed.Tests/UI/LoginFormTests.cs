using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.UI.Forms;

namespace SmartMed.Tests.UI
{
    [TestClass]
    public class LoginFormTests
    {
        [TestMethod]
        public void LoginForm_ShouldInitializeComponents()
        {
            IAuthenticationService authService = CreateTestAuthService();
            using (LoginForm form = new LoginForm(authService))
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
