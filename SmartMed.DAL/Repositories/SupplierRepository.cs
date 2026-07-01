using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SupplierRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public List<Supplier> GetAll()
        {
            const string sql = "SELECT Id, SupplierCode, SupplierName, CompanyName, ContactPerson, " +
                               "PhoneNumber, Email, Address, City, Country, PostalCode, TaxNumber, Notes, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Suppliers WHERE IsActive = 1";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Supplier> suppliers = new List<Supplier>();
                        while (reader.Read())
                            suppliers.Add(MapSupplier(reader));
                        return suppliers;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve suppliers.", exception);
            }
        }

        public Supplier GetById(int id)
        {
            const string sql = "SELECT Id, SupplierCode, SupplierName, CompanyName, ContactPerson, " +
                               "PhoneNumber, Email, Address, City, Country, PostalCode, TaxNumber, Notes, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Suppliers WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapSupplier(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve supplier by Id.", exception);
            }
            return null;
        }

        public Supplier GetBySupplierCode(string supplierCode)
        {
            Guard.AgainstNullOrWhiteSpace(supplierCode, nameof(supplierCode));
            const string sql = "SELECT Id, SupplierCode, SupplierName, CompanyName, ContactPerson, " +
                               "PhoneNumber, Email, Address, City, Country, PostalCode, TaxNumber, Notes, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Suppliers WHERE SupplierCode = @SupplierCode";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SupplierCode", supplierCode);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapSupplier(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve supplier by code.", exception);
            }
            return null;
        }

        public Supplier GetByName(string supplierName)
        {
            Guard.AgainstNullOrWhiteSpace(supplierName, nameof(supplierName));
            const string sql = "SELECT Id, SupplierCode, SupplierName, CompanyName, ContactPerson, " +
                               "PhoneNumber, Email, Address, City, Country, PostalCode, TaxNumber, Notes, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Suppliers WHERE SupplierName = @SupplierName";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SupplierName", supplierName);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapSupplier(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve supplier by name.", exception);
            }
            return null;
        }

        public List<Supplier> Search(string keyword)
        {
            Guard.AgainstNullOrWhiteSpace(keyword, nameof(keyword));
            const string sql = "SELECT Id, SupplierCode, SupplierName, CompanyName, ContactPerson, " +
                               "PhoneNumber, Email, Address, City, Country, PostalCode, TaxNumber, Notes, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Suppliers WHERE IsActive = 1 AND " +
                               "(SupplierCode LIKE @Keyword OR SupplierName LIKE @Keyword OR " +
                               "CompanyName LIKE @Keyword OR PhoneNumber LIKE @Keyword)";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Supplier> suppliers = new List<Supplier>();
                        while (reader.Read())
                            suppliers.Add(MapSupplier(reader));
                        return suppliers;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to search suppliers.", exception);
            }
        }

        public int Add(Supplier supplier)
        {
            Guard.AgainstNull(supplier, nameof(supplier));
            const string sql = "INSERT INTO Suppliers (SupplierCode, SupplierName, CompanyName, ContactPerson, " +
                               "PhoneNumber, Email, Address, City, Country, PostalCode, TaxNumber, Notes) " +
                               "VALUES (@SupplierCode, @SupplierName, @CompanyName, @ContactPerson, " +
                               "@PhoneNumber, @Email, @Address, @City, @Country, @PostalCode, @TaxNumber, @Notes); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SupplierCode", supplier.SupplierCode);
                    command.Parameters.AddWithValue("@SupplierName", supplier.SupplierName);
                    command.Parameters.AddWithValue("@CompanyName", (object)supplier.CompanyName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ContactPerson", (object)supplier.ContactPerson ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PhoneNumber", (object)supplier.PhoneNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object)supplier.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object)supplier.Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@City", (object)supplier.City ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Country", (object)supplier.Country ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PostalCode", (object)supplier.PostalCode ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TaxNumber", (object)supplier.TaxNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Notes", (object)supplier.Notes ?? DBNull.Value);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add supplier.", exception);
            }
        }

        public void Update(Supplier supplier)
        {
            Guard.AgainstNull(supplier, nameof(supplier));
            const string sql = "UPDATE Suppliers SET SupplierCode = @SupplierCode, " +
                               "SupplierName = @SupplierName, CompanyName = @CompanyName, " +
                               "ContactPerson = @ContactPerson, PhoneNumber = @PhoneNumber, " +
                               "Email = @Email, Address = @Address, City = @City, " +
                               "Country = @Country, PostalCode = @PostalCode, " +
                               "TaxNumber = @TaxNumber, Notes = @Notes, " +
                               "UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", supplier.Id);
                    command.Parameters.AddWithValue("@SupplierCode", supplier.SupplierCode);
                    command.Parameters.AddWithValue("@SupplierName", supplier.SupplierName);
                    command.Parameters.AddWithValue("@CompanyName", (object)supplier.CompanyName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ContactPerson", (object)supplier.ContactPerson ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PhoneNumber", (object)supplier.PhoneNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object)supplier.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object)supplier.Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@City", (object)supplier.City ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Country", (object)supplier.Country ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PostalCode", (object)supplier.PostalCode ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TaxNumber", (object)supplier.TaxNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Notes", (object)supplier.Notes ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update supplier.", exception);
            }
        }

        public void Delete(int id)
        {
            const string sql = "UPDATE Suppliers SET IsActive = 0, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to delete supplier.", exception);
            }
        }

        public bool HasPurchases(int supplierId)
        {
            const string sql = "SELECT COUNT(1) FROM Purchases WHERE SupplierId = @SupplierId";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SupplierId", supplierId);
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to check supplier purchase history.", exception);
            }
        }

        private static Supplier MapSupplier(SqlDataReader reader)
        {
            return new Supplier
            {
                Id = (int)reader["Id"],
                SupplierCode = (string)reader["SupplierCode"],
                SupplierName = (string)reader["SupplierName"],
                CompanyName = reader["CompanyName"] == DBNull.Value ? null : (string)reader["CompanyName"],
                ContactPerson = reader["ContactPerson"] == DBNull.Value ? null : (string)reader["ContactPerson"],
                PhoneNumber = reader["PhoneNumber"] == DBNull.Value ? null : (string)reader["PhoneNumber"],
                Email = reader["Email"] == DBNull.Value ? null : (string)reader["Email"],
                Address = reader["Address"] == DBNull.Value ? null : (string)reader["Address"],
                City = reader["City"] == DBNull.Value ? null : (string)reader["City"],
                Country = reader["Country"] == DBNull.Value ? null : (string)reader["Country"],
                PostalCode = reader["PostalCode"] == DBNull.Value ? null : (string)reader["PostalCode"],
                TaxNumber = reader["TaxNumber"] == DBNull.Value ? null : (string)reader["TaxNumber"],
                Notes = reader["Notes"] == DBNull.Value ? null : (string)reader["Notes"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };
        }
    }
}
