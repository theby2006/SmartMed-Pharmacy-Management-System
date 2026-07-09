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
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        private const string SelectColumns =
            "SELECT Id, OrderNumber, CustomerId, OrderDate, Status, SubTotal, DiscountAmount, TaxAmount, " +
            "GrandTotal, PrescriptionFilePath, Notes, IsActive, CreatedDate, UpdatedDate FROM Orders";

        public OrderRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public Order GetById(int id)
        {
            string sql = SelectColumns + " WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapOrder(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve order by Id.", exception);
            }
            return null;
        }

        public Order GetByOrderNumber(string orderNumber)
        {
            Guard.AgainstNullOrWhiteSpace(orderNumber, nameof(orderNumber));
            string sql = SelectColumns + " WHERE OrderNumber = @OrderNumber";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@OrderNumber", orderNumber);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapOrder(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve order by number.", exception);
            }
            return null;
        }

        public List<Order> GetAll()
        {
            string sql = SelectColumns + " ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Order> orders = new List<Order>();
                        while (reader.Read())
                            orders.Add(MapOrder(reader));
                        return orders;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve orders.", exception);
            }
        }

        public List<Order> GetByCustomerId(int customerId)
        {
            string sql = SelectColumns + " WHERE CustomerId = @CustomerId ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Order> orders = new List<Order>();
                        while (reader.Read())
                            orders.Add(MapOrder(reader));
                        return orders;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve orders by customer.", exception);
            }
        }

        public List<Order> GetByStatus(OrderStatus status)
        {
            string sql = SelectColumns + " WHERE Status = @Status ORDER BY CreatedDate DESC";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Status", (int)status);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Order> orders = new List<Order>();
                        while (reader.Read())
                            orders.Add(MapOrder(reader));
                        return orders;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve orders by status.", exception);
            }
        }

        public int Add(Order order, IDbConnection connection, IDbTransaction transaction)
        {
            Guard.AgainstNull(order, nameof(order));
            const string sql = "INSERT INTO Orders (OrderNumber, CustomerId, OrderDate, Status, SubTotal, " +
                               "DiscountAmount, TaxAmount, GrandTotal, PrescriptionFilePath, Notes, IsActive, CreatedDate) " +
                               "VALUES (@OrderNumber, @CustomerId, @OrderDate, @Status, @SubTotal, " +
                               "@DiscountAmount, @TaxAmount, @GrandTotal, @PrescriptionFilePath, @Notes, @IsActive, @CreatedDate); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    command.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                    command.Parameters.AddWithValue("@CustomerId", order.CustomerId);
                    command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                    command.Parameters.AddWithValue("@Status", (int)order.Status);
                    command.Parameters.AddWithValue("@SubTotal", order.SubTotal);
                    command.Parameters.AddWithValue("@DiscountAmount", order.DiscountAmount);
                    command.Parameters.AddWithValue("@TaxAmount", order.TaxAmount);
                    command.Parameters.AddWithValue("@GrandTotal", order.GrandTotal);
                    command.Parameters.AddWithValue("@PrescriptionFilePath", (object)order.PrescriptionFilePath ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Notes", (object)order.Notes ?? DBNull.Value);
                    command.Parameters.AddWithValue("@IsActive", order.IsActive);
                    command.Parameters.AddWithValue("@CreatedDate", order.CreatedDate);
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add order.", exception);
            }
        }

        public void UpdateStatus(int orderId, OrderStatus status)
        {
            const string sql = "UPDATE Orders SET Status = @Status, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", orderId);
                    command.Parameters.AddWithValue("@Status", (int)status);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update order status.", exception);
            }
        }

        public void UpdateStatus(int orderId, OrderStatus status, IDbConnection connection, IDbTransaction transaction)
        {
            const string sql = "UPDATE Orders SET Status = @Status, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    command.Parameters.AddWithValue("@Id", orderId);
                    command.Parameters.AddWithValue("@Status", (int)status);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update order status.", exception);
            }
        }

        public void UpdatePrescriptionPath(int orderId, string relativePath)
        {
            const string sql = "UPDATE Orders SET PrescriptionFilePath = @PrescriptionFilePath, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", orderId);
                    command.Parameters.AddWithValue("@PrescriptionFilePath", (object)relativePath ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update prescription path.", exception);
            }
        }

        private static Order MapOrder(SqlDataReader reader)
        {
            return new Order
            {
                Id = (int)reader["Id"],
                OrderNumber = (string)reader["OrderNumber"],
                CustomerId = (int)reader["CustomerId"],
                OrderDate = (DateTime)reader["OrderDate"],
                Status = (OrderStatus)(int)reader["Status"],
                SubTotal = (decimal)reader["SubTotal"],
                DiscountAmount = (decimal)reader["DiscountAmount"],
                TaxAmount = (decimal)reader["TaxAmount"],
                GrandTotal = (decimal)reader["GrandTotal"],
                PrescriptionFilePath = reader["PrescriptionFilePath"] == DBNull.Value ? null : (string)reader["PrescriptionFilePath"],
                Notes = reader["Notes"] == DBNull.Value ? null : (string)reader["Notes"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };
        }
    }
}
