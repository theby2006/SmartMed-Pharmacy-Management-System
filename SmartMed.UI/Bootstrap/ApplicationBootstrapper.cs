using System.Windows.Forms;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;
using SmartMed.DAL.Infrastructure;
using SmartMed.DAL.Interfaces;
using SmartMed.DAL.Repositories;
using SmartMed.Models.Diagnostics;
using SmartMed.Models.Enums;
using SmartMed.UI.Forms;

namespace SmartMed.UI.Bootstrap
{
    public class ApplicationBootstrapper
    {
        private IPasswordHasher _passwordHasher;
        private IUserRepository _userRepository;
        private ICustomerRepository _customerRepository;
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
        private ICustomerService _customerService;
        private IOrderService _orderService;
        private IOrderNumberGenerator _orderNumberGenerator;
        private IPrescriptionService _prescriptionService;
        private IMedicineSearchService _medicineSearchService;

        public Form BuildMainForm()
        {
            RegisterServices();
            ShowLoginFlow();
            ApplicationStartupContext startupContext = BuildStartupContext();

            bool isCustomer = _sessionManager.CurrentSession?.Role == RoleType.Customer;

            if (isCustomer)
            {
                return new CustomerShellForm(
                    startupContext,
                    _sessionManager,
                    _authService,
                    _medicineService,
                    _medicineCategoryService,
                    _medicineSearchService,
                    _orderService,
                    _prescriptionService,
                    _pricingService,
                    _reportService);
            }

            return new MainShellForm(
                startupContext,
                _sessionManager,
                _authService,
                _salesService,
                _paymentService,
                _pricingService,
                _medicineService,
                _medicineCategoryService,
                _supplierService,
                _purchaseService,
                _inventoryService,
                _saleNumberGenerator,
                _reportService,
                _customerService,
                _orderService);
        }

        private void RegisterServices()
        {
            string connectionString = AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName);
            IDbConnectionFactory dbConnectionFactory = new SqlConnectionFactory(connectionString);
            DatabaseConnectivityChecker.VerifyConnection(dbConnectionFactory);

            _passwordHasher = new PasswordHasher();
            _userRepository = new UserRepository(dbConnectionFactory);
            _customerRepository = new CustomerRepository(dbConnectionFactory);
            _auditLogRepository = new AuditLogRepository(dbConnectionFactory);
            _sessionManager = new SessionManager();
            _authService = new AuthenticationService(_userRepository, _customerRepository, _passwordHasher, _sessionManager, _auditLogRepository);

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

            _customerService = new CustomerService(_customerRepository, _passwordHasher, _auditLogRepository, _sessionManager);

            IOrderRepository orderRepository = new OrderRepository(dbConnectionFactory);
            IOrderItemRepository orderItemRepository = new OrderItemRepository(dbConnectionFactory);
            _orderNumberGenerator = new OrderNumberGenerator(dbConnectionFactory);
            _orderService = new OrderService(
                orderRepository,
                orderItemRepository,
                medicineRepo,
                stockMovementRepo,
                _inventoryService,
                _orderNumberGenerator,
                dbConnectionFactory,
                _auditLogRepository,
                _sessionManager);

            _prescriptionService = new PrescriptionService(orderRepository, _auditLogRepository, _sessionManager);
            _medicineSearchService = new MedicineSearchService(medicineRepo);
        }

        private void ShowLoginFlow()
        {
            using (LoginForm loginForm = new LoginForm(_authService, _customerService))
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
