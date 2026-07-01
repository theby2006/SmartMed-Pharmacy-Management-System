using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Common.Exceptions;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Reports;
using SmartMed.Models.Results;

namespace SmartMed.Tests.BLL
{
    [TestClass]
    public class ReportServiceTests
    {
        private MockReportRepository _reportRepository;
        private IReportService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _reportRepository = new MockReportRepository();
            _service = new ReportService(_reportRepository);
        }

        [TestMethod]
        public void Constructor_ShouldThrow_WhenRepositoryIsNull()
        {
            Assert.ThrowsException<ValidationException>(() => new ReportService(null));
        }

        [TestMethod]
        public void GetDailySales_ShouldReturnSuccess()
        {
            _reportRepository.DailySales = new List<DailySalesSummary>
            {
                new DailySalesSummary { Date = DateTime.Today, TransactionCount = 5, TotalRevenue = 500 }
            };

            OperationResult<List<DailySalesSummary>> result = _service.GetDailySales(DateTime.Today);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual(500, result.Data[0].TotalRevenue);
        }

        [TestMethod]
        public void GetSalesByDateRange_ShouldReturnSuccess()
        {
            _reportRepository.DailySales = new List<DailySalesSummary>
            {
                new DailySalesSummary { Date = DateTime.Today, TransactionCount = 3, TotalRevenue = 300 }
            };

            OperationResult<List<DailySalesSummary>> result = _service.GetSalesByDateRange(DateTime.Today.AddDays(-7), DateTime.Today);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void GetMonthlySales_ShouldReturnSuccess()
        {
            _reportRepository.MonthlySales = new List<MonthlySalesSummary>
            {
                new MonthlySalesSummary { Year = 2024, Month = 1, TransactionCount = 50, TotalRevenue = 5000 }
            };

            OperationResult<List<MonthlySalesSummary>> result = _service.GetMonthlySales(2024);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual(5000, result.Data[0].TotalRevenue);
        }

        [TestMethod]
        public void GetSalesReport_ShouldReturnSuccess_WithoutCashierFilter()
        {
            _reportRepository.SalesReportRows = new List<SalesReportRow>
            {
                new SalesReportRow { SaleId = 1, SaleNumber = "SALE-001", GrandTotal = 100 }
            };

            OperationResult<List<SalesReportRow>> result = _service.GetSalesReport(DateTime.Today.AddDays(-30), DateTime.Today);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void GetSalesReport_ShouldReturnSuccess_WithCashierFilter()
        {
            _reportRepository.SalesReportRows = new List<SalesReportRow>
            {
                new SalesReportRow { SaleId = 1, SaleNumber = "SALE-001", GrandTotal = 100 }
            };

            OperationResult<List<SalesReportRow>> result = _service.GetSalesReport(DateTime.Today.AddDays(-30), DateTime.Today, cashierId: 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void GetCustomerSales_ShouldReturnSuccess()
        {
            _reportRepository.SalesReportRows = new List<SalesReportRow>
            {
                new SalesReportRow { SaleId = 1, SaleNumber = "SALE-001", CustomerName = "John Doe" }
            };

            OperationResult<List<SalesReportRow>> result = _service.GetCustomerSales(1, DateTime.Today.AddDays(-30), DateTime.Today);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void GetPaymentSummary_ShouldReturnSuccess()
        {
            _reportRepository.PaymentSummaries = new List<PaymentSummaryRow>
            {
                new PaymentSummaryRow { PaymentMethod = "Cash", TransactionCount = 10, TotalAmount = 1000 }
            };

            OperationResult<List<PaymentSummaryRow>> result = _service.GetPaymentSummary(DateTime.Today.AddDays(-30), DateTime.Today);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void GetPurchaseReport_ShouldReturnSuccess_WithoutSupplierFilter()
        {
            _reportRepository.PurchaseReportRows = new List<PurchaseReportRow>
            {
                new PurchaseReportRow { PurchaseId = 1, PurchaseNumber = "PO-001", TotalAmount = 500 }
            };

            OperationResult<List<PurchaseReportRow>> result = _service.GetPurchaseReport(DateTime.Today.AddDays(-30), DateTime.Today);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void GetPurchaseReport_ShouldReturnSuccess_WithSupplierFilter()
        {
            _reportRepository.PurchaseReportRows = new List<PurchaseReportRow>
            {
                new PurchaseReportRow { PurchaseId = 1, PurchaseNumber = "PO-001", TotalAmount = 500 }
            };

            OperationResult<List<PurchaseReportRow>> result = _service.GetPurchaseReport(DateTime.Today.AddDays(-30), DateTime.Today, supplierId: 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void GetCurrentStockReport_ShouldReturnSuccess()
        {
            _reportRepository.InventoryRows = new List<InventoryReportRow>
            {
                new InventoryReportRow { MedicineId = 1, MedicineName = "Amoxicillin", CurrentQuantity = 100 }
            };

            OperationResult<List<InventoryReportRow>> result = _service.GetCurrentStockReport();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void GetDashboardSummary_ShouldReturnSuccess()
        {
            _reportRepository.Dashboard = new DashboardSummary
            {
                TodaySalesRevenue = 1500,
                TotalMedicines = 200,
                LowStockCount = 5
            };

            OperationResult<DashboardSummary> result = _service.GetDashboardSummary();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1500, result.Data.TodaySalesRevenue);
            Assert.AreEqual(200, result.Data.TotalMedicines);
        }

        [TestMethod]
        public void ExportToCsv_ShouldReturnBytes_WhenDataExists()
        {
            List<DailySalesSummary> data = new List<DailySalesSummary>
            {
                new DailySalesSummary { Date = DateTime.Today, TransactionCount = 5, TotalRevenue = 500 }
            };

            OperationResult<byte[]> result = _service.ExportToCsv(data);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data.Length > 0);
        }

        [TestMethod]
        public void ExportToCsv_ShouldReturnFailure_WhenDataIsNull()
        {
            OperationResult<byte[]> result = _service.ExportToCsv<DailySalesSummary>(null);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void ExportToCsv_ShouldReturnFailure_WhenDataIsEmpty()
        {
            OperationResult<byte[]> result = _service.ExportToCsv(new List<DailySalesSummary>());

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void ExportToExcel_ShouldReturnBytes_WhenDataExists()
        {
            List<SalesReportRow> data = new List<SalesReportRow>
            {
                new SalesReportRow { SaleId = 1, SaleNumber = "SALE-001", GrandTotal = 100 }
            };

            OperationResult<byte[]> result = _service.ExportToExcel(data);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data.Length > 0);
        }

        [TestMethod]
        public void ExportToExcel_ShouldReturnFailure_WhenDataIsNull()
        {
            OperationResult<byte[]> result = _service.ExportToExcel<SalesReportRow>(null);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void GetMonthlySalesTrend_ShouldReturnSuccess()
        {
            _reportRepository.MonthlySales = new List<MonthlySalesSummary>
            {
                new MonthlySalesSummary { Year = 2024, Month = 1, TransactionCount = 100, TotalRevenue = 10000 }
            };

            OperationResult<List<MonthlySalesSummary>> result = _service.GetMonthlySalesTrend(6);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void GetProfitReport_ShouldReturnSuccess()
        {
            _reportRepository.ProfitRows = new List<ProfitReportRow>
            {
                new ProfitReportRow { SaleId = 1, SaleNumber = "SALE-001", GrandTotal = 200, Profit = 50 }
            };

            OperationResult<List<ProfitReportRow>> result = _service.GetProfitReport(DateTime.Today.AddDays(-30), DateTime.Today);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual(50, result.Data[0].Profit);
        }

        [TestMethod]
        public void GetSupplierList_ShouldReturnSuccess()
        {
            _reportRepository.SupplierRows = new List<SupplierReportRow>
            {
                new SupplierReportRow { SupplierId = 1, SupplierName = "ABC Pharma" }
            };

            OperationResult<List<SupplierReportRow>> result = _service.GetSupplierList();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public void GetTopSellingMedicines_ShouldReturnSuccess()
        {
            _reportRepository.TopSellingRows = new List<TopSellingMedicineRow>
            {
                new TopSellingMedicineRow { MedicineId = 1, MedicineName = "Amoxicillin", TotalQuantitySold = 100, Rank = 1 }
            };

            OperationResult<List<TopSellingMedicineRow>> result = _service.GetTopSellingMedicines(DateTime.Today.AddDays(-30), DateTime.Today, 5);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual(1, result.Data[0].Rank);
        }
    }
}
