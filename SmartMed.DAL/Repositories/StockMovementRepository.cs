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
    public class StockMovementRepository : IStockMovementRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public StockMovementRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public List<StockMovement> GetByStockBatchId(int stockBatchId)
        {
            const string sql = "SELECT Id, StockBatchId, MedicineId, MovementType, Quantity, " +
                               "ReferenceType, ReferenceId, UnitPrice, TotalAmount, " +
                               "CreatedByUserId, CreatedDate, UpdatedDate, IsActive, Notes " +
                               "FROM StockMovements WHERE StockBatchId = @StockBatchId " +
                               "ORDER BY CreatedDate ASC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@StockBatchId", stockBatchId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<StockMovement> movements = new List<StockMovement>();
                        while (reader.Read())
                            movements.Add(MapStockMovement(reader));
                        return movements;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve stock movements by batch.", exception);
            }
        }

        public List<StockMovement> GetByMedicineId(int medicineId)
        {
            const string sql = "SELECT Id, StockBatchId, MedicineId, MovementType, Quantity, " +
                               "ReferenceType, ReferenceId, UnitPrice, TotalAmount, " +
                               "CreatedByUserId, CreatedDate, UpdatedDate, IsActive, Notes " +
                               "FROM StockMovements WHERE MedicineId = @MedicineId " +
                               "ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MedicineId", medicineId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<StockMovement> movements = new List<StockMovement>();
                        while (reader.Read())
                            movements.Add(MapStockMovement(reader));
                        return movements;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve stock movements by medicine.", exception);
            }
        }

        public List<StockMovement> GetByReference(string referenceType, int referenceId)
        {
            Guard.AgainstNullOrWhiteSpace(referenceType, nameof(referenceType));
            const string sql = "SELECT Id, StockBatchId, MedicineId, MovementType, Quantity, " +
                               "ReferenceType, ReferenceId, UnitPrice, TotalAmount, " +
                               "CreatedByUserId, CreatedDate, UpdatedDate, IsActive, Notes " +
                               "FROM StockMovements WHERE ReferenceType = @ReferenceType AND ReferenceId = @ReferenceId " +
                               "ORDER BY CreatedDate ASC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ReferenceType", referenceType);
                    command.Parameters.AddWithValue("@ReferenceId", referenceId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<StockMovement> movements = new List<StockMovement>();
                        while (reader.Read())
                            movements.Add(MapStockMovement(reader));
                        return movements;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve stock movements by reference.", exception);
            }
        }

        public int Add(StockMovement movement)
        {
            Guard.AgainstNull(movement, nameof(movement));
            const string sql = "INSERT INTO StockMovements (StockBatchId, MedicineId, MovementType, Quantity, " +
                               "ReferenceType, ReferenceId, UnitPrice, TotalAmount, CreatedByUserId, CreatedDate, IsActive, Notes) " +
                               "VALUES (@StockBatchId, @MedicineId, @MovementType, @Quantity, " +
                               "@ReferenceType, @ReferenceId, @UnitPrice, @TotalAmount, @CreatedByUserId, @CreatedDate, @IsActive, @Notes); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    SetMovementParameters(command, movement);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add stock movement.", exception);
            }
        }

        public int Add(StockMovement movement, IDbConnection connection, IDbTransaction transaction)
        {
            Guard.AgainstNull(movement, nameof(movement));
            const string sql = "INSERT INTO StockMovements (StockBatchId, MedicineId, MovementType, Quantity, " +
                               "ReferenceType, ReferenceId, UnitPrice, TotalAmount, CreatedByUserId, CreatedDate, IsActive, Notes) " +
                               "VALUES (@StockBatchId, @MedicineId, @MovementType, @Quantity, " +
                               "@ReferenceType, @ReferenceId, @UnitPrice, @TotalAmount, @CreatedByUserId, @CreatedDate, @IsActive, @Notes); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    SetMovementParameters(command, movement);
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add stock movement.", exception);
            }
        }

        private static void SetMovementParameters(SqlCommand command, StockMovement movement)
        {
            command.Parameters.AddWithValue("@StockBatchId", movement.StockBatchId);
            command.Parameters.AddWithValue("@MedicineId", movement.MedicineId);
            command.Parameters.AddWithValue("@MovementType", (int)movement.MovementType);
            command.Parameters.AddWithValue("@Quantity", movement.Quantity);
            command.Parameters.AddWithValue("@ReferenceType", movement.ReferenceType);
            command.Parameters.AddWithValue("@ReferenceId", movement.ReferenceId);
            command.Parameters.AddWithValue("@UnitPrice", movement.UnitPrice);
            command.Parameters.AddWithValue("@TotalAmount", movement.TotalAmount);
            command.Parameters.AddWithValue("@CreatedByUserId", movement.CreatedByUserId);
            command.Parameters.AddWithValue("@CreatedDate", movement.CreatedDate);
            command.Parameters.AddWithValue("@Notes", (object)movement.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsActive", movement.IsActive);
        }

        private static StockMovement MapStockMovement(SqlDataReader reader)
        {
            return new StockMovement
            {
                Id = (int)reader["Id"],
                StockBatchId = (int)reader["StockBatchId"],
                MedicineId = (int)reader["MedicineId"],
                MovementType = (MovementType)(int)reader["MovementType"],
                Quantity = (int)reader["Quantity"],
                ReferenceType = (string)reader["ReferenceType"],
                ReferenceId = (int)reader["ReferenceId"],
                UnitPrice = (decimal)reader["UnitPrice"],
                TotalAmount = (decimal)reader["TotalAmount"],
                CreatedByUserId = (int)reader["CreatedByUserId"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"],
                IsActive = (bool)reader["IsActive"],
                Notes = reader["Notes"] == DBNull.Value ? null : (string)reader["Notes"]
            };
        }
    }
}
