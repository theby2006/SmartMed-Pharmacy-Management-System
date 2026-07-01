using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Models;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Infrastructure;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class SalesService : ISalesService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ISaleItemRepository _saleItemRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly IMedicineRepository _medicineRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IPricingService _pricingService;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ISessionManager _sessionManager;

        public SalesService(
            ISaleRepository saleRepository,
            ISaleItemRepository saleItemRepository,
            IPaymentRepository paymentRepository,
            IStockMovementRepository stockMovementRepository,
            IMedicineRepository medicineRepository,
            IInventoryService inventoryService,
            IPricingService pricingService,
            IDbConnectionFactory connectionFactory,
            ISessionManager sessionManager)
        {
            Guard.AgainstNull(saleRepository, nameof(saleRepository));
            Guard.AgainstNull(saleItemRepository, nameof(saleItemRepository));
            Guard.AgainstNull(paymentRepository, nameof(paymentRepository));
            Guard.AgainstNull(stockMovementRepository, nameof(stockMovementRepository));
            Guard.AgainstNull(medicineRepository, nameof(medicineRepository));
            Guard.AgainstNull(inventoryService, nameof(inventoryService));
            Guard.AgainstNull(pricingService, nameof(pricingService));
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            Guard.AgainstNull(sessionManager, nameof(sessionManager));

            _saleRepository = saleRepository;
            _saleItemRepository = saleItemRepository;
            _paymentRepository = paymentRepository;
            _stockMovementRepository = stockMovementRepository;
            _medicineRepository = medicineRepository;
            _inventoryService = inventoryService;
            _pricingService = pricingService;
            _connectionFactory = connectionFactory;
            _sessionManager = sessionManager;
        }

        public OperationResult<List<Sale>> GetAllSales()
        {
            try
            {
                List<Sale> sales = _saleRepository.GetAll();
                return OperationResult<List<Sale>>.Success(sales);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<Sale>>.Failure(ex.Message);
            }
        }

        public OperationResult<Sale> GetSaleById(int id)
        {
            try
            {
                Sale sale = _saleRepository.GetById(id);
                return sale != null
                    ? OperationResult<Sale>.Success(sale)
                    : OperationResult<Sale>.Failure($"Sale with ID {id} not found.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult<Sale>.Failure(ex.Message);
            }
        }

        public OperationResult<Sale> GetSaleByNumber(string saleNumber)
        {
            try
            {
                Sale sale = _saleRepository.GetBySaleNumber(saleNumber);
                return sale != null
                    ? OperationResult<Sale>.Success(sale)
                    : OperationResult<Sale>.Failure($"Sale with number '{saleNumber}' not found.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult<Sale>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Sale>> SearchSales(string keyword)
        {
            try
            {
                List<Sale> sales = _saleRepository.Search(keyword);
                return OperationResult<List<Sale>>.Success(sales);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<Sale>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Sale>> GetSalesByDateRange(DateTime fromDate, DateTime toDate)
        {
            try
            {
                List<Sale> sales = _saleRepository.GetByDateRange(fromDate, toDate);
                return OperationResult<List<Sale>>.Success(sales);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<Sale>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Sale>> GetSalesByCashier(int cashierId)
        {
            try
            {
                List<Sale> sales = _saleRepository.GetByCashier(cashierId);
                return OperationResult<List<Sale>>.Success(sales);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<Sale>>.Failure(ex.Message);
            }
        }

        private OperationResult AuthorizeSaleOperation()
        {
            if (!_sessionManager.IsActive || _sessionManager.CurrentSession == null)
                return OperationResult.Failure("You must be logged in to perform sales operations.");

            if (!_sessionManager.HasRole(RoleType.Administrator) &&
                !_sessionManager.HasRole(RoleType.Pharmacist) &&
                !_sessionManager.HasRole(RoleType.Cashier))
            {
                return OperationResult.Failure("You are not authorized to perform sales operations.");
            }

            return OperationResult.Success();
        }

        public OperationResult<int> CreateSale(Sale sale, List<SaleItem> items, Payment payment)
        {
            OperationResult authResult = AuthorizeSaleOperation();
            if (!authResult.IsSuccess)
                return OperationResult<int>.Failure(authResult.Message);

            IDbConnection connection = null;
            IDbTransaction transaction = null;

            try
            {
                if (sale == null)
                    return OperationResult<int>.Failure("Sale information is required.");

                if (items == null || items.Count == 0)
                    return OperationResult<int>.Failure("Sale must contain at least one item.");

                if (payment == null)
                    return OperationResult<int>.Failure("Payment information is required.");

                if (string.IsNullOrWhiteSpace(sale.SaleNumber))
                    return OperationResult<int>.Failure("Sale number is required.");

                if (sale.CashierId <= 0)
                    return OperationResult<int>.Failure("Cashier information is missing.");

                if (payment.AmountPaid <= 0)
                    return OperationResult<int>.Failure("Amount paid must be greater than zero.");

                foreach (SaleItem item in items)
                {
                    if (item.MedicineId <= 0)
                        return OperationResult<int>.Failure("Invalid medicine in sale items.");

                    if (item.Quantity <= 0)
                        return OperationResult<int>.Failure($"Invalid quantity for medicine ID {item.MedicineId}.");

                    if (item.SellingPrice < 0)
                        return OperationResult<int>.Failure($"Invalid selling price for medicine ID {item.MedicineId}.");

                    if (item.DiscountPercent < 0 || item.DiscountPercent > 100)
                        return OperationResult<int>.Failure($"Invalid discount percentage for medicine ID {item.MedicineId}.");

                    if (item.TaxPercent < 0 || item.TaxPercent > 100)
                        return OperationResult<int>.Failure($"Invalid tax percentage for medicine ID {item.MedicineId}.");
                }

                using (connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    decimal subTotal = 0m;
                    List<BatchDeduction> allDeductions = new List<BatchDeduction>();

                    foreach (SaleItem item in items)
                    {
                        OperationResult<List<BatchDeduction>> dedResult =
                            _inventoryService.DeductFIFO(item.MedicineId, item.Quantity, connection, transaction);

                        if (!dedResult.IsSuccess)
                            return OperationResult<int>.Failure(dedResult.Message);

                        allDeductions.AddRange(dedResult.Data);
                        subTotal += item.Quantity * item.SellingPrice;
                    }

                    decimal discountAmount = _pricingService.CalculateDiscountAmount(subTotal, sale.DiscountPercent);
                    decimal taxAmount = _pricingService.CalculateTaxAmount(subTotal - discountAmount, sale.TaxPercent);
                    decimal grandTotal = _pricingService.CalculateGrandTotal(subTotal, discountAmount, taxAmount);

                    sale.SubTotal = subTotal;
                    sale.DiscountAmount = discountAmount;
                    sale.TaxAmount = taxAmount;
                    sale.GrandTotal = grandTotal;
                    sale.SaleDate = DateTime.Now;
                    sale.Status = SaleStatus.Completed;
                    sale.Notes = sale.Notes ?? string.Empty;

                    int saleId = _saleRepository.Add(sale, connection, transaction);

                    foreach (SaleItem item in items)
                    {
                        item.SaleId = saleId;
                        item.LineTotal = _pricingService.CalculateLineTotal(
                            item.Quantity, item.SellingPrice, item.DiscountPercent, item.TaxPercent);
                    }

                    _saleItemRepository.AddRange(items, connection, transaction);

                    payment.SaleId = saleId;
                    payment.PaymentStatus = PaymentStatus.Paid;
                    payment.ChangeAmount = _pricingService.CalculateChange(payment.AmountPaid, grandTotal);

                    _paymentRepository.Add(payment, connection, transaction);

                    foreach (BatchDeduction deduction in allDeductions)
                    {
                        StockMovement movement = new StockMovement
                        {
                            MedicineId = deduction.MedicineId,
                            StockBatchId = deduction.StockBatchId,
                            Quantity = -deduction.QuantityDeducted,
                            MovementType = MovementType.StockOut,
                            ReferenceId = saleId,
                            ReferenceType = "Sale",
                            UnitPrice = deduction.SellingPrice,
                            TotalAmount = deduction.SellingPrice * deduction.QuantityDeducted,
                            Notes = $"Sale #{sale.SaleNumber} deducted {deduction.QuantityDeducted} from batch {deduction.BatchNumber}",
                            CreatedByUserId = _sessionManager.CurrentSession?.UserId ?? 0
                        };

                        _stockMovementRepository.Add(movement, connection, transaction);
                    }

                    foreach (BatchDeduction deduction in allDeductions)
                    {
                        _medicineRepository.UpdateStockQuantity(
                            deduction.MedicineId, -deduction.QuantityDeducted, connection, transaction);
                    }

                    transaction.Commit();
                    return OperationResult<int>.Success(saleId);
                }
            }
            catch (ValidationException ex)
            {
                transaction?.Rollback();
                return OperationResult<int>.Failure(ex.Message);
            }
            catch (DataAccessException ex)
            {
                transaction?.Rollback();
                return OperationResult<int>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                return OperationResult<int>.Failure($"An unexpected error occurred while creating the sale: {ex.Message}");
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public OperationResult CancelSale(int saleId)
        {
            OperationResult authResult = AuthorizeSaleOperation();
            if (!authResult.IsSuccess)
                return authResult;

            IDbConnection connection = null;
            IDbTransaction transaction = null;

            try
            {
                if (saleId <= 0)
                    return OperationResult.Failure("Invalid sale ID.");

                Sale sale = _saleRepository.GetById(saleId);
                if (sale == null)
                    return OperationResult.Failure($"Sale with ID {saleId} not found.");

                if (sale.Status == SaleStatus.Cancelled)
                    return OperationResult.Failure("Sale is already cancelled.");

                using (connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    _saleRepository.UpdateStatus(saleId, SaleStatus.Cancelled, connection, transaction);

                    List<SaleItem> items = _saleItemRepository.GetBySaleId(saleId);
                    foreach (SaleItem item in items)
                    {
                        _medicineRepository.UpdateStockQuantity(
                            item.MedicineId, item.Quantity, connection, transaction);
                    }

                    transaction.Commit();
                    return OperationResult.Success();
                }
            }
            catch (DataAccessException ex)
            {
                transaction?.Rollback();
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult<List<SaleItem>> GetSaleItems(int saleId)
        {
            try
            {
                List<SaleItem> items = _saleItemRepository.GetBySaleId(saleId);
                return OperationResult<List<SaleItem>>.Success(items);
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<SaleItem>>.Failure(ex.Message);
            }
        }
    }
}
