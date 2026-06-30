using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;
using SmartMed.DAL.Infrastructure;
using SmartMed.DAL.Interfaces;
using SmartMed.DAL.Repositories;
using SmartMed.Models.Diagnostics;
using SmartMed.UI.Forms;

namespace SmartMed.UI.Bootstrap
{
    public class ApplicationBootstrapper
    {
        private IPasswordHasher _passwordHasher;
        private IUserRepository _userRepository;
        private IAuditLogRepository _auditLogRepository;
        private ISessionManager _sessionManager;
        private IAuthenticationService _authService;

        public MainShellForm BuildMainForm()
        {
            RegisterServices();
            ShowLoginFlow();
            ApplicationStartupContext startupContext = BuildStartupContext();
            return new MainShellForm(startupContext, _sessionManager, _authService);
        }

        private void RegisterServices()
        {
            string connectionString = AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName);
            IDbConnectionFactory dbConnectionFactory = new SqlConnectionFactory(connectionString);

            _passwordHasher = new PasswordHasher();
            _userRepository = new UserRepository(dbConnectionFactory);
            _auditLogRepository = new AuditLogRepository(dbConnectionFactory);
            _sessionManager = new SessionManager();
            _authService = new AuthenticationService(_userRepository, _passwordHasher, _sessionManager, _auditLogRepository);
        }

        private void ShowLoginFlow()
        {
            using (LoginForm loginForm = new LoginForm(_authService))
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    Application.Exit();
                }
            }
        }

        internal ApplicationStartupContext BuildStartupContext()
        {
            string connectionString = AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName);

            IDbConnectionFactory dbConnectionFactory = new SqlConnectionFactory(connectionString);
            IStartupDiagnosticsService startupDiagnosticsService = new StartupDiagnosticsService(dbConnectionFactory);

            return startupDiagnosticsService.BuildContext().Data;
        }
    }
}
