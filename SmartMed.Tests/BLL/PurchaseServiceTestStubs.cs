using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;

namespace SmartMed.Tests.BLL
{
    internal class MockPurchaseRepository : IPurchaseRepository
    {
        public List<Purchase> Purchases { get; set; } = new List<Purchase>();
        public bool AddCalled { get; set; }
        public bool UpdateCalled { get; set; }
        public bool ConfirmCalled { get; set; }
        public bool CancelCalled { get; set; }

        public List<Purchase> GetAll() => Purchases;

        public Purchase GetById(int id) =>
            Purchases.Find(p => p.Id == id);

        public Purchase GetByPurchaseNumber(string purchaseNumber) =>
            Purchases.Find(p =>
                p.PurchaseNumber.Equals(purchaseNumber, StringComparison.OrdinalIgnoreCase));

        public List<Purchase> Search(string keyword) =>
            Purchases.FindAll(p =>
                p.PurchaseNumber.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                (p.InvoiceNumber != null && p.InvoiceNumber.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0));

        public List<Purchase> GetBySupplier(int supplierId) =>
            Purchases.FindAll(p => p.SupplierId == supplierId);

        public List<Purchase> GetByDateRange(DateTime fromDate, DateTime toDate) =>
            Purchases.FindAll(p => p.PurchaseDate >= fromDate && p.PurchaseDate <= toDate);

        public int Add(Purchase purchase)
        {
            AddCalled = true;
            purchase.Id = Purchases.Count + 1;
            purchase.CreatedDate = DateTime.UtcNow;
            purchase.Status = PurchaseStatus.Pending;
            Purchases.Add(purchase);
            return purchase.Id;
        }

        public void Update(Purchase purchase)
        {
            UpdateCalled = true;
            int index = Purchases.FindIndex(p => p.Id == purchase.Id);
            if (index >= 0) Purchases[index] = purchase;
        }

        public void Confirm(int purchaseId, DateTime confirmedDate)
        {
            ConfirmCalled = true;
            Purchase purchase = Purchases.Find(p => p.Id == purchaseId);
            if (purchase != null)
            {
                purchase.Status = PurchaseStatus.Confirmed;
                purchase.ConfirmedDate = confirmedDate;
            }
        }

        public void Cancel(int purchaseId)
        {
            CancelCalled = true;
            Purchase purchase = Purchases.Find(p => p.Id == purchaseId);
            if (purchase != null)
            {
                purchase.Status = PurchaseStatus.Cancelled;
            }
        }

        public int Add(Purchase purchase, IDbConnection connection, IDbTransaction transaction)
        {
            return Add(purchase);
        }

        public void Confirm(int purchaseId, DateTime confirmedDate, decimal totalAmount, IDbConnection connection, IDbTransaction transaction)
        {
            Confirm(purchaseId, confirmedDate);
        }

        public void Cancel(int purchaseId, IDbConnection connection, IDbTransaction transaction)
        {
            Cancel(purchaseId);
        }
    }

    internal class MockPurchaseItemRepository : IPurchaseItemRepository
    {
        public List<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
        public bool AddCalled { get; set; }
        public bool AddRangeCalled { get; set; }

        public List<PurchaseItem> GetByPurchaseId(int purchaseId) =>
            Items.FindAll(i => i.PurchaseId == purchaseId);

        public int Add(PurchaseItem item)
        {
            AddCalled = true;
            item.Id = Items.Count + 1;
            Items.Add(item);
            return item.Id;
        }

        public void AddRange(List<PurchaseItem> items)
        {
            AddRangeCalled = true;
            foreach (PurchaseItem item in items)
            {
                item.Id = Items.Count + 1;
                Items.Add(item);
            }
        }

        public void AddRange(List<PurchaseItem> items, IDbConnection connection, IDbTransaction transaction)
        {
            AddRange(items);
        }

        public bool ExistsByMedicineAndBatch(int medicineId, string batchNumber)
        {
            return Items.Exists(i => i.MedicineId == medicineId &&
                i.BatchNumber.Equals(batchNumber, StringComparison.OrdinalIgnoreCase));
        }
    }

    internal class MockStockBatchRepository : IStockBatchRepository
    {
        public List<StockBatch> Batches { get; set; } = new List<StockBatch>();
        public bool AddCalled { get; set; }
        public bool UpdateQuantityCalled { get; set; }
        public bool DeactivateCalled { get; set; }

