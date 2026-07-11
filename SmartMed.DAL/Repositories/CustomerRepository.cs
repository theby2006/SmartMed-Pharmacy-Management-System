using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        private const string SelectColumns =
            "SELECT Id, FullName, PhoneNumber, Email, PinHash, PinSalt, Address, City, " +
            "IsActive, CreatedDate, UpdatedDate FROM Customers";

        public CustomerRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public Customer GetById(int id)
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
                        if (reader.Read()) return MapCustomer(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve customer by Id.", exception);
            }
            return null;
        }

        public Customer GetByPhoneOrEmail(string identifier)
        {
            Guard.AgainstNullOrWhiteSpace(identifier, nameof(identifier));
            string sql = SelectColumns + " WHERE PhoneNumber = @Identifier OR Email = @Identifier";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Identifier", identifier);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapCustomer(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve customer by phone or email.", exception);
            }
            return null;
        }

        public List<Customer> GetAll()
        {
            string sql = SelectColumns + " WHERE IsActive = 1 ORDER BY FullName";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Customer> customers = new List<Customer>();
                        while (reader.Read())
                            customers.Add(MapCustomer(reader));
                        return customers;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve customers.", exception);
            }
        }

        public List<Customer> Search(string keyword)
        {
            Guard.AgainstNullOrWhiteSpace(keyword, nameof(keyword));
            string sql = SelectColumns +
                " WHERE IsActive = 1 AND (FullName LIKE @Keyword OR PhoneNumber LIKE @Keyword OR Email LIKE @Keyword) ORDER BY FullName";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Customer> customers = new List<Customer>();
                        while (reader.Read())
                            customers.Add(MapCustomer(reader));
                        return customers;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to search customers.", exception);
            }
        }

        public int Add(Customer customer)
        {
            Guard.AgainstNull(customer, nameof(customer));
            const string sql = "INSERT INTO Customers (FullName, PhoneNumber, Email, PinHash, PinSalt, Address, City) " +
                               "VALUES (@FullName, @PhoneNumber, @Email, @PinHash, @PinSalt, @Address, @City); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@FullName", customer.FullName);
                    command.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);
                    command.Parameters.AddWithValue("@Email", (object)customer.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PinHash", (object)customer.PinHash ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PinSalt", (object)customer.PinSalt ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object)customer.Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@City", (object)customer.City ?? DBNull.Value);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add customer.", exception);
            }
        }

        public void Update(Customer customer)
        {
            Guard.AgainstNull(customer, nameof(customer));
            const string sql = "UPDATE Customers SET FullName = @FullName, PhoneNumber = @PhoneNumber, " +
                               "Email = @Email, Address = @Address, City = @City, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", customer.Id);
                    command.Parameters.AddWithValue("@FullName", customer.FullName);
                    command.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);
                    command.Parameters.AddWithValue("@Email", (object)customer.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object)customer.Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@City", (object)customer.City ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update customer.", exception);
            }
        }

        public void UpdatePin(int customerId, string pinHash, string pinSalt)
        {
            const string sql = "UPDATE Customers SET PinHash = @PinHash, PinSalt = @PinSalt, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", customerId);
                    command.Parameters.AddWithValue("@PinHash", pinHash);
                    command.Parameters.AddWithValue("@PinSalt", pinSalt);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update customer PIN.", exception);
            }
        }

        public void UpdateActiveStatus(int customerId, bool isActive)
        {
            const string sql = "UPDATE Customers SET IsActive = @IsActive, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", customerId);
                    command.Parameters.AddWithValue("@IsActive", isActive);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update customer active status.", exception);
            }
        }

        public bool ExistsByPhoneOrEmail(string phoneNumber, string email)
        {
            const string sql = "SELECT COUNT(1) FROM Customers WHERE PhoneNumber = @PhoneNumber OR (@Email IS NOT NULL AND Email = @Email)";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PhoneNumber", (object)phoneNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                    connection.Open();
                    return (int)command.ExecuteScalar() > 0;
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to check duplicate customer.", exception);
            }
        }

        private static Customer MapCustomer(SqlDataReader reader)
        {
            return new Customer
            {
                Id = (int)reader["Id"],
                FullName = (string)reader["FullName"],
                PhoneNumber = (string)reader["PhoneNumber"],
                Email = reader["Email"] == DBNull.Value ? null : (string)reader["Email"],
                PinHash = reader["PinHash"] == DBNull.Value ? null : (string)reader["PinHash"],
                PinSalt = reader["PinSalt"] == DBNull.Value ? null : (string)reader["PinSalt"],
                Address = reader["Address"] == DBNull.Value ? null : (string)reader["Address"],
                City = reader["City"] == DBNull.Value ? null : (string)reader["City"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };
        }
    }
}
