using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Enums;
using SmartMed.Models.Reports;

namespace SmartMed.DAL.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ReportRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public List<DailySalesSummary> GetDailySales(DateTime date)
        {
            const string sql = "SELECT CAST(SaleDate AS DATE) AS SaleDate, COUNT(*) AS TransactionCount, " +
                               "ISNULL(SUM(GrandTotal), 0) AS TotalRevenue, " +
                               "ISNULL(SUM(DiscountAmount), 0) AS TotalDiscount, " +
                               "ISNULL(SUM(TaxAmount), 0) AS TotalTax, " +
                               "ISNULL(AVG(GrandTotal), 0) AS AverageTransactionValue " +
                               "FROM Sales " +
                               "WHERE CAST(SaleDate AS DATE) = @Date AND Status = @Status " +
                               "GROUP BY CAST(SaleDate AS DATE)";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Date", date.Date);
                    command.Parameters.AddWithValue("@Status", (int)SaleStatus.Completed);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<DailySalesSummary> results = new List<DailySalesSummary>();
                        while (reader.Read())
                            results.Add(MapDailySalesSummary(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve daily sales.", exception);
            }
        }

        public List<DailySalesSummary> GetSalesByDateRange(DateTime from, DateTime to)
        {
            const string sql = "SELECT CAST(SaleDate AS DATE) AS SaleDate, COUNT(*) AS TransactionCount, " +
                               "ISNULL(SUM(GrandTotal), 0) AS TotalRevenue, " +
                               "ISNULL(SUM(DiscountAmount), 0) AS TotalDiscount, " +
                               "ISNULL(SUM(TaxAmount), 0) AS TotalTax, " +
                               "ISNULL(AVG(GrandTotal), 0) AS AverageTransactionValue " +
                               "FROM Sales " +
                               "WHERE CAST(SaleDate AS DATE) >= @From AND CAST(SaleDate AS DATE) <= @To AND Status = @Status " +
                               "GROUP BY CAST(SaleDate AS DATE) ORDER BY SaleDate";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@From", from.Date);
                    command.Parameters.AddWithValue("@To", to.Date);
                    command.Parameters.AddWithValue("@Status", (int)SaleStatus.Completed);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<DailySalesSummary> results = new List<DailySalesSummary>();
                        while (reader.Read())
                            results.Add(MapDailySalesSummary(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve sales by date range.", exception);
            }
        }

        public List<MonthlySalesSummary> GetMonthlySales(int year)
        {
            const string sql = "SELECT YEAR(SaleDate) AS Year, MONTH(SaleDate) AS Month, " +
                               "COUNT(*) AS TransactionCount, ISNULL(SUM(GrandTotal), 0) AS TotalRevenue " +
                               "FROM Sales " +
                               "WHERE YEAR(SaleDate) = @Year AND Status = @Status " +
                               "GROUP BY YEAR(SaleDate), MONTH(SaleDate) ORDER BY Month";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Year", year);
                    command.Parameters.AddWithValue("@Status", (int)SaleStatus.Completed);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<MonthlySalesSummary> results = new List<MonthlySalesSummary>();
                        while (reader.Read())
                            results.Add(MapMonthlySalesSummary(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve monthly sales.", exception);
            }
        }

        public List<SalesReportRow> GetSalesByCashier(int cashierId, DateTime from, DateTime to)
        {
            const string sql = "SELECT s.Id AS SaleId, s.SaleNumber, s.SaleDate, " +
                               "u.DisplayName AS CashierName, " +
                               "ISNULL(s.CustomerName, '') AS CustomerName, " +
                               "(SELECT COUNT(*) FROM SaleItems WHERE SaleId = s.Id) AS ItemCount, " +
                               "s.SubTotal, s.DiscountAmount, s.TaxAmount, s.GrandTotal, " +
                               "CASE WHEN p.PaymentMethod = 1 THEN 'Cash' WHEN p.PaymentMethod = 2 THEN 'Card' " +
                               "WHEN p.PaymentMethod = 3 THEN 'QR' WHEN p.PaymentMethod = 4 THEN 'Online' ELSE 'N/A' END AS PaymentMethod, " +
                               "s.Status " +
                               "FROM Sales s " +
                               "INNER JOIN Users u ON s.CashierId = u.Id " +
                               "LEFT JOIN Payments p ON p.SaleId = s.Id " +
                               "WHERE s.CashierId = @CashierId AND s.SaleDate >= @From AND s.SaleDate <= @To " +
                               "ORDER BY s.SaleDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CashierId", cashierId);
                    command.Parameters.AddWithValue("@From", from);
                    command.Parameters.AddWithValue("@To", to);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<SalesReportRow> results = new List<SalesReportRow>();
                        while (reader.Read())
                            results.Add(MapSalesReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve sales by cashier.", exception);
            }
        }

        public List<SalesReportRow> GetCustomerSales(int? customerId, DateTime from, DateTime to)
        {
            const string sql = "SELECT s.Id AS SaleId, s.SaleNumber, s.SaleDate, " +
                               "u.DisplayName AS CashierName, " +
                               "ISNULL(s.CustomerName, '') AS CustomerName, " +
                               "(SELECT COUNT(*) FROM SaleItems WHERE SaleId = s.Id) AS ItemCount, " +
                               "s.SubTotal, s.DiscountAmount, s.TaxAmount, s.GrandTotal, " +
                               "CASE WHEN p.PaymentMethod = 1 THEN 'Cash' WHEN p.PaymentMethod = 2 THEN 'Card' " +
                               "WHEN p.PaymentMethod = 3 THEN 'QR' WHEN p.PaymentMethod = 4 THEN 'Online' ELSE 'N/A' END AS PaymentMethod, " +
                               "s.Status " +
                               "FROM Sales s " +
                               "INNER JOIN Users u ON s.CashierId = u.Id " +
                               "LEFT JOIN Payments p ON p.SaleId = s.Id " +
                               "WHERE (@CustomerId IS NULL OR s.CustomerId = @CustomerId) " +
                               "AND s.SaleDate >= @From AND s.SaleDate <= @To " +
                               "ORDER BY s.SaleDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", (object)customerId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@From", from);
                    command.Parameters.AddWithValue("@To", to);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<SalesReportRow> results = new List<SalesReportRow>();
                        while (reader.Read())
                            results.Add(MapSalesReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve customer sales.", exception);
            }
        }

        public List<PaymentSummaryRow> GetPaymentSummary(DateTime from, DateTime to)
        {
            const string sql = "SELECT CASE WHEN p.PaymentMethod = 1 THEN 'Cash' WHEN p.PaymentMethod = 2 THEN 'Card' " +
                               "WHEN p.PaymentMethod = 3 THEN 'QR' WHEN p.PaymentMethod = 4 THEN 'Online' ELSE 'N/A' END AS PaymentMethod, " +
                               "COUNT(*) AS TransactionCount, ISNULL(SUM(p.AmountPaid), 0) AS TotalAmount " +
                               "FROM Payments p INNER JOIN Sales s ON p.SaleId = s.Id " +
                               "WHERE s.SaleDate >= @From AND s.SaleDate <= @To AND s.Status = @Status " +
                               "GROUP BY p.PaymentMethod ORDER BY TotalAmount DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@From", from);
                    command.Parameters.AddWithValue("@To", to);
                    command.Parameters.AddWithValue("@Status", (int)SaleStatus.Completed);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<PaymentSummaryRow> results = new List<PaymentSummaryRow>();
                        decimal grandTotal = 0;
                        while (reader.Read())
                        {
                            PaymentSummaryRow row = MapPaymentSummaryRow(reader);
                            results.Add(row);
                            grandTotal += row.TotalAmount;
                        }
                        if (grandTotal > 0)
                        {
                            foreach (PaymentSummaryRow r in results)
                                r.PercentageOfTotal = Math.Round(r.TotalAmount / grandTotal * 100, 2);
                        }
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve payment summary.", exception);
            }
        }

        public List<PurchaseReportRow> GetDailyPurchases(DateTime date)
        {
            const string sql = "SELECT p.Id AS PurchaseId, p.PurchaseNumber, p.PurchaseDate, " +
                               "s.SupplierName, ISNULL(p.InvoiceNumber, '') AS InvoiceNumber, " +
                               "(SELECT COUNT(*) FROM PurchaseItems WHERE PurchaseId = p.Id) AS ItemCount, " +
                               "p.TotalAmount, p.Status, '' AS CreatedBy " +
                               "FROM Purchases p INNER JOIN Suppliers s ON p.SupplierId = s.Id " +
                               "WHERE CAST(p.PurchaseDate AS DATE) = @Date " +
                               "ORDER BY p.PurchaseDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Date", date.Date);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<PurchaseReportRow> results = new List<PurchaseReportRow>();
                        while (reader.Read())
                            results.Add(MapPurchaseReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve daily purchases.", exception);
            }
        }

        public List<PurchaseReportRow> GetPurchasesBySupplier(int supplierId, DateTime from, DateTime to)
        {
            const string sql = "SELECT p.Id AS PurchaseId, p.PurchaseNumber, p.PurchaseDate, " +
                               "s.SupplierName, ISNULL(p.InvoiceNumber, '') AS InvoiceNumber, " +
                               "(SELECT COUNT(*) FROM PurchaseItems WHERE PurchaseId = p.Id) AS ItemCount, " +
                               "p.TotalAmount, p.Status, '' AS CreatedBy " +
                               "FROM Purchases p INNER JOIN Suppliers s ON p.SupplierId = s.Id " +
                               "WHERE p.SupplierId = @SupplierId AND p.PurchaseDate >= @From AND p.PurchaseDate <= @To " +
                               "ORDER BY p.PurchaseDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SupplierId", supplierId);
                    command.Parameters.AddWithValue("@From", from);
                    command.Parameters.AddWithValue("@To", to);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<PurchaseReportRow> results = new List<PurchaseReportRow>();
                        while (reader.Read())
                            results.Add(MapPurchaseReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve purchases by supplier.", exception);
            }
        }

        public List<PurchaseReportRow> GetPurchaseHistory(DateTime from, DateTime to)
        {
            const string sql = "SELECT p.Id AS PurchaseId, p.PurchaseNumber, p.PurchaseDate, " +
                               "s.SupplierName, ISNULL(p.InvoiceNumber, '') AS InvoiceNumber, " +
                               "(SELECT COUNT(*) FROM PurchaseItems WHERE PurchaseId = p.Id) AS ItemCount, " +
                               "p.TotalAmount, p.Status, '' AS CreatedBy " +
                               "FROM Purchases p INNER JOIN Suppliers s ON p.SupplierId = s.Id " +
                               "WHERE p.PurchaseDate >= @From AND p.PurchaseDate <= @To " +
                               "ORDER BY p.PurchaseDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@From", from);
                    command.Parameters.AddWithValue("@To", to);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<PurchaseReportRow> results = new List<PurchaseReportRow>();
                        while (reader.Read())
                            results.Add(MapPurchaseReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve purchase history.", exception);
            }
        }

        public List<InventoryReportRow> GetCurrentStock()
        {
            const string sql = "SELECT m.Id AS MedicineId, m.Name AS MedicineName, " +
                               "ISNULL(c.Name, 'Uncategorized') AS CategoryName, " +
                               "ISNULL(sb.BatchNumber, 'N/A') AS BatchNumber, " +
                               "ISNULL(sb.ExpiryDate, GETUTCDATE()) AS ExpiryDate, " +
                               "ISNULL(sb.CurrentQuantity, 0) AS CurrentQuantity, " +
                               "m.ReorderLevel, ISNULL(sb.PurchasePrice, 0) AS PurchasePrice, " +
                               "ISNULL(sb.SellingPrice, m.UnitPrice) AS SellingPrice, " +
                               "DATEDIFF(DAY, GETUTCDATE(), ISNULL(sb.ExpiryDate, GETUTCDATE())) AS DaysUntilExpiry " +
                               "FROM Medicines m " +
                               "LEFT JOIN MedicineCategories c ON m.CategoryId = c.Id " +
                               "LEFT JOIN StockBatches sb ON sb.MedicineId = m.Id AND sb.IsActive = 1 " +
                               "WHERE m.IsActive = 1 " +
                               "ORDER BY m.Name, sb.ExpiryDate";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<InventoryReportRow> results = new List<InventoryReportRow>();
                        while (reader.Read())
                            results.Add(MapInventoryReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve current stock.", exception);
            }
        }

        public List<InventoryReportRow> GetLowStock(int threshold)
        {
            const string sql = "SELECT m.Id AS MedicineId, m.Name AS MedicineName, " +
                               "ISNULL(c.Name, 'Uncategorized') AS CategoryName, " +
                               "ISNULL(sb.BatchNumber, 'N/A') AS BatchNumber, " +
                               "ISNULL(sb.ExpiryDate, GETUTCDATE()) AS ExpiryDate, " +
                               "ISNULL(sb.CurrentQuantity, 0) AS CurrentQuantity, " +
                               "m.ReorderLevel, ISNULL(sb.PurchasePrice, 0) AS PurchasePrice, " +
                               "ISNULL(sb.SellingPrice, m.UnitPrice) AS SellingPrice, " +
                               "DATEDIFF(DAY, GETUTCDATE(), ISNULL(sb.ExpiryDate, GETUTCDATE())) AS DaysUntilExpiry " +
                               "FROM Medicines m " +
                               "LEFT JOIN MedicineCategories c ON m.CategoryId = c.Id " +
                               "LEFT JOIN StockBatches sb ON sb.MedicineId = m.Id AND sb.IsActive = 1 " +
                               "WHERE m.IsActive = 1 AND (ISNULL(sb.CurrentQuantity, 0) <= @Threshold OR m.StockQuantity <= m.ReorderLevel) " +
                               "ORDER BY ISNULL(sb.CurrentQuantity, 0) ASC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Threshold", threshold);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<InventoryReportRow> results = new List<InventoryReportRow>();
                        while (reader.Read())
                            results.Add(MapInventoryReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve low stock items.", exception);
            }
        }

        public List<InventoryReportRow> GetNearExpiry(int days)
        {
            const string sql = "SELECT m.Id AS MedicineId, m.Name AS MedicineName, " +
                               "ISNULL(c.Name, 'Uncategorized') AS CategoryName, " +
                               "sb.BatchNumber, sb.ExpiryDate, sb.CurrentQuantity, " +
                               "m.ReorderLevel, sb.PurchasePrice, sb.SellingPrice, " +
                               "DATEDIFF(DAY, GETUTCDATE(), sb.ExpiryDate) AS DaysUntilExpiry " +
                               "FROM StockBatches sb " +
                               "INNER JOIN Medicines m ON sb.MedicineId = m.Id " +
                               "LEFT JOIN MedicineCategories c ON m.CategoryId = c.Id " +
                               "WHERE sb.IsActive = 1 AND sb.CurrentQuantity > 0 " +
                               "AND sb.ExpiryDate >= GETUTCDATE() " +
                               "AND sb.ExpiryDate <= DATEADD(DAY, @Days, GETUTCDATE()) " +
                               "ORDER BY sb.ExpiryDate ASC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Days", days);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<InventoryReportRow> results = new List<InventoryReportRow>();
                        while (reader.Read())
                            results.Add(MapInventoryReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve near expiry items.", exception);
            }
        }

        public List<InventoryReportRow> GetExpiredMedicines()
        {
            const string sql = "SELECT m.Id AS MedicineId, m.Name AS MedicineName, " +
                               "ISNULL(c.Name, 'Uncategorized') AS CategoryName, " +
                               "sb.BatchNumber, sb.ExpiryDate, sb.CurrentQuantity, " +
                               "m.ReorderLevel, sb.PurchasePrice, sb.SellingPrice, " +
                               "DATEDIFF(DAY, GETUTCDATE(), sb.ExpiryDate) AS DaysUntilExpiry " +
                               "FROM StockBatches sb " +
                               "INNER JOIN Medicines m ON sb.MedicineId = m.Id " +
                               "LEFT JOIN MedicineCategories c ON m.CategoryId = c.Id " +
                               "WHERE sb.IsActive = 1 AND sb.ExpiryDate < GETUTCDATE() " +
                               "ORDER BY sb.ExpiryDate ASC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<InventoryReportRow> results = new List<InventoryReportRow>();
                        while (reader.Read())
                            results.Add(MapInventoryReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve expired medicines.", exception);
            }
        }

        public List<StockMovementReportRow> GetStockMovements(DateTime from, DateTime to)
        {
            const string sql = "SELECT sm.CreatedDate AS MovementDate, m.Name AS MedicineName, " +
                               "ISNULL(sb.BatchNumber, '') AS BatchNumber, " +
                               "sm.MovementType, sm.Quantity, " +
                               "ISNULL(sm.ReferenceType, '') AS ReferenceType, " +
                               "sm.ReferenceId, sm.UnitPrice, sm.TotalAmount " +
                               "FROM StockMovements sm " +
                               "INNER JOIN Medicines m ON sm.MedicineId = m.Id " +
                               "LEFT JOIN StockBatches sb ON sm.StockBatchId = sb.Id " +
                               "WHERE sm.CreatedDate >= @From AND sm.CreatedDate <= @To " +
                               "ORDER BY sm.CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@From", from);
                    command.Parameters.AddWithValue("@To", to);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<StockMovementReportRow> results = new List<StockMovementReportRow>();
                        while (reader.Read())
                            results.Add(MapStockMovementReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve stock movements.", exception);
            }
        }

        public List<BatchReportRow> GetBatchReport(int? medicineId)
        {
            const string sql = "SELECT sb.Id AS BatchId, sb.BatchNumber, m.Name AS MedicineName, " +
                               "sb.ExpiryDate, sb.InitialQuantity, sb.CurrentQuantity, " +
                               "sb.PurchasePrice, sb.SellingPrice, sb.Status AS BatchStatus " +
                               "FROM StockBatches sb " +
                               "INNER JOIN Medicines m ON sb.MedicineId = m.Id " +
                               "WHERE (@MedicineId IS NULL OR sb.MedicineId = @MedicineId) " +
                               "ORDER BY m.Name, sb.ExpiryDate";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MedicineId", (object)medicineId ?? DBNull.Value);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<BatchReportRow> results = new List<BatchReportRow>();
                        while (reader.Read())
                            results.Add(MapBatchReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve batch report.", exception);
            }
        }

        public List<MedicineReportRow> GetMedicineList(int? categoryId)
        {
            const string sql = "SELECT m.Id AS MedicineId, m.Name AS MedicineName, " +
                               "ISNULL(c.Name, 'Uncategorized') AS CategoryName, " +
                               "ISNULL(m.Brand, '') AS Brand, " +
                               "m.StockQuantity, m.ReorderLevel, m.UnitPrice, m.IsActive " +
                               "FROM Medicines m " +
                               "LEFT JOIN MedicineCategories c ON m.CategoryId = c.Id " +
                               "WHERE (@CategoryId IS NULL OR m.CategoryId = @CategoryId) " +
                               "ORDER BY m.Name";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CategoryId", (object)categoryId ?? DBNull.Value);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<MedicineReportRow> results = new List<MedicineReportRow>();
                        while (reader.Read())
                            results.Add(MapMedicineReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve medicine list.", exception);
            }
        }

        public List<CategorySummaryRow> GetCategorySummary()
        {
            const string sql = "SELECT c.Id AS CategoryId, c.Name AS CategoryName, " +
                               "COUNT(m.Id) AS MedicineCount, " +
                               "ISNULL(SUM(m.StockQuantity), 0) AS TotalStockQuantity, " +
                               "ISNULL(SUM(m.StockQuantity * m.UnitPrice), 0) AS TotalStockValue " +
                               "FROM MedicineCategories c " +
                               "LEFT JOIN Medicines m ON m.CategoryId = c.Id AND m.IsActive = 1 " +
                               "GROUP BY c.Id, c.Name ORDER BY c.Name";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<CategorySummaryRow> results = new List<CategorySummaryRow>();
                        while (reader.Read())
                            results.Add(MapCategorySummaryRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve category summary.", exception);
            }
        }

        public List<TopSellingMedicineRow> GetTopSellingMedicines(DateTime from, DateTime to, int topN)
        {
            const string sql = "SELECT TOP (@TopN) m.Id AS MedicineId, m.Name AS MedicineName, " +
                               "ISNULL(c.Name, 'Uncategorized') AS CategoryName, " +
                               "ISNULL(SUM(si.Quantity), 0) AS TotalQuantitySold, " +
                               "ISNULL(SUM(si.LineTotal), 0) AS TotalRevenue, " +
                               "ROW_NUMBER() OVER (ORDER BY ISNULL(SUM(si.Quantity), 0) DESC) AS Rank " +
                               "FROM SaleItems si " +
                               "INNER JOIN Sales s ON si.SaleId = s.Id " +
                               "INNER JOIN Medicines m ON si.MedicineId = m.Id " +
                               "LEFT JOIN MedicineCategories c ON m.CategoryId = c.Id " +
                               "WHERE s.SaleDate >= @From AND s.SaleDate <= @To AND s.Status = @Status " +
                               "GROUP BY m.Id, m.Name, c.Name " +
                               "ORDER BY TotalQuantitySold DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@TopN", topN);
                    command.Parameters.AddWithValue("@From", from);
                    command.Parameters.AddWithValue("@To", to);
                    command.Parameters.AddWithValue("@Status", (int)SaleStatus.Completed);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<TopSellingMedicineRow> results = new List<TopSellingMedicineRow>();
                        while (reader.Read())
                            results.Add(MapTopSellingMedicineRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve top selling medicines.", exception);
            }
        }

        public List<SlowMovingMedicineRow> GetSlowMovingMedicines(int thresholdDays)
        {
            const string sql = "SELECT m.Id AS MedicineId, m.Name AS MedicineName, " +
                               "ISNULL(c.Name, 'Uncategorized') AS CategoryName, " +
                               "m.StockQuantity, " +
                               "DATEDIFF(DAY, ISNULL(MAX(s.SaleDate), '1900-01-01'), GETUTCDATE()) AS DaysSinceLastSale " +
                               "FROM Medicines m " +
                               "LEFT JOIN MedicineCategories c ON m.CategoryId = c.Id " +
                               "LEFT JOIN SaleItems si ON si.MedicineId = m.Id " +
                               "LEFT JOIN Sales s ON si.SaleId = s.Id AND s.Status = @Status " +
                               "WHERE m.IsActive = 1 " +
                               "GROUP BY m.Id, m.Name, c.Name, m.StockQuantity " +
                               "HAVING DATEDIFF(DAY, ISNULL(MAX(s.SaleDate), '1900-01-01'), GETUTCDATE()) >= @ThresholdDays " +
                               "ORDER BY DaysSinceLastSale DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ThresholdDays", thresholdDays);
                    command.Parameters.AddWithValue("@Status", (int)SaleStatus.Completed);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<SlowMovingMedicineRow> results = new List<SlowMovingMedicineRow>();
                        while (reader.Read())
                            results.Add(MapSlowMovingMedicineRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve slow moving medicines.", exception);
            }
        }

        public List<SupplierReportRow> GetSupplierList()
        {
            const string sql = "SELECT s.Id AS SupplierId, s.SupplierName, " +
                               "ISNULL(s.ContactPerson, '') AS ContactPerson, " +
                               "ISNULL(s.PhoneNumber, '') AS Phone, " +
                               "ISNULL(s.Email, '') AS Email, " +
                               "ISNULL(SUM(CASE WHEN p.Status = 2 THEN p.TotalAmount ELSE 0 END), 0) AS TotalPurchases, " +
                               "MAX(CASE WHEN p.Status = 2 THEN p.PurchaseDate ELSE NULL END) AS LastPurchaseDate, " +
                               "COUNT(CASE WHEN p.Status = 2 THEN 1 ELSE NULL END) AS PurchaseCount " +
                               "FROM Suppliers s " +
                               "LEFT JOIN Purchases p ON p.SupplierId = s.Id " +
                               "WHERE s.IsActive = 1 " +
                               "GROUP BY s.Id, s.SupplierName, s.ContactPerson, s.PhoneNumber, s.Email " +
                               "ORDER BY s.SupplierName";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<SupplierReportRow> results = new List<SupplierReportRow>();
                        while (reader.Read())
                            results.Add(MapSupplierReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve supplier list.", exception);
            }
        }

        public List<ProfitReportRow> GetProfitReport(DateTime from, DateTime to)
        {
            const string sql = "SELECT s.Id AS SaleId, s.SaleNumber, s.SaleDate, s.GrandTotal, " +
                               "ISNULL(SUM(sb.PurchasePrice * si.Quantity), 0) AS TotalCostOfGoodsSold, " +
                               "(s.GrandTotal - ISNULL(SUM(sb.PurchasePrice * si.Quantity), 0)) AS Profit, " +
                               "CASE WHEN s.GrandTotal > 0 " +
                               "THEN ((s.GrandTotal - ISNULL(SUM(sb.PurchasePrice * si.Quantity), 0)) / s.GrandTotal) * 100 " +
                               "ELSE 0 END AS ProfitMargin " +
                               "FROM Sales s " +
                               "INNER JOIN SaleItems si ON si.SaleId = s.Id " +
                               "LEFT JOIN StockBatches sb ON si.StockBatchId = sb.Id " +
                               "WHERE s.SaleDate >= @From AND s.SaleDate <= @To AND s.Status = @Status " +
                               "GROUP BY s.Id, s.SaleNumber, s.SaleDate, s.GrandTotal " +
                               "ORDER BY s.SaleDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@From", from);
                    command.Parameters.AddWithValue("@To", to);
                    command.Parameters.AddWithValue("@Status", (int)SaleStatus.Completed);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<ProfitReportRow> results = new List<ProfitReportRow>();
                        while (reader.Read())
                            results.Add(MapProfitReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve profit report.", exception);
            }
        }

        public List<SalesReportRow> GetRevenueReport(DateTime from, DateTime to)
        {
            const string sql = "SELECT s.Id AS SaleId, s.SaleNumber, s.SaleDate, " +
                               "u.DisplayName AS CashierName, " +
                               "ISNULL(s.CustomerName, '') AS CustomerName, " +
                               "(SELECT COUNT(*) FROM SaleItems WHERE SaleId = s.Id) AS ItemCount, " +
                               "s.SubTotal, s.DiscountAmount, s.TaxAmount, s.GrandTotal, " +
                               "CASE WHEN p.PaymentMethod = 1 THEN 'Cash' WHEN p.PaymentMethod = 2 THEN 'Card' " +
                               "WHEN p.PaymentMethod = 3 THEN 'QR' WHEN p.PaymentMethod = 4 THEN 'Online' ELSE 'N/A' END AS PaymentMethod, " +
                               "s.Status " +
                               "FROM Sales s " +
                               "INNER JOIN Users u ON s.CashierId = u.Id " +
                               "LEFT JOIN Payments p ON p.SaleId = s.Id " +
                               "WHERE s.SaleDate >= @From AND s.SaleDate <= @To AND s.Status = @Status " +
                               "ORDER BY s.SaleDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@From", from);
                    command.Parameters.AddWithValue("@To", to);
                    command.Parameters.AddWithValue("@Status", (int)SaleStatus.Completed);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<SalesReportRow> results = new List<SalesReportRow>();
                        while (reader.Read())
                            results.Add(MapSalesReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve revenue report.", exception);
            }
        }

        public List<PurchaseReportRow> GetPurchaseCostReport(DateTime from, DateTime to)
        {
            const string sql = "SELECT p.Id AS PurchaseId, p.PurchaseNumber, p.PurchaseDate, " +
                               "s.SupplierName, ISNULL(p.InvoiceNumber, '') AS InvoiceNumber, " +
                               "(SELECT COUNT(*) FROM PurchaseItems WHERE PurchaseId = p.Id) AS ItemCount, " +
                               "p.TotalAmount, p.Status, '' AS CreatedBy " +
                               "FROM Purchases p INNER JOIN Suppliers s ON p.SupplierId = s.Id " +
                               "WHERE p.PurchaseDate >= @From AND p.PurchaseDate <= @To AND p.Status = @Status " +
                               "ORDER BY p.PurchaseDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@From", from);
                    command.Parameters.AddWithValue("@To", to);
                    command.Parameters.AddWithValue("@Status", (int)PurchaseStatus.Confirmed);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<PurchaseReportRow> results = new List<PurchaseReportRow>();
                        while (reader.Read())
                            results.Add(MapPurchaseReportRow(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve purchase cost report.", exception);
            }
        }

        public DashboardSummary GetDashboardSummary()
        {
            const string sql = "SELECT " +
                               "(SELECT COUNT(*) FROM Sales WHERE CAST(SaleDate AS DATE) = CAST(GETDATE() AS DATE) AND Status = 2) AS TodaySalesCount, " +
                               "(SELECT ISNULL(SUM(GrandTotal), 0) FROM Sales WHERE CAST(SaleDate AS DATE) = CAST(GETDATE() AS DATE) AND Status = 2) AS TodaySalesRevenue, " +
                               "(SELECT ISNULL(SUM(s.GrandTotal - ISNULL(sb.PurchasePrice * si.Quantity, 0)), 0) " +
                               " FROM Sales s INNER JOIN SaleItems si ON si.SaleId = s.Id LEFT JOIN StockBatches sb ON si.StockBatchId = sb.Id " +
                               " WHERE CAST(s.SaleDate AS DATE) = CAST(GETDATE() AS DATE) AND s.Status = 2) AS TodayProfit, " +
                               "(SELECT COUNT(*) FROM Medicines WHERE IsActive = 1) AS TotalMedicines, " +
                               "(SELECT COUNT(*) FROM Medicines WHERE IsActive = 1 AND StockQuantity <= ReorderLevel) AS LowStockCount, " +
                               "(SELECT COUNT(*) FROM StockBatches WHERE IsActive = 1 AND ExpiryDate < GETUTCDATE()) AS ExpiredCount, " +
                               "(SELECT COUNT(*) FROM StockBatches WHERE IsActive = 1 AND CurrentQuantity > 0 " +
                               " AND ExpiryDate >= GETUTCDATE() AND ExpiryDate <= DATEADD(DAY, 30, GETUTCDATE())) AS NearExpiryCount, " +
                               "(SELECT ISNULL(SUM(GrandTotal), 0) FROM Sales WHERE YEAR(SaleDate) = YEAR(GETDATE()) " +
                               " AND MONTH(SaleDate) = MONTH(GETDATE()) AND Status = 2) AS TotalSalesMonth, " +
                               "(SELECT ISNULL(SUM(TotalAmount), 0) FROM Purchases WHERE YEAR(PurchaseDate) = YEAR(GETDATE()) " +
                               " AND MONTH(PurchaseDate) = MONTH(GETDATE()) AND Status = 2) AS TotalPurchasesMonth, " +
                               "(SELECT COUNT(*) FROM Suppliers WHERE IsActive = 1) AS TotalActiveSuppliers";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapDashboardSummary(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve dashboard summary.", exception);
            }
            return new DashboardSummary();
        }

        public List<MonthlySalesSummary> GetMonthlySalesTrend(int months)
        {
            const string sql = "SELECT YEAR(SaleDate) AS Year, MONTH(SaleDate) AS Month, " +
                               "COUNT(*) AS TransactionCount, ISNULL(SUM(GrandTotal), 0) AS TotalRevenue " +
                               "FROM Sales " +
                               "WHERE SaleDate >= DATEADD(MONTH, -@Months, GETUTCDATE()) AND Status = @Status " +
                               "GROUP BY YEAR(SaleDate), MONTH(SaleDate) " +
                               "ORDER BY Year, Month";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Months", months);
                    command.Parameters.AddWithValue("@Status", (int)SaleStatus.Completed);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<MonthlySalesSummary> results = new List<MonthlySalesSummary>();
                        while (reader.Read())
                            results.Add(MapMonthlySalesSummary(reader));
                        return results;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve monthly sales trend.", exception);
            }
        }

        private static DailySalesSummary MapDailySalesSummary(SqlDataReader reader)
        {
            return new DailySalesSummary
            {
                Date = (DateTime)reader["SaleDate"],
                TransactionCount = (int)reader["TransactionCount"],
                TotalRevenue = (decimal)reader["TotalRevenue"],
                TotalDiscount = (decimal)reader["TotalDiscount"],
                TotalTax = (decimal)reader["TotalTax"],
                AverageTransactionValue = (decimal)reader["AverageTransactionValue"]
            };
        }

        private static MonthlySalesSummary MapMonthlySalesSummary(SqlDataReader reader)
        {
            return new MonthlySalesSummary
            {
                Year = (int)reader["Year"],
                Month = (int)reader["Month"],
                TransactionCount = (int)reader["TransactionCount"],
                TotalRevenue = (decimal)reader["TotalRevenue"],
                TotalCost = 0,
                TotalProfit = 0
            };
        }

        private static SalesReportRow MapSalesReportRow(SqlDataReader reader)
        {
            return new SalesReportRow
            {
                SaleId = (int)reader["SaleId"],
                SaleNumber = (string)reader["SaleNumber"],
                SaleDate = (DateTime)reader["SaleDate"],
                CashierName = (string)reader["CashierName"],
                CustomerName = reader["CustomerName"] == DBNull.Value ? "" : (string)reader["CustomerName"],
                ItemCount = (int)reader["ItemCount"],
                SubTotal = (decimal)reader["SubTotal"],
                DiscountAmount = (decimal)reader["DiscountAmount"],
                TaxAmount = (decimal)reader["TaxAmount"],
                GrandTotal = (decimal)reader["GrandTotal"],
                PaymentMethod = (string)reader["PaymentMethod"],
                Status = (SaleStatus)(int)reader["Status"]
            };
        }

        private static PaymentSummaryRow MapPaymentSummaryRow(SqlDataReader reader)
        {
            return new PaymentSummaryRow
            {
                PaymentMethod = (string)reader["PaymentMethod"],
                TransactionCount = (int)reader["TransactionCount"],
                TotalAmount = (decimal)reader["TotalAmount"]
            };
        }

        private static PurchaseReportRow MapPurchaseReportRow(SqlDataReader reader)
        {
            return new PurchaseReportRow
            {
                PurchaseId = (int)reader["PurchaseId"],
                PurchaseNumber = (string)reader["PurchaseNumber"],
                PurchaseDate = (DateTime)reader["PurchaseDate"],
                SupplierName = (string)reader["SupplierName"],
                InvoiceNumber = reader["InvoiceNumber"] == DBNull.Value ? "" : (string)reader["InvoiceNumber"],
                ItemCount = (int)reader["ItemCount"],
                TotalAmount = (decimal)reader["TotalAmount"],
                Status = (PurchaseStatus)(int)reader["Status"],
                CreatedBy = reader["CreatedBy"] == DBNull.Value ? "" : (string)reader["CreatedBy"]
            };
        }

        private static InventoryReportRow MapInventoryReportRow(SqlDataReader reader)
        {
            return new InventoryReportRow
            {
                MedicineId = (int)reader["MedicineId"],
                MedicineName = (string)reader["MedicineName"],
                CategoryName = reader["CategoryName"] == DBNull.Value ? "Uncategorized" : (string)reader["CategoryName"],
                BatchNumber = reader["BatchNumber"] == DBNull.Value ? "N/A" : (string)reader["BatchNumber"],
                ExpiryDate = (DateTime)reader["ExpiryDate"],
                CurrentQuantity = (int)reader["CurrentQuantity"],
                ReorderLevel = (int)reader["ReorderLevel"],
                PurchasePrice = (decimal)reader["PurchasePrice"],
                SellingPrice = (decimal)reader["SellingPrice"],
                DaysUntilExpiry = (int)reader["DaysUntilExpiry"]
            };
        }

        private static StockMovementReportRow MapStockMovementReportRow(SqlDataReader reader)
        {
            return new StockMovementReportRow
            {
                MovementDate = (DateTime)reader["MovementDate"],
                MedicineName = (string)reader["MedicineName"],
                BatchNumber = reader["BatchNumber"] == DBNull.Value ? "" : (string)reader["BatchNumber"],
                MovementType = (MovementType)(int)reader["MovementType"],
                Quantity = (int)reader["Quantity"],
                ReferenceType = reader["ReferenceType"] == DBNull.Value ? "" : (string)reader["ReferenceType"],
                ReferenceId = (int)reader["ReferenceId"],
                UnitPrice = (decimal)reader["UnitPrice"],
                TotalAmount = (decimal)reader["TotalAmount"]
            };
        }

        private static BatchReportRow MapBatchReportRow(SqlDataReader reader)
        {
            return new BatchReportRow
            {
                BatchId = (int)reader["BatchId"],
                BatchNumber = (string)reader["BatchNumber"],
                MedicineName = (string)reader["MedicineName"],
                ExpiryDate = (DateTime)reader["ExpiryDate"],
                InitialQuantity = (int)reader["InitialQuantity"],
                CurrentQuantity = (int)reader["CurrentQuantity"],
                PurchasePrice = (decimal)reader["PurchasePrice"],
                SellingPrice = (decimal)reader["SellingPrice"],
                BatchStatus = (BatchStatus)(int)reader["BatchStatus"]
            };
        }

        private static MedicineReportRow MapMedicineReportRow(SqlDataReader reader)
        {
            return new MedicineReportRow
            {
                MedicineId = (int)reader["MedicineId"],
                MedicineName = (string)reader["MedicineName"],
                CategoryName = reader["CategoryName"] == DBNull.Value ? "Uncategorized" : (string)reader["CategoryName"],
                Brand = reader["Brand"] == DBNull.Value ? "" : (string)reader["Brand"],
                StockQuantity = (int)reader["StockQuantity"],
                ReorderLevel = (int)reader["ReorderLevel"],
                UnitPrice = (decimal)reader["UnitPrice"],
                IsActive = (bool)reader["IsActive"]
            };
        }

        private static CategorySummaryRow MapCategorySummaryRow(SqlDataReader reader)
        {
            return new CategorySummaryRow
            {
                CategoryId = (int)reader["CategoryId"],
                CategoryName = (string)reader["CategoryName"],
                MedicineCount = (int)reader["MedicineCount"],
                TotalStockQuantity = (int)reader["TotalStockQuantity"],
                TotalStockValue = (decimal)reader["TotalStockValue"]
            };
        }

        private static TopSellingMedicineRow MapTopSellingMedicineRow(SqlDataReader reader)
        {
            return new TopSellingMedicineRow
            {
                MedicineId = (int)reader["MedicineId"],
                MedicineName = (string)reader["MedicineName"],
                CategoryName = reader["CategoryName"] == DBNull.Value ? "Uncategorized" : (string)reader["CategoryName"],
                TotalQuantitySold = (int)reader["TotalQuantitySold"],
                TotalRevenue = (decimal)reader["TotalRevenue"],
                Rank = (int)reader["Rank"]
            };
        }

        private static SlowMovingMedicineRow MapSlowMovingMedicineRow(SqlDataReader reader)
        {
            return new SlowMovingMedicineRow
            {
                MedicineId = (int)reader["MedicineId"],
                MedicineName = (string)reader["MedicineName"],
                CategoryName = reader["CategoryName"] == DBNull.Value ? "Uncategorized" : (string)reader["CategoryName"],
                StockQuantity = (int)reader["StockQuantity"],
                DaysSinceLastSale = (int)reader["DaysSinceLastSale"]
            };
        }

        private static SupplierReportRow MapSupplierReportRow(SqlDataReader reader)
        {
            return new SupplierReportRow
            {
                SupplierId = (int)reader["SupplierId"],
                SupplierName = (string)reader["SupplierName"],
                ContactPerson = reader["ContactPerson"] == DBNull.Value ? "" : (string)reader["ContactPerson"],
                Phone = reader["Phone"] == DBNull.Value ? "" : (string)reader["Phone"],
                Email = reader["Email"] == DBNull.Value ? "" : (string)reader["Email"],
                TotalPurchases = (decimal)reader["TotalPurchases"],
                LastPurchaseDate = reader["LastPurchaseDate"] == DBNull.Value ? null : (DateTime?)reader["LastPurchaseDate"],
                PurchaseCount = (int)reader["PurchaseCount"]
            };
        }

        private static ProfitReportRow MapProfitReportRow(SqlDataReader reader)
        {
            return new ProfitReportRow
            {
                SaleId = (int)reader["SaleId"],
                SaleNumber = (string)reader["SaleNumber"],
                SaleDate = (DateTime)reader["SaleDate"],
                GrandTotal = (decimal)reader["GrandTotal"],
                TotalCostOfGoodsSold = (decimal)reader["TotalCostOfGoodsSold"],
                Profit = (decimal)reader["Profit"],
                ProfitMargin = (decimal)reader["ProfitMargin"]
            };
        }

        private static DashboardSummary MapDashboardSummary(SqlDataReader reader)
        {
            return new DashboardSummary
            {
                TodaySalesCount = (int)reader["TodaySalesCount"],
                TodaySalesRevenue = (decimal)reader["TodaySalesRevenue"],
                TodayProfit = (decimal)reader["TodayProfit"],
                TotalMedicines = (int)reader["TotalMedicines"],
                LowStockCount = (int)reader["LowStockCount"],
                ExpiredCount = (int)reader["ExpiredCount"],
                NearExpiryCount = (int)reader["NearExpiryCount"],
                TotalSalesMonth = (decimal)reader["TotalSalesMonth"],
                TotalPurchasesMonth = (decimal)reader["TotalPurchasesMonth"],
                TotalActiveSuppliers = (int)reader["TotalActiveSuppliers"]
            };
        }
    }
}
