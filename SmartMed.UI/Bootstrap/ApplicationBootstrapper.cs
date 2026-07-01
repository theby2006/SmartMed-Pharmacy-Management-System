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
        private IMedicineService _medicineService;
        private IMedicineCategoryService _medicineCategoryService;
        private ISupplierService _supplierService;
        private IPurchaseService _purchaseService;
        private IPricingService _pricingService;
        private IInventoryService _inventoryService;
        private ISalesService _salesService;
        private IPaymentService _paymentService;
        private ISaleNumberGenerator _saleNumberGenerator;
        private IReportService _reportService;

        public MainShellForm BuildMainForm()
        {
            RegisterServices();
            ShowLoginFlow();
            ApplicationStartupContext startupContext = BuildStartupContext();
            return new MainShellForm(
                startupContext,
                _sessionManager,
                _authService,
                _salesService,
                _paymentService,
                _pricingService,
                _medicineService,
                _inventoryService,
                _saleNumberGenerator,
                _reportService);
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

            IMedicineCategoryRepository medicineCategoryRepo = new MedicineCategoryRepository(dbConnectionFactory);
            IMedicineRepository medicineRepo = new MedicineRepository(dbConnectionFactory);
            _medicineCategoryService = new MedicineCategoryService(medicineCategoryRepo, medicineRepo);
            _medicineService = new MedicineService(medicineRepo, medicineCategoryRepo);
            _supplierService = new SupplierService(new SupplierRepository(dbConnectionFactory), _auditLogRepository, _sessionManager);

            _pricingService = new PricingService();

            IStockBatchRepository stockBatchRepo = new StockBatchRepository(dbConnectionFactory);
            IStockMovementRepository stockMovementRepo = new StockMovementRepository(dbConnectionFactory);
            _inventoryService = new InventoryService(stockBatchRepo, stockMovementRepo, medicineRepo, dbConnectionFactory);

            _purchaseService = new PurchaseService(
                new PurchaseRepository(dbConnectionFactory),
                new PurchaseItemRepository(dbConnectionFactory),
                stockBatchRepo,
                stockMovementRepo,
                medicineRepo,
                new SupplierRepository(dbConnectionFactory),
                dbConnectionFactory);

            _saleNumberGenerator = new SaleNumberGenerator(dbConnectionFactory);
            _paymentService = new PaymentService(new PaymentRepository(dbConnectionFactory));

            IReportRepository reportRepository = new ReportRepository(dbConnectionFactory);
            _reportService = new ReportService(reportRepository);

            _salesService = new SalesService(
                new SaleRepository(dbConnectionFactory),
                new SaleItemRepository(dbConnectionFactory),
                new PaymentRepository(dbConnectionFactory),
                stockMovementRepo,
                medicineRepo,
                _inventoryService,
                _pricingService,
                dbConnectionFactory,
                _sessionManager);
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
