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
    public class PurchaseItemRepository : IPurchaseItemRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PurchaseItemRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public List<PurchaseItem> GetByPurchaseId(int purchaseId)
        {
            const string sql = "SELECT Id, PurchaseId, MedicineId, BatchNumber, ExpiryDate, " +
                               "Quantity, PurchasePrice, SellingPrice, DiscountPercent, TaxPercent, " +
                               "LineTotal, IsActive, CreatedDate, UpdatedDate " +
                               "FROM PurchaseItems WHERE PurchaseId = @PurchaseId";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PurchaseId", purchaseId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<PurchaseItem> items = new List<PurchaseItem>();
                        while (reader.Read())
                            items.Add(MapPurchaseItem(reader));
                        return items;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve purchase items.", exception);
            }
        }

        public int Add(PurchaseItem item)
        {
            Guard.AgainstNull(item, nameof(item));
            const string sql = "INSERT INTO PurchaseItems (PurchaseId, MedicineId, BatchNumber, ExpiryDate, " +
                               "Quantity, PurchasePrice, SellingPrice, DiscountPercent, TaxPercent, LineTotal, " +
                               "IsActive, CreatedDate) " +
                               "VALUES (@PurchaseId, @MedicineId, @BatchNumber, @ExpiryDate, " +
                               "@Quantity, @PurchasePrice, @SellingPrice, @DiscountPercent, @TaxPercent, @LineTotal, " +
                               "@IsActive, @CreatedDate); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    SetPurchaseItemParameters(command, item);
                    command.Parameters.AddWithValue("@IsActive", item.IsActive);
                    command.Parameters.AddWithValue("@CreatedDate", item.CreatedDate);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add purchase item.", exception);
            }
        }

        public void AddRange(List<PurchaseItem> items)
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
                throw new DataAccessException("Failed to add purchase items.", exception);
            }
        }

        public void AddRange(List<PurchaseItem> items, IDbConnection connection, IDbTransaction transaction)
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
                throw new DataAccessException("Failed to add purchase items.", exception);
            }
        }

        private static void ExecuteAddRange(List<PurchaseItem> items, SqlConnection connection, SqlTransaction transaction)
        {
            foreach (PurchaseItem item in items)
            {
                const string sql = "INSERT INTO PurchaseItems (PurchaseId, MedicineId, BatchNumber, ExpiryDate, " +
                                   "Quantity, PurchasePrice, SellingPrice, DiscountPercent, TaxPercent, LineTotal, " +
                                   "IsActive, CreatedDate) " +
                                   "VALUES (@PurchaseId, @MedicineId, @BatchNumber, @ExpiryDate, " +
                                   "@Quantity, @PurchasePrice, @SellingPrice, @DiscountPercent, @TaxPercent, @LineTotal, " +
                                   "@IsActive, @CreatedDate); " +
                                   "SELECT CAST(SCOPE_IDENTITY() AS INT);";
                using (SqlCommand command = new SqlCommand(sql, connection, transaction))
                {
                    SetPurchaseItemParameters(command, item);
                    int id = (int)command.ExecuteScalar();
                    item.Id = id;
                }
            }
        }

        private static void SetPurchaseItemParameters(SqlCommand command, PurchaseItem item)
        {
            command.Parameters.AddWithValue("@PurchaseId", item.PurchaseId);
            command.Parameters.AddWithValue("@MedicineId", item.MedicineId);
            command.Parameters.AddWithValue("@BatchNumber", item.BatchNumber);
            command.Parameters.AddWithValue("@ExpiryDate", item.ExpiryDate);
            command.Parameters.AddWithValue("@Quantity", item.Quantity);
            command.Parameters.AddWithValue("@PurchasePrice", item.PurchasePrice);
            command.Parameters.AddWithValue("@SellingPrice", item.SellingPrice);
            command.Parameters.AddWithValue("@DiscountPercent", item.DiscountPercent);
            command.Parameters.AddWithValue("@TaxPercent", item.TaxPercent);
            command.Parameters.AddWithValue("@LineTotal", item.LineTotal);
            command.Parameters.AddWithValue("@IsActive", item.IsActive);
            command.Parameters.AddWithValue("@CreatedDate", item.CreatedDate);
        }

        public bool ExistsByMedicineAndBatch(int medicineId, string batchNumber)
        {
            Guard.AgainstNullOrWhiteSpace(batchNumber, nameof(batchNumber));
            const string sql = "SELECT COUNT(1) FROM PurchaseItems " +
                               "WHERE MedicineId = @MedicineId AND BatchNumber = @BatchNumber";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MedicineId", medicineId);
                    command.Parameters.AddWithValue("@BatchNumber", batchNumber);
                    connection.Open();
                    return (int)command.ExecuteScalar() > 0;
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to check duplicate batch.", exception);
            }
        }

        private static PurchaseItem MapPurchaseItem(SqlDataReader reader)
        {
            return new PurchaseItem
            {
                Id = (int)reader["Id"],
                PurchaseId = (int)reader["PurchaseId"],
                MedicineId = (int)reader["MedicineId"],
                BatchNumber = (string)reader["BatchNumber"],
                ExpiryDate = (DateTime)reader["ExpiryDate"],
                Quantity = (int)reader["Quantity"],
                PurchasePrice = (decimal)reader["PurchasePrice"],
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
