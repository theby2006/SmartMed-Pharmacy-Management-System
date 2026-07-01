using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Infrastructure;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IPurchaseItemRepository _purchaseItemRepository;
        private readonly IStockBatchRepository _stockBatchRepository;
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly IMedicineRepository _medicineRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IDbConnectionFactory _connectionFactory;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            IPurchaseItemRepository purchaseItemRepository,
            IStockBatchRepository stockBatchRepository,
            IStockMovementRepository stockMovementRepository,
            IMedicineRepository medicineRepository,
            ISupplierRepository supplierRepository,
            IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(purchaseRepository, nameof(purchaseRepository));
            Guard.AgainstNull(purchaseItemRepository, nameof(purchaseItemRepository));
            Guard.AgainstNull(stockBatchRepository, nameof(stockBatchRepository));
            Guard.AgainstNull(stockMovementRepository, nameof(stockMovementRepository));
            Guard.AgainstNull(medicineRepository, nameof(medicineRepository));
            Guard.AgainstNull(supplierRepository, nameof(supplierRepository));
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _purchaseRepository = purchaseRepository;
            _purchaseItemRepository = purchaseItemRepository;
            _stockBatchRepository = stockBatchRepository;
            _stockMovementRepository = stockMovementRepository;
            _medicineRepository = medicineRepository;
            _supplierRepository = supplierRepository;
            _connectionFactory = connectionFactory;
        }

        public OperationResult<List<Purchase>> GetAllPurchases()
        {
            try
            {
                List<Purchase> purchases = _purchaseRepository.GetAll();
                return OperationResult<List<Purchase>>.Success(purchases);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<Purchase>>.Failure(ex.Message);
            }
        }

        public OperationResult<Purchase> GetPurchaseById(int id)
        {
            try
            {
                Purchase purchase = _purchaseRepository.GetById(id);
                if (purchase == null)
                    return OperationResult<Purchase>.Failure("Purchase not found.");

                purchase.Items = _purchaseItemRepository.GetByPurchaseId(id);
                return OperationResult<Purchase>.Success(purchase);
            }
            catch (ValidationException ex)
            {
                return OperationResult<Purchase>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Purchase>> SearchPurchases(string keyword)
        {
            try
            {
                Guard.AgainstNullOrWhiteSpace(keyword, nameof(keyword));
                List<Purchase> purchases = _purchaseRepository.Search(keyword);
                return OperationResult<List<Purchase>>.Success(purchases);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<Purchase>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Purchase>> GetPurchasesBySupplier(int supplierId)
        {
            try
            {
                List<Purchase> purchases = _purchaseRepository.GetBySupplier(supplierId);
                return OperationResult<List<Purchase>>.Success(purchases);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<Purchase>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<Purchase>> GetPurchasesByDateRange(DateTime fromDate, DateTime toDate)
        {
            try
            {
                List<Purchase> purchases = _purchaseRepository.GetByDateRange(fromDate, toDate);
                return OperationResult<List<Purchase>>.Success(purchases);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<Purchase>>.Failure(ex.Message);
            }
        }

        public OperationResult<int> CreatePurchase(Purchase purchase)
        {
            try
            {
                Guard.AgainstNull(purchase, nameof(purchase));
                Guard.AgainstNullOrWhiteSpace(purchase.PurchaseNumber, nameof(purchase.PurchaseNumber));

                if (purchase.PurchaseNumber.Length > 50)
                    return OperationResult<int>.Failure("Purchase number must not exceed 50 characters.");

                if (purchase.Remarks != null && purchase.Remarks.Length > 500)
                    return OperationResult<int>.Failure("Remarks must not exceed 500 characters.");

                if (purchase.InvoiceNumber != null && purchase.InvoiceNumber.Length > 100)
                    return OperationResult<int>.Failure("Invoice number must not exceed 100 characters.");

                Purchase existing = _purchaseRepository.GetByPurchaseNumber(purchase.PurchaseNumber);
                if (existing != null)
                    return OperationResult<int>.Failure("A purchase with this number already exists.");

                if (purchase.InvoiceNumber != null)
                {
                    List<Purchase> byInvoice = _purchaseRepository.Search(purchase.InvoiceNumber);
                    foreach (Purchase p in byInvoice)
                    {
                        if (string.Equals(p.InvoiceNumber, purchase.InvoiceNumber, StringComparison.OrdinalIgnoreCase))
                            return OperationResult<int>.Failure("A purchase with this invoice number already exists.");
                    }
                }

                Supplier supplier = _supplierRepository.GetById(purchase.SupplierId);
                if (supplier == null)
                    return OperationResult<int>.Failure("Supplier not found.");
                if (!supplier.IsActive)
                    return OperationResult<int>.Failure("Supplier is not active. Cannot create purchase for inactive supplier.");

                if (purchase.Items == null || purchase.Items.Count == 0)
                    return OperationResult<int>.Failure("Purchase must have at least one item.");

                for (int i = 0; i < purchase.Items.Count; i++)
                {
                    PurchaseItem item = purchase.Items[i];
                    if (item.MedicineId <= 0)
                        return OperationResult<int>.Failure($"Item {i + 1}: Medicine is required.");

                    if (string.IsNullOrWhiteSpace(item.BatchNumber))
                        return OperationResult<int>.Failure($"Item {i + 1}: Batch number is required.");

                    if (item.BatchNumber.Length > 100)
                        return OperationResult<int>.Failure($"Item {i + 1}: Batch number must not exceed 100 characters.");

                    if (item.Quantity <= 0)
                        return OperationResult<int>.Failure($"Item {i + 1}: Quantity must be greater than zero.");

                    if (item.PurchasePrice <= 0)
                        return OperationResult<int>.Failure($"Item {i + 1}: Purchase price must be greater than zero.");

                    if (item.SellingPrice < item.PurchasePrice)
                        return OperationResult<int>.Failure($"Item {i + 1}: Selling price cannot be less than purchase price.");

                    if (item.ExpiryDate <= DateTime.UtcNow)
                        return OperationResult<int>.Failure($"Item {i + 1}: Expiry date must be in the future.");

                    if (item.DiscountPercent < 0 || item.DiscountPercent > 100)
                        return OperationResult<int>.Failure($"Item {i + 1}: Discount must be between 0 and 100.");

                    if (item.TaxPercent < 0 || item.TaxPercent > 100)
                        return OperationResult<int>.Failure($"Item {i + 1}: Tax must be between 0 and 100.");

                    Medicine medicine = _medicineRepository.GetById(item.MedicineId);
                    if (medicine == null)
                        return OperationResult<int>.Failure($"Item {i + 1}: Medicine not found.");
                    if (!medicine.IsActive)
                        return OperationResult<int>.Failure($"Item {i + 1}: Medicine '{medicine.Name}' is not active.");

                    StockBatch existingBatch = _stockBatchRepository.GetBatch(item.MedicineId, item.BatchNumber);
                    if (existingBatch != null)
                        return OperationResult<int>.Failure($"Item {i + 1}: Batch '{item.BatchNumber}' already exists for this medicine.");

                    if (_purchaseItemRepository.ExistsByMedicineAndBatch(item.MedicineId, item.BatchNumber))
                        return OperationResult<int>.Failure($"Item {i + 1}: Batch '{item.BatchNumber}' already exists in a pending purchase.");

                    decimal subtotal = item.Quantity * item.PurchasePrice;
                    decimal discountAmount = subtotal * (item.DiscountPercent / 100m);
                    decimal taxAmount = (subtotal - discountAmount) * (item.TaxPercent / 100m);
                    item.LineTotal = subtotal - discountAmount + taxAmount;
                }

                purchase.Status = PurchaseStatus.Pending;
                purchase.CreatedDate = DateTime.UtcNow;

                if (purchase.PurchaseDate == default)
                    purchase.PurchaseDate = DateTime.UtcNow;

                decimal totalAmount = 0;
                foreach (PurchaseItem item in purchase.Items)
                    totalAmount += item.LineTotal;
                purchase.TotalAmount = totalAmount;

                int purchaseId = _purchaseRepository.Add(purchase);

                foreach (PurchaseItem item in purchase.Items)
                {
                    item.PurchaseId = purchaseId;
                }
                _purchaseItemRepository.AddRange(purchase.Items);

                return OperationResult<int>.Success(purchaseId, "Purchase created successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult<int>.Failure(ex.Message);
            }
        }

        public OperationResult ConfirmPurchase(int purchaseId)
        {
            try
            {
                Purchase purchase = _purchaseRepository.GetById(purchaseId);
                if (purchase == null)
                    return OperationResult.Failure("Purchase not found.");

                if (purchase.Status != PurchaseStatus.Pending)
                    return OperationResult.Failure("Only pending purchases can be confirmed.");

                List<PurchaseItem> items = _purchaseItemRepository.GetByPurchaseId(purchaseId);
                if (items.Count == 0)
                    return OperationResult.Failure("Purchase has no items to confirm.");

                Supplier supplier = _supplierRepository.GetById(purchase.SupplierId);
                if (supplier == null)
                    return OperationResult.Failure("Supplier not found.");
                if (!supplier.IsActive)
                    return OperationResult.Failure("Supplier is not active.");

                foreach (PurchaseItem item in items)
                {
                    Medicine medicine = _medicineRepository.GetById(item.MedicineId);
                    if (medicine == null)
                        return OperationResult.Failure($"Medicine with ID {item.MedicineId} not found.");
                    if (!medicine.IsActive)
                        return OperationResult.Failure($"Medicine '{medicine.Name}' is not active.");

                    if (item.ExpiryDate <= DateTime.UtcNow)
                        return OperationResult.Failure($"Expiry date for batch '{item.BatchNumber}' must be in the future.");

                    if (item.Quantity <= 0)
                        return OperationResult.Failure($"Quantity for batch '{item.BatchNumber}' must be greater than zero.");

                    if (item.PurchasePrice <= 0)
                        return OperationResult.Failure($"Purchase price for batch '{item.BatchNumber}' must be greater than zero.");

                    if (item.SellingPrice < item.PurchasePrice)
                        return OperationResult.Failure($"Selling price for batch '{item.BatchNumber}' cannot be less than purchase price.");

                    StockBatch existingBatch = _stockBatchRepository.GetBatch(item.MedicineId, item.BatchNumber);
                    if (existingBatch != null)
                        return OperationResult.Failure($"Batch '{item.BatchNumber}' already exists for this medicine.");

                    if (_purchaseItemRepository.ExistsByMedicineAndBatch(item.MedicineId, item.BatchNumber))
                        return OperationResult.Failure($"Batch '{item.BatchNumber}' already exists in a pending purchase.");
                }

                decimal totalAmount = 0;
                foreach (PurchaseItem item in items)
                {
                    decimal subtotal = item.Quantity * item.PurchasePrice;
                    decimal discountAmount = subtotal * (item.DiscountPercent / 100m);
                    decimal taxAmount = (subtotal - discountAmount) * (item.TaxPercent / 100m);
                    item.LineTotal = subtotal - discountAmount + taxAmount;
                    totalAmount += item.LineTotal;
                }

                using (SqlUnitOfWork uow = new SqlUnitOfWork(_connectionFactory))
                {
                    uow.BeginTransaction();

                    try
                    {
                        IDbConnection connection = uow.Connection;
                        IDbTransaction transaction = uow.Transaction;

                        foreach (PurchaseItem item in items)
                        {
                            StockBatch batch = new StockBatch
                            {
                                MedicineId = item.MedicineId,
                                BatchNumber = item.BatchNumber,
                                ExpiryDate = item.ExpiryDate,
                                PurchasePrice = item.PurchasePrice,
                                SellingPrice = item.SellingPrice,
                                CurrentQuantity = item.Quantity,
                                InitialQuantity = item.Quantity,
                                PurchaseItemId = item.Id
                            };
                            int batchId = _stockBatchRepository.Add(batch, connection, transaction);

                            StockMovement movement = new StockMovement
                            {
                                StockBatchId = batchId,
                                MedicineId = item.MedicineId,
                                MovementType = MovementType.StockIn,
                                Quantity = item.Quantity,
                                ReferenceType = "Purchase",
                                ReferenceId = purchaseId,
                                UnitPrice = item.PurchasePrice,
                                TotalAmount = item.LineTotal,
                                CreatedByUserId = purchase.CreatedByUserId
                            };
                            _stockMovementRepository.Add(movement, connection, transaction);

                            _medicineRepository.UpdateStockQuantity(item.MedicineId, item.Quantity, connection, transaction);
                        }

                        _purchaseRepository.Confirm(purchaseId, DateTime.UtcNow, totalAmount, connection, transaction);

                        uow.Commit();
                    }
                    catch
                    {
                        uow.Rollback();
                        throw;
                    }
                }

                return OperationResult.Success("Purchase confirmed successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
            catch (SqlException ex)
            {
                return OperationResult.Failure("A database error occurred while confirming the purchase: " + ex.Message);
            }
        }

        public OperationResult CancelPurchase(int purchaseId)
        {
            try
            {
                Purchase purchase = _purchaseRepository.GetById(purchaseId);
                if (purchase == null)
                    return OperationResult.Failure("Purchase not found.");

                if (purchase.Status == PurchaseStatus.Confirmed)
                    return OperationResult.Failure("Confirmed purchases cannot be cancelled. Stock has already been updated.");

                if (purchase.Status == PurchaseStatus.Cancelled)
                    return OperationResult.Failure("Purchase is already cancelled.");

                _purchaseRepository.Cancel(purchaseId);

                return OperationResult.Success("Purchase cancelled successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }
    }
}
