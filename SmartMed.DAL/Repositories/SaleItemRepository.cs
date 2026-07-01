using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Repositories
{
    public class SaleItemRepository : ISaleItemRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SaleItemRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public List<SaleItem> GetBySaleId(int saleId)
        {
            const string sql = "SELECT Id, SaleId, MedicineId, StockBatchId, BatchNumber, ExpiryDate, " +
                               "Quantity, SellingPrice, DiscountPercent, TaxPercent, LineTotal, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM SaleItems WHERE SaleId = @SaleId";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SaleId", saleId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<SaleItem> items = new List<SaleItem>();
                        while (reader.Read())
                            items.Add(MapSaleItem(reader));
                        return items;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve sale items.", exception);
            }
        }

        public int Add(SaleItem item)
        {
            Guard.AgainstNull(item, nameof(item));
            const string sql = "INSERT INTO SaleItems (SaleId, MedicineId, StockBatchId, BatchNumber, ExpiryDate, " +
                               "Quantity, SellingPrice, DiscountPercent, TaxPercent, LineTotal, " +
                               "IsActive, CreatedDate) " +
                               "VALUES (@SaleId, @MedicineId, @StockBatchId, @BatchNumber, @ExpiryDate, " +
                               "@Quantity, @SellingPrice, @DiscountPercent, @TaxPercent, @LineTotal, " +
                               "@IsActive, @CreatedDate); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    SetSaleItemParameters(command, item);
                    command.Parameters.AddWithValue("@IsActive", item.IsActive);
                    command.Parameters.AddWithValue("@CreatedDate", item.CreatedDate);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add sale item.", exception);
            }
        }

        public int Add(SaleItem item, IDbConnection connection, IDbTransaction transaction)
        {
            Guard.AgainstNull(item, nameof(item));
            const string sql = "INSERT INTO SaleItems (SaleId, MedicineId, StockBatchId, BatchNumber, ExpiryDate, " +
                               "Quantity, SellingPrice, DiscountPercent, TaxPercent, LineTotal, " +
                               "IsActive, CreatedDate) " +
                               "VALUES (@SaleId, @MedicineId, @StockBatchId, @BatchNumber, @ExpiryDate, " +
                               "@Quantity, @SellingPrice, @DiscountPercent, @TaxPercent, @LineTotal, " +
                               "@IsActive, @CreatedDate); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    SetSaleItemParameters(command, item);
                    command.Parameters.AddWithValue("@IsActive", item.IsActive);
                    command.Parameters.AddWithValue("@CreatedDate", item.CreatedDate);
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add sale item.", exception);
            }
        }

        public void AddRange(List<SaleItem> items)
        {
            Guard.AgainstNull(items, nameof(items));

            if (items.Count == 0)
                return;

            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            ExecuteAddRange(items, connection, transaction);
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add sale items.", exception);
            }
        }

        public void AddRange(List<SaleItem> items, IDbConnection connection, IDbTransaction transaction)
        {
            Guard.AgainstNull(items, nameof(items));

            if (items.Count == 0)
                return;

            try
            {
                ExecuteAddRange(items, (SqlConnection)connection, (SqlTransaction)transaction);
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add sale items.", exception);
            }
        }

        private static void ExecuteAddRange(List<SaleItem> items, SqlConnection connection, SqlTransaction transaction)
        {
            foreach (SaleItem item in items)
            {
                const string sql = "INSERT INTO SaleItems (SaleId, MedicineId, StockBatchId, BatchNumber, ExpiryDate, " +
                                   "Quantity, SellingPrice, DiscountPercent, TaxPercent, LineTotal, " +
                                   "IsActive, CreatedDate) " +
                                   "VALUES (@SaleId, @MedicineId, @StockBatchId, @BatchNumber, @ExpiryDate, " +
                                   "@Quantity, @SellingPrice, @DiscountPercent, @TaxPercent, @LineTotal, " +
                                   "@IsActive, @CreatedDate); " +
                                   "SELECT CAST(SCOPE_IDENTITY() AS INT);";
                using (SqlCommand command = new SqlCommand(sql, connection, transaction))
                {
                    SetSaleItemParameters(command, item);
                    int id = (int)command.ExecuteScalar();
                    item.Id = id;
                }
            }
        }

        private static void SetSaleItemParameters(SqlCommand command, SaleItem item)
        {
            command.Parameters.AddWithValue("@SaleId", item.SaleId);
            command.Parameters.AddWithValue("@MedicineId", item.MedicineId);
            command.Parameters.AddWithValue("@StockBatchId", item.StockBatchId);
            command.Parameters.AddWithValue("@BatchNumber", item.BatchNumber);
            command.Parameters.AddWithValue("@ExpiryDate", item.ExpiryDate);
            command.Parameters.AddWithValue("@Quantity", item.Quantity);
            command.Parameters.AddWithValue("@SellingPrice", item.SellingPrice);
            command.Parameters.AddWithValue("@DiscountPercent", item.DiscountPercent);
            command.Parameters.AddWithValue("@TaxPercent", item.TaxPercent);
            command.Parameters.AddWithValue("@LineTotal", item.LineTotal);
        }

        private static SaleItem MapSaleItem(SqlDataReader reader)
        {
            return new SaleItem
            {
                Id = (int)reader["Id"],
                SaleId = (int)reader["SaleId"],
                MedicineId = (int)reader["MedicineId"],
                StockBatchId = (int)reader["StockBatchId"],
                BatchNumber = (string)reader["BatchNumber"],
                ExpiryDate = (DateTime)reader["ExpiryDate"],
                Quantity = (int)reader["Quantity"],
                SellingPrice = (decimal)reader["SellingPrice"],
                DiscountPercent = (decimal)reader["DiscountPercent"],
                TaxPercent = (decimal)reader["TaxPercent"],
                LineTotal = (decimal)reader["LineTotal"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };
        }
    }
}
