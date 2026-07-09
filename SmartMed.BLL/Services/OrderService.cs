using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Models;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IMedicineRepository _medicineRepository;
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ISessionManager _sessionManager;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IMedicineRepository medicineRepository,
            IStockMovementRepository stockMovementRepository,
            IInventoryService inventoryService,
            IOrderNumberGenerator orderNumberGenerator,
            IDbConnectionFactory connectionFactory,
            IAuditLogRepository auditLogRepository,
            ISessionManager sessionManager)
        {
            Guard.AgainstNull(orderRepository, nameof(orderRepository));
            Guard.AgainstNull(orderItemRepository, nameof(orderItemRepository));
            Guard.AgainstNull(medicineRepository, nameof(medicineRepository));
            Guard.AgainstNull(stockMovementRepository, nameof(stockMovementRepository));
            Guard.AgainstNull(inventoryService, nameof(inventoryService));
            Guard.AgainstNull(orderNumberGenerator, nameof(orderNumberGenerator));
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            Guard.AgainstNull(auditLogRepository, nameof(auditLogRepository));
            Guard.AgainstNull(sessionManager, nameof(sessionManager));

            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _medicineRepository = medicineRepository;
            _stockMovementRepository = stockMovementRepository;
            _inventoryService = inventoryService;
            _orderNumberGenerator = orderNumberGenerator;
            _connectionFactory = connectionFactory;
            _auditLogRepository = auditLogRepository;
            _sessionManager = sessionManager;
        }

        private string CurrentMachineName => Environment.MachineName;

        private int? CurrentUserId => _sessionManager.CurrentSession?.UserId;

        private string CurrentUsername => _sessionManager.CurrentSession?.Username ?? "System";

        private OperationResult AuthorizeStaffRole()
        {
            if (!_sessionManager.IsActive || _sessionManager.CurrentSession == null)
                return OperationResult.Failure("You must be logged in to perform this operation.");

            if (!_sessionManager.HasRole(RoleType.Administrator) &&
                !_sessionManager.HasRole(RoleType.Pharmacist) &&
                !_sessionManager.HasRole(RoleType.Cashier))
            {
                return OperationResult.Failure("You are not authorized to perform this operation.");
            }

            return OperationResult.Success();
        }

        public OperationResult<int> PlaceOrder(Order order, List<OrderItem> items)
        {
            try
            {
                if (order == null)
                    return OperationResult<int>.Failure("Order information is required.");

                if (order.CustomerId <= 0)
                    return OperationResult<int>.Failure("Customer information is missing.");

                if (items == null || items.Count == 0)
                    return OperationResult<int>.Failure("Order must contain at least one item.");

                bool isSelf = _sessionManager.CurrentSession?.CustomerId == order.CustomerId;
                if (!isSelf)
                {
                    OperationResult authResult = AuthorizeStaffRole();
                    if (!authResult.IsSuccess)
                        return OperationResult<int>.Failure("You are not authorized to place an order for this customer.");
                }

                decimal subTotal = 0m;
                decimal discountAmount = 0m;
                List<string> missingPrescriptionMedicines = new List<string>();

                foreach (OrderItem item in items)
                {
                    if (item.MedicineId <= 0)
                        return OperationResult<int>.Failure("Invalid medicine in order items.");

                    if (item.Quantity <= 0)
                        return OperationResult<int>.Failure($"Invalid quantity for medicine ID {item.MedicineId}.");

                    Medicine medicine = _medicineRepository.GetById(item.MedicineId);
                    if (medicine == null || !medicine.IsActive)
                        return OperationResult<int>.Failure($"Medicine ID {item.MedicineId} is not available.");

                    if (medicine.StockQuantity < item.Quantity)
                        return OperationResult<int>.Failure($"Insufficient stock for '{medicine.Name}'. Available: {medicine.StockQuantity}, Requested: {item.Quantity}.");

                    if (medicine.RequiresPrescription && string.IsNullOrWhiteSpace(order.PrescriptionFilePath))
                        missingPrescriptionMedicines.Add(medicine.Name);

                    item.UnitPrice = medicine.UnitPrice;
                    item.DiscountPercent = medicine.DiscountPercent;
                    item.LineTotal = item.Quantity * item.UnitPrice * (1 - item.DiscountPercent / 100m);

                    subTotal += item.Quantity * item.UnitPrice;
                    discountAmount += item.Quantity * item.UnitPrice * (item.DiscountPercent / 100m);
                }

                if (missingPrescriptionMedicines.Count > 0)
                    return OperationResult<int>.Failure(
                        "A prescription file must be attached before ordering: " + string.Join(", ", missingPrescriptionMedicines));

                OperationResult<string> numberResult = _orderNumberGenerator.GenerateNextNumber();
                if (!numberResult.IsSuccess)
                    return OperationResult<int>.Failure(numberResult.Message);

                order.OrderNumber = numberResult.Data;
                order.OrderDate = DateTime.UtcNow;
                order.Status = OrderStatus.Pending;
                order.SubTotal = subTotal;
                order.DiscountAmount = discountAmount;
                order.TaxAmount = 0m;
                order.GrandTotal = subTotal - discountAmount;

                IDbConnection connection = null;
                IDbTransaction transaction = null;

                try
                {
                    using (connection = _connectionFactory.CreateConnection())
                    {
                        connection.Open();
                        transaction = connection.BeginTransaction();

                        int orderId = _orderRepository.Add(order, connection, transaction);

                        foreach (OrderItem item in items)
                            item.OrderId = orderId;

                        _orderItemRepository.AddRange(items, connection, transaction);

                        transaction.Commit();

                        _auditLogRepository.Log(CurrentUserId, CurrentUsername, AuditAction.OrderPlaced,
                            CurrentMachineName, $"Order '{order.OrderNumber}' placed for customer {order.CustomerId}");

                        return OperationResult<int>.Success(orderId, "Order placed successfully.");
                    }
                }
                catch (Exception ex) when (!(ex is AppException))
                {
                    transaction?.Rollback();
                    return OperationResult<int>.Failure($"An unexpected error occurred while placing the order: {ex.Message}");
                }
            }
            catch (ValidationException ex)
            {
                return OperationResult<int>.Failure(ex.Message);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<int>.Failure(ex.Message);
            }
        }

        public OperationResult<Order> GetOrderById(int id)
        {
            Order order = _orderRepository.GetById(id);
            if (order == null)
                return OperationResult<Order>.Failure("Order not found.");

            bool isSelf = _sessionManager.CurrentSession?.CustomerId == order.CustomerId;
            if (!isSelf)
            {
                OperationResult authResult = AuthorizeStaffRole();
                if (!authResult.IsSuccess)
                    return OperationResult<Order>.Failure(authResult.Message);
            }

            order.Items = _orderItemRepository.GetByOrderId(id);
            return OperationResult<Order>.Success(order);
        }

        public OperationResult<List<Order>> GetOrdersByCustomer(int customerId)
        {
            bool isSelf = _sessionManager.CurrentSession?.CustomerId == customerId;
            if (!isSelf)
            {
                OperationResult authResult = AuthorizeStaffRole();
                if (!authResult.IsSuccess)
                    return OperationResult<List<Order>>.Failure(authResult.Message);
            }

            try
            {
                List<Order> orders = _orderRepository.GetByCustomerId(customerId);
                return OperationResult<List<Order>>.Success(orders);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<Order>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Order>> GetAllOrders()
        {
            OperationResult authResult = AuthorizeStaffRole();
            if (!authResult.IsSuccess)
                return OperationResult<List<Order>>.Failure(authResult.Message);

            try
            {
                List<Order> orders = _orderRepository.GetAll();
                return OperationResult<List<Order>>.Success(orders);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<Order>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Order>> GetOrdersByStatus(OrderStatus status)
        {
            OperationResult authResult = AuthorizeStaffRole();
            if (!authResult.IsSuccess)
                return OperationResult<List<Order>>.Failure(authResult.Message);

            try
            {
                List<Order> orders = _orderRepository.GetByStatus(status);
                return OperationResult<List<Order>>.Success(orders);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<Order>>.Failure(ex.Message);
            }
        }

        public OperationResult UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            OperationResult authResult = AuthorizeStaffRole();
            if (!authResult.IsSuccess)
                return authResult;

            Order order = _orderRepository.GetById(orderId);
            if (order == null)
                return OperationResult.Failure("Order not found.");

            OperationResult transitionResult = ValidateTransition(order.Status, newStatus);
            if (!transitionResult.IsSuccess)
                return transitionResult;

            try
            {
                if (newStatus == OrderStatus.Processing)
                {
                    return CommitStockForOrder(order);
                }

                if (newStatus == OrderStatus.Cancelled &&
                    (order.Status == OrderStatus.Processing || order.Status == OrderStatus.Completed))
                {
                    return ReverseStockForOrder(order);
                }

                _orderRepository.UpdateStatus(orderId, newStatus);

                _auditLogRepository.Log(CurrentUserId, CurrentUsername, AuditAction.OrderStatusChanged,
                    CurrentMachineName, $"Order '{order.OrderNumber}' status changed from {order.Status} to {newStatus}");

                return OperationResult.Success("Order status updated successfully.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        private static OperationResult ValidateTransition(OrderStatus current, OrderStatus next)
        {
            switch (current)
            {
                case OrderStatus.Pending:
                    if (next == OrderStatus.Approved || next == OrderStatus.Rejected || next == OrderStatus.Cancelled)
                        return OperationResult.Success();
                    break;
                case OrderStatus.Approved:
                    if (next == OrderStatus.Processing || next == OrderStatus.Cancelled)
                        return OperationResult.Success();
                    break;
                case OrderStatus.Processing:
                    if (next == OrderStatus.Completed || next == OrderStatus.Cancelled)
                        return OperationResult.Success();
                    break;
            }

            return OperationResult.Failure($"Cannot change order status from {current} to {next}.");
        }

        private OperationResult CommitStockForOrder(Order order)
        {
            IDbConnection connection = null;
            IDbTransaction transaction = null;

            try
            {
                List<OrderItem> items = _orderItemRepository.GetByOrderId(order.Id);

                using (connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    List<BatchDeduction> allDeductions = new List<BatchDeduction>();

                    foreach (OrderItem item in items)
                    {
                        OperationResult<List<BatchDeduction>> dedResult =
                            _inventoryService.DeductFIFO(item.MedicineId, item.Quantity, connection, transaction);

                        if (!dedResult.IsSuccess)
                        {
                            transaction.Rollback();
                            return OperationResult.Failure(dedResult.Message);
                        }

                        allDeductions.AddRange(dedResult.Data);
                    }

                    foreach (BatchDeduction deduction in allDeductions)
                    {
                        StockMovement movement = new StockMovement
                        {
                            MedicineId = deduction.MedicineId,
                            StockBatchId = deduction.StockBatchId,
                            Quantity = -deduction.QuantityDeducted,
                            MovementType = MovementType.StockOut,
                            ReferenceId = order.Id,
                            ReferenceType = "Order",
                            UnitPrice = deduction.SellingPrice,
                            TotalAmount = deduction.SellingPrice * deduction.QuantityDeducted,
                            Notes = $"Order #{order.OrderNumber} deducted {deduction.QuantityDeducted} from batch {deduction.BatchNumber}",
                            CreatedByUserId = CurrentUserId ?? 0
                        };

                        _stockMovementRepository.Add(movement, connection, transaction);
                        _medicineRepository.UpdateStockQuantity(deduction.MedicineId, -deduction.QuantityDeducted, connection, transaction);
                    }

                    _orderRepository.UpdateStatus(order.Id, OrderStatus.Processing, connection, transaction);

                    transaction.Commit();
                }

                _auditLogRepository.Log(CurrentUserId, CurrentUsername, AuditAction.OrderStatusChanged,
                    CurrentMachineName, $"Order '{order.OrderNumber}' moved to Processing; stock committed.");

                return OperationResult.Success("Order moved to processing and stock has been committed.");
            }
            catch (Exception ex) when (!(ex is AppException))
            {
                transaction?.Rollback();
                return OperationResult.Failure($"An unexpected error occurred while committing stock: {ex.Message}");
            }
        }

        private OperationResult ReverseStockForOrder(Order order)
        {
            try
            {
                List<OrderItem> items = _orderItemRepository.GetByOrderId(order.Id);

                using (IDbConnection connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (IDbTransaction transaction = connection.BeginTransaction())
                    {
                        foreach (OrderItem item in items)
                        {
                            _medicineRepository.UpdateStockQuantity(item.MedicineId, item.Quantity, connection, transaction);
                        }

                        _orderRepository.UpdateStatus(order.Id, OrderStatus.Cancelled, connection, transaction);

                        transaction.Commit();
                    }
                }

                _auditLogRepository.Log(CurrentUserId, CurrentUsername, AuditAction.OrderCancelled,
                    CurrentMachineName, $"Order '{order.OrderNumber}' cancelled; stock reversed.");

                return OperationResult.Success("Order cancelled and stock reversed successfully.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }
    }
}
