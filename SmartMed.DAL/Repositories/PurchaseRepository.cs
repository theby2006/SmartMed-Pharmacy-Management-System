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
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PurchaseRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public List<Purchase> GetAll()
        {
            const string sql = "SELECT Id, PurchaseNumber, PurchaseDate, SupplierId, InvoiceNumber, " +
                               "Remarks, CreatedByUserId, CreatedDate, UpdatedDate, IsActive, Status, ConfirmedDate, TotalAmount " +
                               "FROM Purchases ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Purchase> purchases = new List<Purchase>();
                        while (reader.Read())
                            purchases.Add(MapPurchase(reader));
                        return purchases;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve purchases.", exception);
            }
        }

        public Purchase GetById(int id)
        {
            const string sql = "SELECT Id, PurchaseNumber, PurchaseDate, SupplierId, InvoiceNumber, " +
                               "Remarks, CreatedByUserId, CreatedDate, UpdatedDate, IsActive, Status, ConfirmedDate, TotalAmount " +
                               "FROM Purchases WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapPurchase(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve purchase by Id.", exception);
            }
            return null;
        }

        public Purchase GetByPurchaseNumber(string purchaseNumber)
        {
            Guard.AgainstNullOrWhiteSpace(purchaseNumber, nameof(purchaseNumber));
            const string sql = "SELECT Id, PurchaseNumber, PurchaseDate, SupplierId, InvoiceNumber, " +
                               "Remarks, CreatedByUserId, CreatedDate, UpdatedDate, IsActive, Status, ConfirmedDate, TotalAmount " +
                               "FROM Purchases WHERE PurchaseNumber = @PurchaseNumber";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PurchaseNumber", purchaseNumber);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapPurchase(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve purchase by number.", exception);
            }
            return null;
        }

        public List<Purchase> Search(string keyword)
        {
            Guard.AgainstNullOrWhiteSpace(keyword, nameof(keyword));
            const string sql = "SELECT p.Id, p.PurchaseNumber, p.PurchaseDate, p.SupplierId, p.InvoiceNumber, " +
                               "p.Remarks, p.CreatedByUserId, p.CreatedDate, p.UpdatedDate, p.IsActive, p.Status, p.ConfirmedDate, p.TotalAmount " +
                               "FROM Purchases p " +
                               "LEFT JOIN Suppliers s ON p.SupplierId = s.Id " +
                               "WHERE p.PurchaseNumber LIKE @Keyword OR " +
                               "p.InvoiceNumber LIKE @Keyword OR " +
                               "s.SupplierName LIKE @Keyword " +
                               "ORDER BY p.CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Purchase> purchases = new List<Purchase>();
                        while (reader.Read())
                            purchases.Add(MapPurchase(reader));
                        return purchases;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to search purchases.", exception);
            }
        }

        public List<Purchase> GetBySupplier(int supplierId)
        {
            const string sql = "SELECT Id, PurchaseNumber, PurchaseDate, SupplierId, InvoiceNumber, " +
                               "Remarks, CreatedByUserId, CreatedDate, UpdatedDate, IsActive, Status, ConfirmedDate, TotalAmount " +
                               "FROM Purchases WHERE SupplierId = @SupplierId ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SupplierId", supplierId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Purchase> purchases = new List<Purchase>();
                        while (reader.Read())
                            purchases.Add(MapPurchase(reader));
                        return purchases;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve purchases by supplier.", exception);
            }
        }

        public List<Purchase> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            const string sql = "SELECT Id, PurchaseNumber, PurchaseDate, SupplierId, InvoiceNumber, " +
                               "Remarks, CreatedByUserId, CreatedDate, UpdatedDate, IsActive, Status, ConfirmedDate, TotalAmount " +
                               "FROM Purchases WHERE PurchaseDate >= @FromDate AND PurchaseDate <= @ToDate " +
                               "ORDER BY PurchaseDate DESC";
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
                        List<Purchase> purchases = new List<Purchase>();
                        while (reader.Read())
                            purchases.Add(MapPurchase(reader));
                        return purchases;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve purchases by date range.", exception);
            }
        }

        public int Add(Purchase purchase)
        {
            Guard.AgainstNull(purchase, nameof(purchase));
            const string sql = "INSERT INTO Purchases (PurchaseNumber, PurchaseDate, SupplierId, InvoiceNumber, " +
                               "Remarks, CreatedByUserId, CreatedDate, IsActive, Status, TotalAmount) " +
                               "VALUES (@PurchaseNumber, @PurchaseDate, @SupplierId, @InvoiceNumber, " +
                               "@Remarks, @CreatedByUserId, @CreatedDate, @IsActive, @Status, @TotalAmount); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    SetPurchaseInsertParameters(command, purchase);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add purchase.", exception);
            }
        }

        public int Add(Purchase purchase, IDbConnection connection, IDbTransaction transaction)
        {
            Guard.AgainstNull(purchase, nameof(purchase));
            const string sql = "INSERT INTO Purchases (PurchaseNumber, PurchaseDate, SupplierId, InvoiceNumber, " +
                               "Remarks, CreatedByUserId, CreatedDate, IsActive, Status, TotalAmount) " +
                               "VALUES (@PurchaseNumber, @PurchaseDate, @SupplierId, @InvoiceNumber, " +
                               "@Remarks, @CreatedByUserId, @CreatedDate, @IsActive, @Status, @TotalAmount); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    SetPurchaseInsertParameters(command, purchase);
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add purchase.", exception);
            }
        }

        public void Update(Purchase purchase)
        {
            Guard.AgainstNull(purchase, nameof(purchase));
            const string sql = "UPDATE Purchases SET PurchaseNumber = @PurchaseNumber, " +
                               "PurchaseDate = @PurchaseDate, SupplierId = @SupplierId, " +
                               "InvoiceNumber = @InvoiceNumber, Remarks = @Remarks, " +
                               "TotalAmount = @TotalAmount, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", purchase.Id);
                    command.Parameters.AddWithValue("@PurchaseNumber", purchase.PurchaseNumber);
                    command.Parameters.AddWithValue("@PurchaseDate", purchase.PurchaseDate);
                    command.Parameters.AddWithValue("@SupplierId", purchase.SupplierId);
                    command.Parameters.AddWithValue("@InvoiceNumber", (object)purchase.InvoiceNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Remarks", (object)purchase.Remarks ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TotalAmount", purchase.TotalAmount);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update purchase.", exception);
            }
        }

        public void Confirm(int purchaseId, DateTime confirmedDate)
        {
            const string sql = "UPDATE Purchases SET Status = @Status, ConfirmedDate = @ConfirmedDate, " +
                               "UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", purchaseId);
                    command.Parameters.AddWithValue("@Status", (int)PurchaseStatus.Confirmed);
                    command.Parameters.AddWithValue("@ConfirmedDate", confirmedDate);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to confirm purchase.", exception);
            }
        }

        public void Confirm(int purchaseId, DateTime confirmedDate, decimal totalAmount, IDbConnection connection, IDbTransaction transaction)
        {
            const string sql = "UPDATE Purchases SET Status = @Status, ConfirmedDate = @ConfirmedDate, " +
                               "TotalAmount = @TotalAmount, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    command.Parameters.AddWithValue("@Id", purchaseId);
                    command.Parameters.AddWithValue("@Status", (int)PurchaseStatus.Confirmed);
                    command.Parameters.AddWithValue("@ConfirmedDate", confirmedDate);
                    command.Parameters.AddWithValue("@TotalAmount", totalAmount);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to confirm purchase.", exception);
            }
        }

        public void Cancel(int purchaseId)
        {
            const string sql = "UPDATE Purchases SET Status = @Status, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", purchaseId);
                    command.Parameters.AddWithValue("@Status", (int)PurchaseStatus.Cancelled);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to cancel purchase.", exception);
            }
        }

        public void Cancel(int purchaseId, IDbConnection connection, IDbTransaction transaction)
        {
            const string sql = "UPDATE Purchases SET Status = @Status, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    command.Parameters.AddWithValue("@Id", purchaseId);
                    command.Parameters.AddWithValue("@Status", (int)PurchaseStatus.Cancelled);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to cancel purchase.", exception);
            }
        }

        private static void SetPurchaseInsertParameters(SqlCommand command, Purchase purchase)
        {
            command.Parameters.AddWithValue("@PurchaseNumber", purchase.PurchaseNumber);
            command.Parameters.AddWithValue("@PurchaseDate", purchase.PurchaseDate);
            command.Parameters.AddWithValue("@SupplierId", purchase.SupplierId);
            command.Parameters.AddWithValue("@InvoiceNumber", (object)purchase.InvoiceNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@Remarks", (object)purchase.Remarks ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedByUserId", purchase.CreatedByUserId);
            command.Parameters.AddWithValue("@CreatedDate", purchase.CreatedDate);
            command.Parameters.AddWithValue("@Status", (int)purchase.Status);
            command.Parameters.AddWithValue("@TotalAmount", purchase.TotalAmount);
            command.Parameters.AddWithValue("@IsActive", purchase.IsActive);
        }

        private static Purchase MapPurchase(SqlDataReader reader)
        {
            return new Purchase
            {
                Id = (int)reader["Id"],
                PurchaseNumber = (string)reader["PurchaseNumber"],
                PurchaseDate = (DateTime)reader["PurchaseDate"],
                SupplierId = (int)reader["SupplierId"],
                InvoiceNumber = reader["InvoiceNumber"] == DBNull.Value ? null : (string)reader["InvoiceNumber"],
                Remarks = reader["Remarks"] == DBNull.Value ? null : (string)reader["Remarks"],
                CreatedByUserId = (int)reader["CreatedByUserId"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"],
                IsActive = (bool)reader["IsActive"],
                Status = (PurchaseStatus)(int)reader["Status"],
                ConfirmedDate = reader["ConfirmedDate"] == DBNull.Value ? null : (DateTime?)reader["ConfirmedDate"],
                TotalAmount = (decimal)reader["TotalAmount"]
            };
        }
    }
}
