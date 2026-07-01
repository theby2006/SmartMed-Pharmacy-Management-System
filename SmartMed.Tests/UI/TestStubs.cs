using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Models;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Reports;
using SmartMed.Models.Results;
using SmartMed.Models.Session;

namespace SmartMed.Tests.UI
{
    internal class StubUserRepository : IUserRepository
    {
        public User GetById(int userId) => null;
        public User GetByUsername(string username) => null;
        public void IncrementFailedAttempts(int userId) { }
        public void ResetFailedAttempts(int userId) { }
        public void SetLockedUntil(int userId, DateTime? lockedUntil) { }
        public void UpdateLastLogin(int userId, DateTime loginTime) { }
        public List<User> GetAll() => new List<User>();
        public List<User> Search(string keyword) => new List<User>();
        public int Add(User user) => 1;
        public void Update(User user) { }
        public void UpdatePassword(int userId, string passwordHash, string passwordSalt) { }
        public void UpdateActiveStatus(int userId, bool isActive) { }
    }

    internal class StubPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password, string salt) => "hash";
        public bool VerifyPassword(string password, string hash, string salt) => false;
        public string GenerateSalt() => "salt";
    }

    internal class StubSessionManager : ISessionManager
    {
        public SessionContext StartSession(User user) => new SessionContext { UserId = user.Id };
        public void EndSession() { }
        public SessionContext CurrentSession => null;
        public bool IsActive => false;
        public bool HasRole(RoleType role) => false;
    }

    internal class StubAuditLogRepository : IAuditLogRepository
    {
        public void LogLogin(int userId, string username, string machineName) { }
        public void LogLogout(int? userId, string username, string machineName) { }
        public void LogFailedAttempt(string username, string machineName, string details) { }
        public void Log(int? userId, string username, AuditAction action, string machineName, string details) { }
    }

    internal class StubSupplierService : ISupplierService
    {
        public OperationResult<List<Supplier>> GetAllSuppliers() =>
            OperationResult<List<Supplier>>.Success(new List<Supplier>());
        public OperationResult<Supplier> GetSupplierById(int id) =>
            OperationResult<Supplier>.Failure("Not implemented");
        public OperationResult<List<Supplier>> SearchSuppliers(string keyword) =>
            OperationResult<List<Supplier>>.Success(new List<Supplier>());
        public OperationResult<int> AddSupplier(Supplier supplier) =>
            OperationResult<int>.Success(1);
        public OperationResult UpdateSupplier(Supplier supplier) =>
            OperationResult.Success();
        public OperationResult DeleteSupplier(int id) =>
            OperationResult.Success();
    }

    internal class StubMedicineService : IMedicineService
    {
        public OperationResult<Medicine> GetMedicineById(int id) =>
            OperationResult<Medicine>.Failure("Not implemented");
        public OperationResult<List<Medicine>> GetAllMedicines() =>
            OperationResult<List<Medicine>>.Success(new List<Medicine>());
        public OperationResult<List<Medicine>> GetMedicinesByCategory(int categoryId) =>
            OperationResult<List<Medicine>>.Success(new List<Medicine>());
        public OperationResult<List<Medicine>> SearchMedicines(string keyword) =>
            OperationResult<List<Medicine>>.Success(new List<Medicine>());
        public OperationResult<List<Medicine>> GetLowStockMedicines() =>
            OperationResult<List<Medicine>>.Success(new List<Medicine>());
        public OperationResult<List<Medicine>> GetNearExpiryMedicines() =>
            OperationResult<List<Medicine>>.Success(new List<Medicine>());
        public OperationResult<int> AddMedicine(Medicine medicine) =>
            OperationResult<int>.Success(1);
        public OperationResult UpdateMedicine(Medicine medicine) =>
            OperationResult.Success();
        public OperationResult DeleteMedicine(int id) =>
            OperationResult.Success();
    }

    internal class StubMedicineCategoryService : IMedicineCategoryService
    {
        public OperationResult<List<MedicineCategory>> GetAllCategories() =>
            OperationResult<List<MedicineCategory>>.Success(new List<MedicineCategory>());
        public OperationResult<MedicineCategory> GetCategoryById(int id) =>
            OperationResult<MedicineCategory>.Failure("Not implemented");
        public OperationResult<int> AddCategory(MedicineCategory category) =>
            OperationResult<int>.Success(1);
        public OperationResult UpdateCategory(MedicineCategory category) =>
            OperationResult.Success();
        public OperationResult DeleteCategory(int id) =>
            OperationResult.Success();
    }

    internal class StubSalesService : ISalesService
    {
        public OperationResult<List<Sale>> GetAllSales() =>
            OperationResult<List<Sale>>.Success(new List<Sale>());
        public OperationResult<Sale> GetSaleById(int id) =>
            OperationResult<Sale>.Failure("Not implemented");
        public OperationResult<Sale> GetSaleByNumber(string saleNumber) =>
            OperationResult<Sale>.Failure("Not implemented");
        public OperationResult<List<Sale>> SearchSales(string keyword) =>
            OperationResult<List<Sale>>.Success(new List<Sale>());
        public OperationResult<List<Sale>> GetSalesByDateRange(DateTime fromDate, DateTime toDate) =>
            OperationResult<List<Sale>>.Success(new List<Sale>());
        public OperationResult<List<Sale>> GetSalesByCashier(int cashierId) =>
            OperationResult<List<Sale>>.Success(new List<Sale>());
        public OperationResult<int> CreateSale(Sale sale, List<SaleItem> items, Payment payment) =>
            OperationResult<int>.Success(1);
        public OperationResult CancelSale(int saleId) =>
            OperationResult.Success();
        public OperationResult<List<SaleItem>> GetSaleItems(int saleId) =>
            OperationResult<List<SaleItem>>.Success(new List<SaleItem>());
    }

    internal class StubPaymentService : IPaymentService
    {
        public OperationResult<Payment> GetPaymentBySaleId(int saleId) =>
            OperationResult<Payment>.Failure("Not implemented");
        public OperationResult<int> ProcessPayment(Payment payment) =>
            OperationResult<int>.Success(1);
    }

    internal class StubSaleNumberGenerator : ISaleNumberGenerator
    {
        private int _counter = 0;
        public OperationResult<string> GenerateNextNumber()
        {
            _counter++;
            return OperationResult<string>.Success($"SAL-{DateTime.UtcNow.Year}-{_counter:D6}");
        }
    }

    internal class StubPricingService : IPricingService
    {
        public decimal CalculateSubTotal(int quantity, decimal unitPrice) => quantity * unitPrice;

        public decimal CalculateLineTotal(int quantity, decimal sellingPrice, decimal discountPercent, decimal taxPercent)
        {
            decimal subTotal = quantity * sellingPrice;
            decimal discount = subTotal * (discountPercent / 100m);
            decimal afterDiscount = subTotal - discount;
            decimal tax = afterDiscount * (taxPercent / 100m);
            return afterDiscount + tax;
        }

        public decimal CalculateDiscountAmount(decimal subTotal, decimal discountPercent) =>
            subTotal * (discountPercent / 100m);

        public decimal CalculateTaxAmount(decimal amountAfterDiscount, decimal taxPercent) =>
            amountAfterDiscount * (taxPercent / 100m);

        public decimal CalculateGrandTotal(decimal subTotal, decimal discountAmount, decimal taxAmount) =>
            subTotal - discountAmount + taxAmount;

        public decimal CalculateChange(decimal amountPaid, decimal grandTotal) =>
            amountPaid - grandTotal;
    }

    internal class StubInventoryService : IInventoryService
    {
        public OperationResult<int> GetMedicineStock(int medicineId) =>
            OperationResult<int>.Success(100);
        public OperationResult<List<StockBatch>> GetStockBatches(int medicineId) =>
            OperationResult<List<StockBatch>>.Success(new List<StockBatch>
            {
                new StockBatch { Id = 1, BatchNumber = "BATCH001", ExpiryDate = DateTime.Now.AddMonths(6), CurrentQuantity = 50, SellingPrice = 10.00m }
            });
        public OperationResult<List<StockBatch>> GetFIFOBatches(int medicineId, int quantity) =>
            OperationResult<List<StockBatch>>.Success(new List<StockBatch>());
        public OperationResult<List<StockMovement>> GetStockMovements(int medicineId) =>
            OperationResult<List<StockMovement>>.Success(new List<StockMovement>());
        public OperationResult SyncMedicineStock() =>
            OperationResult.Success();
        public OperationResult<List<BatchDeduction>> DeductFIFO(int medicineId, int quantity, IDbConnection connection, IDbTransaction transaction) =>
            OperationResult<List<BatchDeduction>>.Success(new List<BatchDeduction>());
    }

    internal class StubReportService : IReportService
    {
        public OperationResult<List<DailySalesSummary>> GetDailySales(DateTime date) =>
            OperationResult<List<DailySalesSummary>>.Success(new List<DailySalesSummary>());
        public OperationResult<List<DailySalesSummary>> GetSalesByDateRange(DateTime from, DateTime to) =>
            OperationResult<List<DailySalesSummary>>.Success(new List<DailySalesSummary>());
        public OperationResult<List<MonthlySalesSummary>> GetMonthlySales(int year) =>
            OperationResult<List<MonthlySalesSummary>>.Success(new List<MonthlySalesSummary>());
        public OperationResult<List<SalesReportRow>> GetSalesReport(DateTime from, DateTime to, int? cashierId = null) =>
            OperationResult<List<SalesReportRow>>.Success(new List<SalesReportRow>());
        public OperationResult<List<SalesReportRow>> GetCustomerSales(int? customerId, DateTime from, DateTime to) =>
            OperationResult<List<SalesReportRow>>.Success(new List<SalesReportRow>());
        public OperationResult<List<PaymentSummaryRow>> GetPaymentSummary(DateTime from, DateTime to) =>
            OperationResult<List<PaymentSummaryRow>>.Success(new List<PaymentSummaryRow>());
        public OperationResult<List<PurchaseReportRow>> GetPurchaseReport(DateTime from, DateTime to, int? supplierId = null) =>
            OperationResult<List<PurchaseReportRow>>.Success(new List<PurchaseReportRow>());
        public OperationResult<List<PurchaseReportRow>> GetDailyPurchases(DateTime date) =>
            OperationResult<List<PurchaseReportRow>>.Success(new List<PurchaseReportRow>());
        public OperationResult<List<InventoryReportRow>> GetCurrentStockReport() =>
            OperationResult<List<InventoryReportRow>>.Success(new List<InventoryReportRow>());
        public OperationResult<List<InventoryReportRow>> GetLowStockReport(int threshold) =>
            OperationResult<List<InventoryReportRow>>.Success(new List<InventoryReportRow>());
        public OperationResult<List<InventoryReportRow>> GetNearExpiryReport(int days) =>
            OperationResult<List<InventoryReportRow>>.Success(new List<InventoryReportRow>());
        public OperationResult<List<InventoryReportRow>> GetExpiredMedicinesReport() =>
            OperationResult<List<InventoryReportRow>>.Success(new List<InventoryReportRow>());
        public OperationResult<List<StockMovementReportRow>> GetStockMovementReport(DateTime from, DateTime to) =>
            OperationResult<List<StockMovementReportRow>>.Success(new List<StockMovementReportRow>());
        public OperationResult<List<BatchReportRow>> GetBatchReport(int? medicineId = null) =>
            OperationResult<List<BatchReportRow>>.Success(new List<BatchReportRow>());
        public OperationResult<List<MedicineReportRow>> GetMedicineList(int? categoryId = null) =>
            OperationResult<List<MedicineReportRow>>.Success(new List<MedicineReportRow>());
        public OperationResult<List<CategorySummaryRow>> GetCategorySummary() =>
            OperationResult<List<CategorySummaryRow>>.Success(new List<CategorySummaryRow>());
        public OperationResult<List<TopSellingMedicineRow>> GetTopSellingMedicines(DateTime from, DateTime to, int topN = 10) =>
            OperationResult<List<TopSellingMedicineRow>>.Success(new List<TopSellingMedicineRow>());
        public OperationResult<List<SlowMovingMedicineRow>> GetSlowMovingMedicines(int thresholdDays) =>
            OperationResult<List<SlowMovingMedicineRow>>.Success(new List<SlowMovingMedicineRow>());
        public OperationResult<List<SupplierReportRow>> GetSupplierList() =>
            OperationResult<List<SupplierReportRow>>.Success(new List<SupplierReportRow>());
        public OperationResult<List<ProfitReportRow>> GetProfitReport(DateTime from, DateTime to) =>
            OperationResult<List<ProfitReportRow>>.Success(new List<ProfitReportRow>());
        public OperationResult<List<SalesReportRow>> GetRevenueReport(DateTime from, DateTime to) =>
            OperationResult<List<SalesReportRow>>.Success(new List<SalesReportRow>());
        public OperationResult<List<PurchaseReportRow>> GetPurchaseCostReport(DateTime from, DateTime to) =>
            OperationResult<List<PurchaseReportRow>>.Success(new List<PurchaseReportRow>());
        public OperationResult<DashboardSummary> GetDashboardSummary() =>
            OperationResult<DashboardSummary>.Success(new DashboardSummary());
        public OperationResult<List<MonthlySalesSummary>> GetMonthlySalesTrend(int months = 12) =>
            OperationResult<List<MonthlySalesSummary>>.Success(new List<MonthlySalesSummary>());
        public OperationResult<byte[]> ExportToCsv<T>(List<T> data) =>
            OperationResult<byte[]>.Success(new byte[0]);
        public OperationResult<byte[]> ExportToExcel<T>(List<T> data) =>
            OperationResult<byte[]>.Success(new byte[0]);
    }

    internal class StubSessionManagerForSales : ISessionManager
    {
        public SessionContext CurrentSession => new SessionContext
        {
            UserId = 1,
            DisplayName = "Test Cashier",
            Username = "cashier",
            Role = RoleType.Cashier
        };
        public bool IsActive => true;
        public SessionContext StartSession(User user) => CurrentSession;
        public void EndSession() { }
        public bool HasRole(RoleType role) => role == RoleType.Cashier;
    }
}
