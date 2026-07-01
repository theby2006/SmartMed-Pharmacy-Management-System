using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Common.Exceptions;
using SmartMed.DAL.Infrastructure;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;

namespace SmartMed.Tests.BLL
{
    [TestClass]
    public class InventoryServiceTests
    {
        private MockStockBatchRepository _stockBatchRepository;
        private MockStockMovementRepository _stockMovementRepository;
        private MockMedicineRepository _medicineRepository;
        private IDbConnectionFactory _connectionFactory;
        private IInventoryService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _stockBatchRepository = new MockStockBatchRepository();
            _stockMovementRepository = new MockStockMovementRepository();
            _medicineRepository = new MockMedicineRepository();
            _connectionFactory = new SqlConnectionFactory(AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName));
            _service = new InventoryService(_stockBatchRepository, _stockMovementRepository,
                _medicineRepository, _connectionFactory);
        }

        [TestMethod]
        public void GetMedicineStock_ShouldReturnSuccess()
        {
            _stockBatchRepository.Batches = new List<StockBatch>
            {
                new StockBatch { Id = 1, MedicineId = 1, BatchNumber = "BATCH-001", CurrentQuantity = 100, IsActive = true },
                new StockBatch { Id = 2, MedicineId = 1, BatchNumber = "BATCH-002", CurrentQuantity = 50, IsActive = true }
            };

            OperationResult<int> result = _service.GetMedicineStock(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(150, result.Data);
        }

        [TestMethod]
        public void GetMedicineStock_ShouldReturnZero_WhenNoStock()
        {
            OperationResult<int> result = _service.GetMedicineStock(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Data);
        }

        [TestMethod]
        public void GetStockBatches_ShouldReturnSuccess()
        {
            _stockBatchRepository.Batches = new List<StockBatch>
            {
                new StockBatch { Id = 1, MedicineId = 1, BatchNumber = "BATCH-001", CurrentQuantity = 100, IsActive = true },
                new StockBatch { Id = 2, MedicineId = 1, BatchNumber = "BATCH-002", CurrentQuantity = 50, IsActive = true }
            };

            OperationResult<List<StockBatch>> result = _service.GetStockBatches(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, result.Data.Count);
        }

        [TestMethod]
        public void GetStockBatches_ShouldReturnEmptyList_WhenNoBatches()
        {
            OperationResult<List<StockBatch>> result = _service.GetStockBatches(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Data.Count);
        }

        [TestMethod]
        public void GetFIFOBatches_ShouldReturnBatchesOrderedByExpiryDate()
        {
            DateTime laterDate = DateTime.UtcNow.AddYears(2);
            DateTime earlierDate = DateTime.UtcNow.AddYears(1);
            _stockBatchRepository.Batches = new List<StockBatch>
            {
                new StockBatch { Id = 1, MedicineId = 1, BatchNumber = "BATCH-002", ExpiryDate = laterDate, CurrentQuantity = 100, IsActive = true, CreatedDate = DateTime.UtcNow },
                new StockBatch { Id = 2, MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = earlierDate, CurrentQuantity = 50, IsActive = true, CreatedDate = DateTime.UtcNow }
            };

            OperationResult<List<StockBatch>> result = _service.GetFIFOBatches(1, 150);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, result.Data.Count);
            Assert.AreEqual("BATCH-001", result.Data[0].BatchNumber);
            Assert.AreEqual("BATCH-002", result.Data[1].BatchNumber);
        }

        [TestMethod]
        public void GetFIFOBatches_ShouldReturnOnlyEnoughBatches_ForQuantity()
        {
            _stockBatchRepository.Batches = new List<StockBatch>
            {
                new StockBatch { Id = 1, MedicineId = 1, BatchNumber = "BATCH-001", ExpiryDate = DateTime.UtcNow.AddYears(1), CurrentQuantity = 50, IsActive = true, CreatedDate = DateTime.UtcNow },
                new StockBatch { Id = 2, MedicineId = 1, BatchNumber = "BATCH-002", ExpiryDate = DateTime.UtcNow.AddYears(2), CurrentQuantity = 100, IsActive = true, CreatedDate = DateTime.UtcNow }
            };

            OperationResult<List<StockBatch>> result = _service.GetFIFOBatches(1, 30);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual("BATCH-001", result.Data[0].BatchNumber);
        }

        [TestMethod]
        public void GetStockMovements_ShouldReturnSuccess()
        {
            _stockMovementRepository.Movements = new List<StockMovement>
            {
                new StockMovement { Id = 1, MedicineId = 1, MovementType = MovementType.StockIn, Quantity = 100 },
                new StockMovement { Id = 2, MedicineId = 1, MovementType = MovementType.StockIn, Quantity = 50 }
            };

            OperationResult<List<StockMovement>> result = _service.GetStockMovements(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, result.Data.Count);
        }

        [TestMethod]
        public void GetStockMovements_ShouldReturnEmptyList_WhenNoMovements()
        {
            OperationResult<List<StockMovement>> result = _service.GetStockMovements(1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Data.Count);
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenStockBatchRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new InventoryService(null, _stockMovementRepository, _medicineRepository, _connectionFactory));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenStockMovementRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new InventoryService(_stockBatchRepository, null, _medicineRepository, _connectionFactory));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenMedicineRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new InventoryService(_stockBatchRepository, _stockMovementRepository, null, _connectionFactory));
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenConnectionFactoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() =>
                new InventoryService(_stockBatchRepository, _stockMovementRepository, _medicineRepository, null));
        }
    }
}
