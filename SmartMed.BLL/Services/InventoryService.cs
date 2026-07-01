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
    public class InventoryService : IInventoryService
    {
        private readonly IStockBatchRepository _stockBatchRepository;
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly IMedicineRepository _medicineRepository;
        private readonly IDbConnectionFactory _connectionFactory;

        public InventoryService(
            IStockBatchRepository stockBatchRepository,
            IStockMovementRepository stockMovementRepository,
            IMedicineRepository medicineRepository,
            IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(stockBatchRepository, nameof(stockBatchRepository));
            Guard.AgainstNull(stockMovementRepository, nameof(stockMovementRepository));
            Guard.AgainstNull(medicineRepository, nameof(medicineRepository));
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _stockBatchRepository = stockBatchRepository;
            _stockMovementRepository = stockMovementRepository;
            _medicineRepository = medicineRepository;
            _connectionFactory = connectionFactory;
        }

        public OperationResult<List<BatchDeduction>> DeductFIFO(int medicineId, int quantity, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                Guard.AgainstZeroOrNegative(quantity, nameof(quantity));

                List<StockBatch> fifoBatches = _stockBatchRepository.GetFIFOBatches(medicineId, quantity);

                int totalAvailable = 0;
                foreach (StockBatch b in fifoBatches)
                    totalAvailable += b.CurrentQuantity;

                if (totalAvailable < quantity)
                    return OperationResult<List<BatchDeduction>>.Failure(
                        $"Insufficient stock for medicine ID {medicineId}. Available: {totalAvailable}, Requested: {quantity}.");

                List<BatchDeduction> deductions = new List<BatchDeduction>();
                int remaining = quantity;

                foreach (StockBatch batch in fifoBatches)
                {
                    if (remaining <= 0) break;

                    int take = Math.Min(batch.CurrentQuantity, remaining);
                    int newQuantity = batch.CurrentQuantity - take;

                    _stockBatchRepository.UpdateQuantity(batch.Id, newQuantity, connection, transaction);

                    if (newQuantity == 0)
                    {
                        _stockBatchRepository.Deactivate(batch.Id);
                    }

                    deductions.Add(new BatchDeduction
                    {
                        MedicineId = medicineId,
                        StockBatchId = batch.Id,
                        BatchNumber = batch.BatchNumber,
                        ExpiryDate = batch.ExpiryDate,
                        QuantityDeducted = take,
                        SellingPrice = batch.SellingPrice
                    });

                    remaining -= take;
                }

                return OperationResult<List<BatchDeduction>>.Success(deductions);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<BatchDeduction>>.Failure(ex.Message);
            }
        }

        public OperationResult<int> GetMedicineStock(int medicineId)
        {
            try
            {
                int stock = _stockBatchRepository.GetAvailableStock(medicineId);
                return OperationResult<int>.Success(stock);
            }
            catch (ValidationException ex)
            {
                return OperationResult<int>.Failure(ex.Message);
            }
        }

        public OperationResult<List<StockBatch>> GetStockBatches(int medicineId)
        {
            try
            {
                List<StockBatch> batches = _stockBatchRepository.GetByMedicineId(medicineId);
                return OperationResult<List<StockBatch>>.Success(batches);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<StockBatch>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<StockBatch>> GetFIFOBatches(int medicineId, int quantity)
        {
            try
            {
                List<StockBatch> batches = _stockBatchRepository.GetFIFOBatches(medicineId, quantity);
                return OperationResult<List<StockBatch>>.Success(batches);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<StockBatch>>.Failure(ex.Message);
            }
        }

        public OperationResult<List<StockMovement>> GetStockMovements(int medicineId)
        {
            try
            {
                List<StockMovement> movements = _stockMovementRepository.GetByMedicineId(medicineId);
                return OperationResult<List<StockMovement>>.Success(movements);
            }
            catch (ValidationException ex)
            {
                return OperationResult<List<StockMovement>>.Failure(ex.Message);
            }
        }

        public OperationResult SyncMedicineStock()
        {
            try
            {
                List<Medicine> medicines = _medicineRepository.GetAll();

                using (SqlUnitOfWork uow = new SqlUnitOfWork(_connectionFactory))
                {
                    uow.BeginTransaction();

                    try
                    {
                        IDbConnection connection = uow.Connection;
                        IDbTransaction transaction = uow.Transaction;

                        foreach (Medicine medicine in medicines)
                        {
                            int batchStock = _stockBatchRepository.GetAvailableStock(medicine.Id);
                            _medicineRepository.SetStockQuantity(medicine.Id, batchStock, connection, transaction);
                        }

                        uow.Commit();
                    }
                    catch
                    {
                        uow.Rollback();
                        throw;
                    }
                }

                return OperationResult.Success("Medicine stock synchronized successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }
    }
}
