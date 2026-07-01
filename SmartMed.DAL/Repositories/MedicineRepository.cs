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
    public class MedicineRepository : IMedicineRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MedicineRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public Medicine GetById(int id)
        {
            const string sql = "SELECT Id, CategoryId, Name, Brand, DosageForm, Strength, Unit, " +
                               "StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Medicines WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapMedicine(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve medicine by Id.", exception);
            }
            return null;
        }

        public List<Medicine> GetAll()
        {
            const string sql = "SELECT Id, CategoryId, Name, Brand, DosageForm, Strength, Unit, " +
                               "StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Medicines WHERE IsActive = 1";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Medicine> medicines = new List<Medicine>();
                        while (reader.Read())
                            medicines.Add(MapMedicine(reader));
                        return medicines;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve medicines.", exception);
            }
        }

        public List<Medicine> GetByCategoryId(int categoryId)
        {
            const string sql = "SELECT Id, CategoryId, Name, Brand, DosageForm, Strength, Unit, " +
                               "StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Medicines WHERE CategoryId = @CategoryId AND IsActive = 1";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CategoryId", categoryId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Medicine> medicines = new List<Medicine>();
                        while (reader.Read())
                            medicines.Add(MapMedicine(reader));
                        return medicines;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve medicines by category.", exception);
            }
        }

        public List<Medicine> Search(string keyword)
        {
            Guard.AgainstNullOrWhiteSpace(keyword, nameof(keyword));
            const string sql = "SELECT Id, CategoryId, Name, Brand, DosageForm, Strength, Unit, " +
                               "StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Medicines WHERE IsActive = 1 AND " +
                               "(Name LIKE @Keyword OR Brand LIKE @Keyword)";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Medicine> medicines = new List<Medicine>();
                        while (reader.Read())
                            medicines.Add(MapMedicine(reader));
                        return medicines;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to search medicines.", exception);
            }
        }

        public List<Medicine> GetLowStock()
        {
            const string sql = "SELECT Id, CategoryId, Name, Brand, DosageForm, Strength, Unit, " +
                               "StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Medicines WHERE IsActive = 1 AND StockQuantity <= ReorderLevel";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Medicine> medicines = new List<Medicine>();
                        while (reader.Read())
                            medicines.Add(MapMedicine(reader));
                        return medicines;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve low stock medicines.", exception);
            }
        }

        public List<Medicine> GetNearExpiry(int thresholdDays)
        {
            Guard.AgainstNegative(thresholdDays, nameof(thresholdDays));
            const string sql = "SELECT Id, CategoryId, Name, Brand, DosageForm, Strength, Unit, " +
                               "StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Medicines WHERE IsActive = 1 AND ExpiryDate IS NOT NULL " +
                               "AND ExpiryDate <= DATEADD(DAY, @ThresholdDays, GETUTCDATE()) " +
                               "AND ExpiryDate >= GETUTCDATE()";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ThresholdDays", thresholdDays);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Medicine> medicines = new List<Medicine>();
                        while (reader.Read())
                            medicines.Add(MapMedicine(reader));
                        return medicines;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve near expiry medicines.", exception);
            }
        }

        public Medicine GetByNameAndBrand(string name, string brand)
        {
            Guard.AgainstNullOrWhiteSpace(name, nameof(name));
            const string sql = "SELECT Id, CategoryId, Name, Brand, DosageForm, Strength, Unit, " +
                               "StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description, " +
                               "IsActive, CreatedDate, UpdatedDate " +
                               "FROM Medicines WHERE Name = @Name AND " +
                               "((Brand = @Brand) OR (Brand IS NULL AND @Brand IS NULL))";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Brand", (object)brand ?? DBNull.Value);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapMedicine(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve medicine by name and brand.", exception);
            }
            return null;
        }

        public int Add(Medicine medicine)
        {
            Guard.AgainstNull(medicine, nameof(medicine));
            const string sql = "INSERT INTO Medicines (CategoryId, Name, Brand, DosageForm, Strength, Unit, " +
                               "StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description) " +
                               "VALUES (@CategoryId, @Name, @Brand, @DosageForm, @Strength, @Unit, " +
                               "@StockQuantity, @ReorderLevel, @UnitPrice, @ExpiryDate, @Description); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CategoryId", medicine.CategoryId);
                    command.Parameters.AddWithValue("@Name", medicine.Name);
                    command.Parameters.AddWithValue("@Brand", (object)medicine.Brand ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DosageForm", (int)medicine.DosageForm);
                    command.Parameters.AddWithValue("@Strength", (object)medicine.Strength ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Unit", medicine.Unit);
                    command.Parameters.AddWithValue("@StockQuantity", medicine.StockQuantity);
                    command.Parameters.AddWithValue("@ReorderLevel", medicine.ReorderLevel);
                    command.Parameters.AddWithValue("@UnitPrice", medicine.UnitPrice);
                    command.Parameters.AddWithValue("@ExpiryDate", (object)medicine.ExpiryDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Description", (object)medicine.Description ?? DBNull.Value);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add medicine.", exception);
            }
        }

        public void Update(Medicine medicine)
        {
            Guard.AgainstNull(medicine, nameof(medicine));
            const string sql = "UPDATE Medicines SET CategoryId = @CategoryId, Name = @Name, " +
                               "Brand = @Brand, DosageForm = @DosageForm, Strength = @Strength, " +
                               "Unit = @Unit, StockQuantity = @StockQuantity, " +
                               "ReorderLevel = @ReorderLevel, UnitPrice = @UnitPrice, " +
                               "ExpiryDate = @ExpiryDate, Description = @Description, " +
                               "UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", medicine.Id);
                    command.Parameters.AddWithValue("@CategoryId", medicine.CategoryId);
                    command.Parameters.AddWithValue("@Name", medicine.Name);
                    command.Parameters.AddWithValue("@Brand", (object)medicine.Brand ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DosageForm", (int)medicine.DosageForm);
                    command.Parameters.AddWithValue("@Strength", (object)medicine.Strength ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Unit", medicine.Unit);
                    command.Parameters.AddWithValue("@StockQuantity", medicine.StockQuantity);
                    command.Parameters.AddWithValue("@ReorderLevel", medicine.ReorderLevel);
                    command.Parameters.AddWithValue("@UnitPrice", medicine.UnitPrice);
                    command.Parameters.AddWithValue("@ExpiryDate", (object)medicine.ExpiryDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Description", (object)medicine.Description ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update medicine.", exception);
            }
        }

        public void Delete(int id)
        {
            const string sql = "UPDATE Medicines SET IsActive = 0, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
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
                throw new DataAccessException("Failed to delete medicine.", exception);
            }
        }

        public void SetStockQuantity(int medicineId, int quantity)
        {
            const string sql = "UPDATE Medicines SET StockQuantity = @StockQuantity, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", medicineId);
                    command.Parameters.AddWithValue("@StockQuantity", quantity);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to set stock quantity.", exception);
            }
        }

        public void SetStockQuantity(int medicineId, int quantity, IDbConnection connection, IDbTransaction transaction)
        {
            const string sql = "UPDATE Medicines SET StockQuantity = @StockQuantity, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    command.Parameters.AddWithValue("@Id", medicineId);
                    command.Parameters.AddWithValue("@StockQuantity", quantity);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to set stock quantity.", exception);
            }
        }

        public void UpdateStockQuantity(int medicineId, int delta)
        {
            const string sql = "UPDATE Medicines SET StockQuantity = StockQuantity + @Delta, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", medicineId);
                    command.Parameters.AddWithValue("@Delta", delta);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update stock quantity.", exception);
            }
        }

        public void UpdateStockQuantity(int medicineId, int delta, IDbConnection connection, IDbTransaction transaction)
        {
            const string sql = "UPDATE Medicines SET StockQuantity = StockQuantity + @Delta, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction))
                {
                    command.Parameters.AddWithValue("@Id", medicineId);
                    command.Parameters.AddWithValue("@Delta", delta);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update stock quantity.", exception);
            }
        }

        private static Medicine MapMedicine(SqlDataReader reader)
        {
            return new Medicine
            {
                Id = (int)reader["Id"],
                CategoryId = (int)reader["CategoryId"],
                Name = (string)reader["Name"],
                Brand = reader["Brand"] == DBNull.Value ? null : (string)reader["Brand"],
                DosageForm = (DosageForm)(int)reader["DosageForm"],
                Strength = reader["Strength"] == DBNull.Value ? null : (string)reader["Strength"],
                Unit = (string)reader["Unit"],
                StockQuantity = (int)reader["StockQuantity"],
                ReorderLevel = (int)reader["ReorderLevel"],
                UnitPrice = (decimal)reader["UnitPrice"],
                ExpiryDate = reader["ExpiryDate"] == DBNull.Value ? null : (DateTime?)reader["ExpiryDate"],
                Description = reader["Description"] == DBNull.Value ? null : (string)reader["Description"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };
        }
    }
}
