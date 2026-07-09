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
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public OrderItemRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public List<OrderItem> GetByOrderId(int orderId)
        {
            const string sql = "SELECT Id, OrderId, MedicineId, Quantity, UnitPrice, DiscountPercent, " +
                               "LineTotal, IsActive, CreatedDate, UpdatedDate " +
                               "FROM OrderItems WHERE OrderId = @OrderId";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<OrderItem> items = new List<OrderItem>();
                        while (reader.Read())
                            items.Add(MapOrderItem(reader));
                        return items;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve order items.", exception);
            }
        }

        public void AddRange(List<OrderItem> items, IDbConnection connection, IDbTransaction transaction)
        {
            Guard.AgainstNull(items, nameof(items));

            if (items.Count == 0)
                return;

            try
            {
                foreach (OrderItem item in items)
                {
                    const string sql = "INSERT INTO OrderItems (OrderId, MedicineId, Quantity, UnitPrice, " +
                                       "DiscountPercent, LineTotal, IsActive, CreatedDate) " +
                                       "VALUES (@OrderId, @MedicineId, @Quantity, @UnitPrice, " +
                                       "@DiscountPercent, @LineTotal, @IsActive, @CreatedDate); " +
                                       "SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                    {
                        command.Parameters.AddWithValue("@OrderId", item.OrderId);
                        command.Parameters.AddWithValue("@MedicineId", item.MedicineId);
                        command.Parameters.AddWithValue("@Quantity", item.Quantity);
                        command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                        command.Parameters.AddWithValue("@DiscountPercent", item.DiscountPercent);
                        command.Parameters.AddWithValue("@LineTotal", item.LineTotal);
                        command.Parameters.AddWithValue("@IsActive", item.IsActive);
                        command.Parameters.AddWithValue("@CreatedDate", item.CreatedDate);
                        item.Id = (int)command.ExecuteScalar();
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add order items.", exception);
            }
        }

        private static OrderItem MapOrderItem(SqlDataReader reader)
        {
            return new OrderItem
            {
                Id = (int)reader["Id"],
                OrderId = (int)reader["OrderId"],
                MedicineId = (int)reader["MedicineId"],
                Quantity = (int)reader["Quantity"],
                UnitPrice = (decimal)reader["UnitPrice"],
                DiscountPercent = (decimal)reader["DiscountPercent"],
                LineTotal = (decimal)reader["LineTotal"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };
        }
    }
}
