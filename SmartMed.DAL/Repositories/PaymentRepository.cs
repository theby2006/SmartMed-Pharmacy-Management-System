using System;
using System.Data;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;

namespace SmartMed.DAL.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PaymentRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public Payment GetById(int id)
        {
            const string sql = "SELECT Id, SaleId, PaymentMethod, AmountPaid, ChangeAmount, " +
                               "PaymentStatus, TransactionReference, ProcessedByUserId, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Payments WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapPayment(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve payment by Id.", exception);
            }
            return null;
        }

        public Payment GetBySaleId(int saleId)
        {
            const string sql = "SELECT Id, SaleId, PaymentMethod, AmountPaid, ChangeAmount, " +
                               "PaymentStatus, TransactionReference, ProcessedByUserId, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Payments WHERE SaleId = @SaleId";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SaleId", saleId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapPayment(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve payment by sale Id.", exception);
            }
            return null;
        }

        public int Add(Payment payment)
        {
            Guard.AgainstNull(payment, nameof(payment));
            const string sql = "INSERT INTO Payments (SaleId, PaymentMethod, AmountPaid, ChangeAmount, " +
                               "PaymentStatus, TransactionReference, ProcessedByUserId, IsActive, CreatedDate) " +
                               "VALUES (@SaleId, @PaymentMethod, @AmountPaid, @ChangeAmount, " +
                               "@PaymentStatus, @TransactionReference, @ProcessedByUserId, @IsActive, @CreatedDate); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    SetPaymentParameters(command, payment);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add payment.", exception);
            }
        }

        public int Add(Payment payment, IDbConnection connection, IDbTransaction transaction)
        {
            Guard.AgainstNull(payment, nameof(payment));
            const string sql = "INSERT INTO Payments (SaleId, PaymentMethod, AmountPaid, ChangeAmount, " +
                               "PaymentStatus, TransactionReference, ProcessedByUserId, IsActive, CreatedDate) " +
                               "VALUES (@SaleId, @PaymentMethod, @AmountPaid, @ChangeAmount, " +
                               "@PaymentStatus, @TransactionReference, @ProcessedByUserId, @IsActive, @CreatedDate); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    SetPaymentParameters(command, payment);
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add payment.", exception);
            }
        }

        private static void SetPaymentParameters(SqlCommand command, Payment payment)
        {
            command.Parameters.AddWithValue("@SaleId", payment.SaleId);
            command.Parameters.AddWithValue("@PaymentMethod", (int)payment.PaymentMethod);
            command.Parameters.AddWithValue("@AmountPaid", payment.AmountPaid);
            command.Parameters.AddWithValue("@ChangeAmount", payment.ChangeAmount);
            command.Parameters.AddWithValue("@PaymentStatus", (int)payment.PaymentStatus);
            command.Parameters.AddWithValue("@TransactionReference", (object)payment.TransactionReference ?? DBNull.Value);
            command.Parameters.AddWithValue("@ProcessedByUserId", payment.ProcessedByUserId);
            command.Parameters.AddWithValue("@IsActive", payment.IsActive);
            command.Parameters.AddWithValue("@CreatedDate", payment.CreatedDate);
        }

        private static Payment MapPayment(SqlDataReader reader)
        {
            return new Payment
            {
                Id = (int)reader["Id"],
                SaleId = (int)reader["SaleId"],
                PaymentMethod = (PaymentMethod)(int)reader["PaymentMethod"],
                AmountPaid = (decimal)reader["AmountPaid"],
                ChangeAmount = (decimal)reader["ChangeAmount"],
                PaymentStatus = (PaymentStatus)(int)reader["PaymentStatus"],
                TransactionReference = reader["TransactionReference"] == DBNull.Value ? null : (string)reader["TransactionReference"],
                ProcessedByUserId = (int)reader["ProcessedByUserId"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };
        }
    }
}