        public StockBatch GetById(int id) =>
            Batches.Find(b => b.Id == id);

        public List<StockBatch> GetByMedicineId(int medicineId) =>
            Batches.FindAll(b => b.MedicineId == medicineId && b.IsActive);

        public StockBatch GetBatch(int medicineId, string batchNumber) =>
            Batches.Find(b => b.MedicineId == medicineId &&
                b.BatchNumber.Equals(batchNumber, StringComparison.OrdinalIgnoreCase) && b.IsActive);

        public List<StockBatch> GetFIFOBatches(int medicineId, int quantity)
        {
            List<StockBatch> sorted = Batches.FindAll(b =>
                b.MedicineId == medicineId && b.IsActive && b.CurrentQuantity > 0);
            sorted.Sort((a, b) =>
            {
                int cmp = a.ExpiryDate.CompareTo(b.ExpiryDate);
                if (cmp == 0) cmp = a.CreatedDate.CompareTo(b.CreatedDate);
                return cmp;
            });
            int remaining = quantity;
            List<StockBatch> result = new List<StockBatch>();
            foreach (StockBatch batch in sorted)
            {
                if (remaining <= 0) break;
                result.Add(batch);
                remaining -= batch.CurrentQuantity;
            }
            return result;
        }

        public int GetAvailableStock(int medicineId)
        {
            int sum = 0;
            foreach (StockBatch b in Batches.FindAll(b => b.MedicineId == medicineId && b.IsActive))
                sum += b.CurrentQuantity;
            return sum;
        }

        public int Add(StockBatch batch)
        {
            AddCalled = true;
            batch.Id = Batches.Count + 1;
            batch.CreatedDate = DateTime.UtcNow;
            batch.IsActive = true;
            Batches.Add(batch);
            return batch.Id;
        }

        public void UpdateQuantity(int batchId, int quantity)
        {
            UpdateQuantityCalled = true;
            StockBatch batch = Batches.Find(b => b.Id == batchId);
            if (batch != null) batch.CurrentQuantity = quantity;
        }

        public void Deactivate(int batchId)
        {
            DeactivateCalled = true;
            StockBatch batch = Batches.Find(b => b.Id == batchId);
            if (batch != null) batch.IsActive = false;
        }

        public int Add(StockBatch batch, IDbConnection connection, IDbTransaction transaction)
        {
            return Add(batch);
        }

        public void UpdateQuantity(int batchId, int quantity, IDbConnection connection, IDbTransaction transaction)
        {
            UpdateQuantity(batchId, quantity);
        }
    }

    internal class MockStockMovementRepository : IStockMovementRepository
    {
        public List<StockMovement> Movements { get; set; } = new List<StockMovement>();
        public bool AddCalled { get; set; }

        public List<StockMovement> GetByStockBatchId(int stockBatchId) =>
            Movements.FindAll(m => m.StockBatchId == stockBatchId);

        public List<StockMovement> GetByMedicineId(int medicineId) =>
            Movements.FindAll(m => m.MedicineId == medicineId);

        public List<StockMovement> GetByReference(string referenceType, int referenceId) =>
            Movements.FindAll(m =>
                m.ReferenceType == referenceType && m.ReferenceId == referenceId);

        public int Add(StockMovement movement)
        {
            AddCalled = true;
            movement.Id = Movements.Count + 1;
            movement.CreatedDate = DateTime.UtcNow;
            Movements.Add(movement);
            return movement.Id;
        }

        public int Add(StockMovement movement, IDbConnection connection, IDbTransaction transaction)
        {
            return Add(movement);
        }
    }

    internal class MockUnitOfWork : IUnitOfWork
    {
        public bool BeginTransactionCalled { get; set; }
        public bool CommitCalled { get; set; }
        public bool RollbackCalled { get; set; }
        public bool IsDisposed { get; set; }

        public IDbConnection Connection { get; private set; }
        public IDbTransaction Transaction { get; private set; }

        public void BeginTransaction()
        {
            BeginTransactionCalled = true;
        }

        public void Commit()
        {
            CommitCalled = true;
        }

        public void Rollback()
        {
            RollbackCalled = true;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
