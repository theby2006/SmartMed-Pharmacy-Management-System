using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;

namespace SmartMed.DAL.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SaleRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public Sale GetById(int id)
        {
            const string sql = "SELECT Id, SaleNumber, SaleDate, CashierId, CustomerId, CustomerName, CustomerPhone, " +
                               "DiscountPercent, TaxPercent, SubTotal, DiscountAmount, TaxAmount, GrandTotal, " +
                               "Status, Notes, IsActive, CreatedDate, UpdatedDate " +
                               "FROM Sales WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapSale(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve sale by Id.", exception);
            }
            return null;
        }

        public Sale GetBySaleNumber(string saleNumber)
        {
            Guard.AgainstNullOrWhiteSpace(saleNumber, nameof(saleNumber));
            const string sql = "SELECT Id, SaleNumber, SaleDate, CashierId, CustomerId, CustomerName, CustomerPhone, " +
                               "DiscountPercent, TaxPercent, SubTotal, DiscountAmount, TaxAmount, GrandTotal, " +
                               "Status, Notes, IsActive, CreatedDate, UpdatedDate " +
                               "FROM Sales WHERE SaleNumber = @SaleNumber";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SaleNumber", saleNumber);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapSale(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve sale by number.", exception);
            }
            return null;
        }

        public List<Sale> GetAll()
        {
            const string sql = "SELECT Id, SaleNumber, SaleDate, CashierId, CustomerId, CustomerName, CustomerPhone, " +
                               "DiscountPercent, TaxPercent, SubTotal, DiscountAmount, TaxAmount, GrandTotal, " +
                               "Status, Notes, IsActive, CreatedDate, UpdatedDate " +
                               "FROM Sales ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Sale> sales = new List<Sale>();
                        while (reader.Read())
                            sales.Add(MapSale(reader));
                        return sales;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve sales.", exception);
            }
        }

        public List<Sale> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            const string sql = "SELECT Id, SaleNumber, SaleDate, CashierId, CustomerId, CustomerName, CustomerPhone, " +
                               "DiscountPercent, TaxPercent, SubTotal, DiscountAmount, TaxAmount, GrandTotal, " +
                               "Status, Notes, IsActive, CreatedDate, UpdatedDate " +
                               "FROM Sales WHERE SaleDate >= @FromDate AND SaleDate <= @ToDate " +
                               "ORDER BY SaleDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@FromDate", fromDate);
                    command.Parameters.AddWithValue("@ToDate", toDate);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Sale> sales = new List<Sale>();
                        while (reader.Read())
                            sales.Add(MapSale(reader));
                        return sales;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve sales by date range.", exception);
            }
        }

        public List<Sale> GetByCashier(int cashierId)
        {
            const string sql = "SELECT Id, SaleNumber, SaleDate, CashierId, CustomerName, CustomerPhone, " +
                               "DiscountPercent, TaxPercent, SubTotal, DiscountAmount, TaxAmount, GrandTotal, " +
                               "Status, Notes, IsActive, CreatedDate, UpdatedDate " +
                               "FROM Sales WHERE CashierId = @CashierId ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CashierId", cashierId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Sale> sales = new List<Sale>();
                        while (reader.Read())
                            sales.Add(MapSale(reader));
                        return sales;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve sales by cashier.", exception);
            }
        }

        public List<Sale> GetByStatus(SaleStatus status)
        {
            const string sql = "SELECT Id, SaleNumber, SaleDate, CashierId, CustomerName, CustomerPhone, " +
                               "DiscountPercent, TaxPercent, SubTotal, DiscountAmount, TaxAmount, GrandTotal, " +
                               "Status, Notes, IsActive, CreatedDate, UpdatedDate " +
                               "FROM Sales WHERE Status = @Status ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Status", (int)status);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Sale> sales = new List<Sale>();
                        while (reader.Read())
                            sales.Add(MapSale(reader));
                        return sales;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve sales by status.", exception);
            }
        }

        public List<Sale> Search(string keyword)
        {
            Guard.AgainstNullOrWhiteSpace(keyword, nameof(keyword));
            const string sql = "SELECT Id, SaleNumber, SaleDate, CashierId, CustomerName, CustomerPhone, " +
                               "DiscountPercent, TaxPercent, SubTotal, DiscountAmount, TaxAmount, GrandTotal, " +
                               "Status, Notes, IsActive, CreatedDate, UpdatedDate " +
                               "FROM Sales WHERE SaleNumber LIKE @Keyword OR " +
                               "CustomerName LIKE @Keyword OR " +
                               "CustomerPhone LIKE @Keyword " +
                               "ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Sale> sales = new List<Sale>();
                        while (reader.Read())
                            sales.Add(MapSale(reader));
                        return sales;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to search sales.", exception);
            }
        }

        public int Add(Sale sale)
        {
            Guard.AgainstNull(sale, nameof(sale));
            const string sql = "INSERT INTO Sales (SaleNumber, SaleDate, CashierId, CustomerName, CustomerPhone, " +
                               "DiscountPercent, TaxPercent, SubTotal, DiscountAmount, TaxAmount, GrandTotal, " +
                               "Status, Notes, IsActive, CreatedDate) " +
                               "VALUES (@SaleNumber, @SaleDate, @CashierId, @CustomerName, @CustomerPhone, " +
                               "@DiscountPercent, @TaxPercent, @SubTotal, @DiscountAmount, @TaxAmount, @GrandTotal, " +
                               "@Status, @Notes, @IsActive, @CreatedDate); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    SetSaleInsertParameters(command, sale);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add sale.", exception);
            }
        }

        public int Add(Sale sale, IDbConnection connection, IDbTransaction transaction)
        {
            Guard.AgainstNull(sale, nameof(sale));
            const string sql = "INSERT INTO Sales (SaleNumber, SaleDate, CashierId, CustomerName, CustomerPhone, " +
                               "DiscountPercent, TaxPercent, SubTotal, DiscountAmount, TaxAmount, GrandTotal, " +
                               "Status, Notes, IsActive, CreatedDate) " +
                               "VALUES (@SaleNumber, @SaleDate, @CashierId, @CustomerName, @CustomerPhone, " +
                               "@DiscountPercent, @TaxPercent, @SubTotal, @DiscountAmount, @TaxAmount, @GrandTotal, " +
                               "@Status, @Notes, @IsActive, @CreatedDate); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    SetSaleInsertParameters(command, sale);
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add sale.", exception);
            }
        }

        public void UpdateStatus(int saleId, SaleStatus status)
        {
            const string sql = "UPDATE Sales SET Status = @Status, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", saleId);
                    command.Parameters.AddWithValue("@Status", (int)status);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update sale status.", exception);
            }
        }

        public void UpdateStatus(int saleId, SaleStatus status, IDbConnection connection, IDbTransaction transaction)
        {
            const string sql = "UPDATE Sales SET Status = @Status, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    command.Parameters.AddWithValue("@Id", saleId);
                    command.Parameters.AddWithValue("@Status", (int)status);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update sale status.", exception);
            }
        }

        private static void SetSaleInsertParameters(SqlCommand command, Sale sale)
        {
            command.Parameters.AddWithValue("@SaleNumber", sale.SaleNumber);
            command.Parameters.AddWithValue("@SaleDate", sale.SaleDate);
            command.Parameters.AddWithValue("@CashierId", sale.CashierId);
            command.Parameters.AddWithValue("@CustomerName", (object)sale.CustomerName ?? DBNull.Value);
            command.Parameters.AddWithValue("@CustomerPhone", (object)sale.CustomerPhone ?? DBNull.Value);
            command.Parameters.AddWithValue("@DiscountPercent", sale.DiscountPercent);
            command.Parameters.AddWithValue("@TaxPercent", sale.TaxPercent);
            command.Parameters.AddWithValue("@SubTotal", sale.SubTotal);
            command.Parameters.AddWithValue("@DiscountAmount", sale.DiscountAmount);
            command.Parameters.AddWithValue("@TaxAmount", sale.TaxAmount);
            command.Parameters.AddWithValue("@GrandTotal", sale.GrandTotal);
            command.Parameters.AddWithValue("@Status", (int)sale.Status);
            command.Parameters.AddWithValue("@Notes", (object)sale.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsActive", sale.IsActive);
            command.Parameters.AddWithValue("@CreatedDate", sale.CreatedDate);
        }

        private static Sale MapSale(SqlDataReader reader)
        {
            return new Sale
            {
                Id = (int)reader["Id"],
                SaleNumber = (string)reader["SaleNumber"],
                SaleDate = (DateTime)reader["SaleDate"],
                CashierId = (int)reader["CashierId"],
                CustomerName = reader["CustomerName"] == DBNull.Value ? null : (string)reader["CustomerName"],
                CustomerPhone = reader["CustomerPhone"] == DBNull.Value ? null : (string)reader["CustomerPhone"],
                DiscountPercent = (decimal)reader["DiscountPercent"],
                TaxPercent = (decimal)reader["TaxPercent"],
                SubTotal = (decimal)reader["SubTotal"],
                DiscountAmount = (decimal)reader["DiscountAmount"],
                TaxAmount = (decimal)reader["TaxAmount"],
                GrandTotal = (decimal)reader["GrandTotal"],
                Status = (SaleStatus)(int)reader["Status"],
                Notes = reader["Notes"] == DBNull.Value ? null : (string)reader["Notes"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };
        }
    }
}
