using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Models;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.Tests.BLL
{
    internal class MockOrderRepository : IOrderRepository
    {
        public List<Order> Orders { get; set; } = new List<Order>();
        public bool AddCalled { get; set; }
        public bool UpdateStatusCalled { get; set; }
        public bool UpdatePrescriptionPathCalled { get; set; }

        public Order GetById(int id) => Orders.Find(o => o.Id == id);

        public Order GetByOrderNumber(string orderNumber) =>
            Orders.Find(o => o.OrderNumber == orderNumber);

        public List<Order> GetAll() => Orders;

        public List<Order> GetByCustomerId(int customerId) =>
            Orders.FindAll(o => o.CustomerId == customerId);

        public List<Order> GetByStatus(OrderStatus status) =>
            Orders.FindAll(o => o.Status == status);

        public int Add(Order order, IDbConnection connection, IDbTransaction transaction)
        {
            AddCalled = true;
            order.Id = Orders.Count + 1;
            Orders.Add(order);
            return order.Id;
        }

        public void UpdateStatus(int orderId, OrderStatus status)
        {
            UpdateStatusCalled = true;
            Order order = Orders.Find(o => o.Id == orderId);
            if (order != null) order.Status = status;
        }

        public void UpdateStatus(int orderId, OrderStatus status, IDbConnection connection, IDbTransaction transaction)
        {
            UpdateStatus(orderId, status);
        }

        public void UpdatePrescriptionPath(int orderId, string relativePath)
        {
            UpdatePrescriptionPathCalled = true;
            Order order = Orders.Find(o => o.Id == orderId);
            if (order != null) order.PrescriptionFilePath = relativePath;
        }
    }

    internal class MockOrderItemRepository : IOrderItemRepository
    {
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public bool AddRangeCalled { get; set; }

        public List<OrderItem> GetByOrderId(int orderId) =>
            Items.FindAll(i => i.OrderId == orderId);

        public void AddRange(List<OrderItem> items, IDbConnection connection, IDbTransaction transaction)
        {
            AddRangeCalled = true;
            foreach (OrderItem item in items)
            {
                item.Id = Items.Count + 1;
                Items.Add(item);
            }
        }
    }

    internal class MockInventoryService : IInventoryService
    {
        public OperationResult<int> GetMedicineStock(int medicineId) => OperationResult<int>.Success(0);

        public OperationResult<List<StockBatch>> GetStockBatches(int medicineId) =>
            OperationResult<List<StockBatch>>.Success(new List<StockBatch>());

        public OperationResult<List<StockBatch>> GetFIFOBatches(int medicineId, int quantity) =>
            OperationResult<List<StockBatch>>.Success(new List<StockBatch>());

        public OperationResult<List<StockMovement>> GetStockMovements(int medicineId) =>
            OperationResult<List<StockMovement>>.Success(new List<StockMovement>());

        public OperationResult SyncMedicineStock() => OperationResult.Success();

        public OperationResult<List<BatchDeduction>> DeductFIFO(int medicineId, int quantity, IDbConnection connection, IDbTransaction transaction) =>
            OperationResult<List<BatchDeduction>>.Success(new List<BatchDeduction>());
    }

    internal class MockOrderNumberGenerator : IOrderNumberGenerator
    {
        public string NextNumber { get; set; } = "ORD-2026-000001";

        public OperationResult<string> GenerateNextNumber() => OperationResult<string>.Success(NextNumber);
    }
}
