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
    public class StockBatchRepository : IStockBatchRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public StockBatchRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public StockBatch GetById(int id)
        {
            const string sql = "SELECT Id, MedicineId, BatchNumber, ExpiryDate, PurchasePrice, SellingPrice, " +
                               "CurrentQuantity, InitialQuantity, PurchaseItemId, BatchStatus, IsActive, CreatedDate, UpdatedDate " +
                               "FROM StockBatches WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapStockBatch(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve stock batch by Id.", exception);
            }
            return null;
        }

        public List<StockBatch> GetByMedicineId(int medicineId)
        {
            const string sql = "SELECT Id, MedicineId, BatchNumber, ExpiryDate, PurchasePrice, SellingPrice, " +
                               "CurrentQuantity, InitialQuantity, PurchaseItemId, IsActive, CreatedDate, UpdatedDate " +
                               "FROM StockBatches WHERE MedicineId = @MedicineId AND IsActive = 1 " +
                               "ORDER BY ExpiryDate ASC, CreatedDate ASC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MedicineId", medicineId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<StockBatch> batches = new List<StockBatch>();
                        while (reader.Read())
                            batches.Add(MapStockBatch(reader));
                        return batches;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve stock batches by medicine.", exception);
            }
        }

        public StockBatch GetBatch(int medicineId, string batchNumber)
        {
            Guard.AgainstNullOrWhiteSpace(batchNumber, nameof(batchNumber));
            const string sql = "SELECT Id, MedicineId, BatchNumber, ExpiryDate, PurchasePrice, SellingPrice, " +
                               "CurrentQuantity, InitialQuantity, PurchaseItemId, IsActive, CreatedDate, UpdatedDate " +
                               "FROM StockBatches WHERE MedicineId = @MedicineId AND BatchNumber = @BatchNumber AND IsActive = 1";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MedicineId", medicineId);
                    command.Parameters.AddWithValue("@BatchNumber", batchNumber);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapStockBatch(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve stock batch.", exception);
            }
            return null;
        }

        public List<StockBatch> GetFIFOBatches(int medicineId, int quantity)
        {
            const string sql = "SELECT Id, MedicineId, BatchNumber, ExpiryDate, PurchasePrice, SellingPrice, " +
                               "CurrentQuantity, InitialQuantity, PurchaseItemId, IsActive, CreatedDate, UpdatedDate " +
                               "FROM StockBatches WHERE MedicineId = @MedicineId AND IsActive = 1 " +
                               "AND CurrentQuantity > 0 " +
                               "ORDER BY ExpiryDate ASC, CreatedDate ASC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MedicineId", medicineId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<StockBatch> batches = new List<StockBatch>();
                        int remaining = quantity;
                        while (reader.Read() && remaining > 0)
                        {
                            StockBatch batch = MapStockBatch(reader);
                            batches.Add(batch);
                            remaining -= batch.CurrentQuantity;
                        }
                        return batches;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve FIFO batches.", exception);
            }
        }

        public int GetAvailableStock(int medicineId)
        {
            const string sql = "SELECT COALESCE(SUM(CurrentQuantity), 0) " +
                               "FROM StockBatches WHERE MedicineId = @MedicineId AND IsActive = 1";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MedicineId", medicineId);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve available stock.", exception);
            }
        }

        public int Add(StockBatch batch)
        {
            Guard.AgainstNull(batch, nameof(batch));
            const string sql = "INSERT INTO StockBatches (MedicineId, BatchNumber, ExpiryDate, " +
                               "PurchasePrice, SellingPrice, CurrentQuantity, InitialQuantity, PurchaseItemId) " +
                               "VALUES (@MedicineId, @BatchNumber, @ExpiryDate, " +
                               "@PurchasePrice, @SellingPrice, @CurrentQuantity, @InitialQuantity, @PurchaseItemId); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    SetBatchParameters(command, batch);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add stock batch.", exception);
            }
        }

        public int Add(StockBatch batch, IDbConnection connection, IDbTransaction transaction)
        {
            Guard.AgainstNull(batch, nameof(batch));
            const string sql = "INSERT INTO StockBatches (MedicineId, BatchNumber, ExpiryDate, " +
                               "PurchasePrice, SellingPrice, CurrentQuantity, InitialQuantity, PurchaseItemId) " +
                               "VALUES (@MedicineId, @BatchNumber, @ExpiryDate, " +
                               "@PurchasePrice, @SellingPrice, @CurrentQuantity, @InitialQuantity, @PurchaseItemId); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    SetBatchParameters(command, batch);
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add stock batch.", exception);
            }
        }

        public void UpdateQuantity(int batchId, int quantity)
        {
            const string sql = "UPDATE StockBatches SET CurrentQuantity = @CurrentQuantity, " +
                               "UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", batchId);
                    command.Parameters.AddWithValue("@CurrentQuantity", quantity);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update stock batch quantity.", exception);
            }
        }

        public void UpdateQuantity(int batchId, int quantity, IDbConnection connection, IDbTransaction transaction)
        {
            const string sql = "UPDATE StockBatches SET CurrentQuantity = @CurrentQuantity, " +
                               "UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    command.Parameters.AddWithValue("@Id", batchId);
                    command.Parameters.AddWithValue("@CurrentQuantity", quantity);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update stock batch quantity.", exception);
            }
        }

        public void Deactivate(int batchId)
        {
            const string sql = "UPDATE StockBatches SET IsActive = 0, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", batchId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to deactivate stock batch.", exception);
            }
        }

        private static void SetBatchParameters(SqlCommand command, StockBatch batch)
        {
            command.Parameters.AddWithValue("@MedicineId", batch.MedicineId);
            command.Parameters.AddWithValue("@BatchNumber", batch.BatchNumber);
            command.Parameters.AddWithValue("@ExpiryDate", batch.ExpiryDate);
            command.Parameters.AddWithValue("@PurchasePrice", batch.PurchasePrice);
            command.Parameters.AddWithValue("@SellingPrice", batch.SellingPrice);
            command.Parameters.AddWithValue("@CurrentQuantity", batch.CurrentQuantity);
            command.Parameters.AddWithValue("@InitialQuantity", batch.InitialQuantity);
            command.Parameters.AddWithValue("@PurchaseItemId", batch.PurchaseItemId);
        }

        private static StockBatch MapStockBatch(SqlDataReader reader)
        {
            return new StockBatch
            {
                Id = (int)reader["Id"],
                MedicineId = (int)reader["MedicineId"],
                BatchNumber = (string)reader["BatchNumber"],
                ExpiryDate = (DateTime)reader["ExpiryDate"],
                PurchasePrice = (decimal)reader["PurchasePrice"],
                SellingPrice = (decimal)reader["SellingPrice"],
                CurrentQuantity = (int)reader["CurrentQuantity"],
                InitialQuantity = (int)reader["InitialQuantity"],
                PurchaseItemId = (int)reader["PurchaseItemId"],
                Status = (BatchStatus)(int)reader["BatchStatus"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };
        }
    }
}
