using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;
using SmartMed.DAL.Infrastructure;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.Tests.BLL
{
    [TestClass]
    public class OrderServiceTests
    {
        private MockOrderRepository _orderRepository;
        private MockOrderItemRepository _orderItemRepository;
        private MockMedicineRepository _medicineRepository;
        private MockStockMovementRepository _stockMovementRepository;
        private MockInventoryService _inventoryService;
        private MockOrderNumberGenerator _orderNumberGenerator;
        private IDbConnectionFactory _connectionFactory;
        private MockAuditLogRepository _auditLogRepository;
        private MockSessionManager _sessionManager;
        private IOrderService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _orderRepository = new MockOrderRepository();
            _orderItemRepository = new MockOrderItemRepository();
            _medicineRepository = new MockMedicineRepository();
            _stockMovementRepository = new MockStockMovementRepository();
            _inventoryService = new MockInventoryService();
            _orderNumberGenerator = new MockOrderNumberGenerator();
            _connectionFactory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            _auditLogRepository = new MockAuditLogRepository();
            _sessionManager = new MockSessionManager();

            _service = new OrderService(
                _orderRepository,
                _orderItemRepository,
                _medicineRepository,
                _stockMovementRepository,
                _inventoryService,
                _orderNumberGenerator,
                _connectionFactory,
                _auditLogRepository,
                _sessionManager);
        }

        private void LoginAsCustomer(int customerId)
        {
            _sessionManager.StartCustomerSession(new Customer { Id = customerId, FullName = "Jane Doe", PhoneNumber = "0771234567" });
        }

        [TestMethod]
        public void PlaceOrder_ShouldFail_WhenOrderIsNull()
        {
            OperationResult<int> result = _service.PlaceOrder(null, new List<OrderItem>());

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void PlaceOrder_ShouldFail_WhenCustomerIdMissing()
        {
            OperationResult<int> result = _service.PlaceOrder(new Order { CustomerId = 0 }, new List<OrderItem> { new OrderItem { MedicineId = 1, Quantity = 1 } });

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void PlaceOrder_ShouldFail_WhenNoItems()
        {
            LoginAsCustomer(1);

            OperationResult<int> result = _service.PlaceOrder(new Order { CustomerId = 1 }, new List<OrderItem>());

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void PlaceOrder_ShouldFail_WhenPlacingForAnotherCustomerWithoutStaffRole()
        {
            LoginAsCustomer(1);

            OperationResult<int> result = _service.PlaceOrder(
                new Order { CustomerId = 2 },
                new List<OrderItem> { new OrderItem { MedicineId = 1, Quantity = 1 } });

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void PlaceOrder_ShouldFail_WhenQuantityIsInvalid()
        {
            LoginAsCustomer(1);

            OperationResult<int> result = _service.PlaceOrder(
                new Order { CustomerId = 1 },
                new List<OrderItem> { new OrderItem { MedicineId = 1, Quantity = 0 } });

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void PlaceOrder_ShouldFail_WhenMedicineNotFound()
        {
            LoginAsCustomer(1);

            OperationResult<int> result = _service.PlaceOrder(
                new Order { CustomerId = 1 },
                new List<OrderItem> { new OrderItem { MedicineId = 999, Quantity = 1 } });

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void PlaceOrder_ShouldFail_WhenInsufficientStock()
        {
            LoginAsCustomer(1);
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Paracetamol", DosageForm = DosageForm.Tablet, Unit = "mg", StockQuantity = 2, UnitPrice = 4.50m, IsActive = true }
            };

            OperationResult<int> result = _service.PlaceOrder(
                new Order { CustomerId = 1 },
                new List<OrderItem> { new OrderItem { MedicineId = 1, Quantity = 5 } });

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Message, "Insufficient stock");
        }

        [TestMethod]
        public void PlaceOrder_ShouldFail_WhenPrescriptionRequiredButNotAttached()
        {
            LoginAsCustomer(1);
            _medicineRepository.Medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Amoxicillin", DosageForm = DosageForm.Tablet, Unit = "mg", StockQuantity = 100, UnitPrice = 12.50m, IsActive = true, RequiresPrescription = true }
            };

            OperationResult<int> result = _service.PlaceOrder(
                new Order { CustomerId = 1 },
                new List<OrderItem> { new OrderItem { MedicineId = 1, Quantity = 1 } });

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Message, "prescription");
        }

        [TestMethod]
        public void UpdateOrderStatus_ShouldFail_WhenOrderNotFound()
        {
            _sessionManager.StartSession(new User { Id = 1, Username = "admin", DisplayName = "Admin", Role = RoleType.Administrator });

            OperationResult result = _service.UpdateOrderStatus(999, OrderStatus.Approved);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void UpdateOrderStatus_ShouldFail_WhenNotAuthorized()
        {
            _orderRepository.Orders.Add(new Order { Id = 1, OrderNumber = "ORD-2026-000001", CustomerId = 1, Status = OrderStatus.Pending });

            OperationResult result = _service.UpdateOrderStatus(1, OrderStatus.Approved);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void UpdateOrderStatus_ShouldFail_ForIllegalTransition()
        {
            _sessionManager.StartSession(new User { Id = 1, Username = "admin", DisplayName = "Admin", Role = RoleType.Administrator });
            _orderRepository.Orders.Add(new Order { Id = 1, OrderNumber = "ORD-2026-000001", CustomerId = 1, Status = OrderStatus.Pending });

            OperationResult result = _service.UpdateOrderStatus(1, OrderStatus.Completed);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void UpdateOrderStatus_ShouldSucceed_ForPendingToApproved()
        {
            _sessionManager.StartSession(new User { Id = 1, Username = "admin", DisplayName = "Admin", Role = RoleType.Administrator });
            _orderRepository.Orders.Add(new Order { Id = 1, OrderNumber = "ORD-2026-000001", CustomerId = 1, Status = OrderStatus.Pending });

            OperationResult result = _service.UpdateOrderStatus(1, OrderStatus.Approved);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(OrderStatus.Approved, _orderRepository.Orders[0].Status);
        }

        [TestMethod]
        public void UpdateOrderStatus_ShouldSucceed_ForPendingToRejected()
        {
            _sessionManager.StartSession(new User { Id = 1, Username = "admin", DisplayName = "Admin", Role = RoleType.Administrator });
            _orderRepository.Orders.Add(new Order { Id = 1, OrderNumber = "ORD-2026-000001", CustomerId = 1, Status = OrderStatus.Pending });

            OperationResult result = _service.UpdateOrderStatus(1, OrderStatus.Rejected);

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void UpdateOrderStatus_ShouldSucceed_ForPendingToCancelled_WithoutStockReversal()
        {
            _sessionManager.StartSession(new User { Id = 1, Username = "admin", DisplayName = "Admin", Role = RoleType.Administrator });
            _orderRepository.Orders.Add(new Order { Id = 1, OrderNumber = "ORD-2026-000001", CustomerId = 1, Status = OrderStatus.Pending });

            OperationResult result = _service.UpdateOrderStatus(1, OrderStatus.Cancelled);

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void GetAllOrders_ShouldFail_WhenNotLoggedIn()
        {
            OperationResult<List<Order>> result = _service.GetAllOrders();

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void GetAllOrders_ShouldSucceed_ForStaff()
        {
            _sessionManager.StartSession(new User { Id = 1, Username = "cashier", DisplayName = "Cashier", Role = RoleType.Cashier });

            OperationResult<List<Order>> result = _service.GetAllOrders();

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void GetOrdersByCustomer_ShouldSucceed_ForSelf()
        {
            LoginAsCustomer(1);
            _orderRepository.Orders.Add(new Order { Id = 1, OrderNumber = "ORD-2026-000001", CustomerId = 1, Status = OrderStatus.Pending });

            OperationResult<List<Order>> result = _service.GetOrdersByCustomer(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void GetOrdersByCustomer_ShouldFail_ForDifferentCustomerWithoutStaffRole()
        {
            LoginAsCustomer(1);

            OperationResult<List<Order>> result = _service.GetOrdersByCustomer(2);

            Assert.IsFalse(result.IsSuccess);
        }
    }
}
