using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Repositories
{
    public class MedicineCategoryRepository : IMedicineCategoryRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MedicineCategoryRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public List<MedicineCategory> GetAll()
        {
            const string sql = "SELECT Id, Name, Description, IsActive, CreatedDate, UpdatedDate " +
                               "FROM MedicineCategories WHERE IsActive = 1";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<MedicineCategory> categories = new List<MedicineCategory>();
                        while (reader.Read())
                            categories.Add(MapCategory(reader));
                        return categories;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve medicine categories.", exception);
            }
        }

        public MedicineCategory GetById(int id)
        {
            const string sql = "SELECT Id, Name, Description, IsActive, CreatedDate, UpdatedDate " +
                               "FROM MedicineCategories WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapCategory(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve medicine category by Id.", exception);
            }
            return null;
        }

        public MedicineCategory GetByName(string name)
        {
            Guard.AgainstNullOrWhiteSpace(name, nameof(name));
            const string sql = "SELECT Id, Name, Description, IsActive, CreatedDate, UpdatedDate " +
                               "FROM MedicineCategories WHERE Name = @Name";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapCategory(reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve medicine category by name.", exception);
            }
            return null;
        }

        public int Add(MedicineCategory category)
        {
            Guard.AgainstNull(category, nameof(category));
            const string sql = "INSERT INTO MedicineCategories (Name, Description) " +
                               "VALUES (@Name, @Description); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", category.Name);
                    command.Parameters.AddWithValue("@Description", (object)category.Description ?? DBNull.Value);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add medicine category.", exception);
            }
        }

        public void Update(MedicineCategory category)
        {
            Guard.AgainstNull(category, nameof(category));
            const string sql = "UPDATE MedicineCategories SET Name = @Name, Description = @Description, " +
                               "UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", category.Id);
                    command.Parameters.AddWithValue("@Name", category.Name);
                    command.Parameters.AddWithValue("@Description", (object)category.Description ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update medicine category.", exception);
            }
        }

        public void Delete(int id)
        {
            const string sql = "UPDATE MedicineCategories SET IsActive = 0, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
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
                throw new DataAccessException("Failed to delete medicine category.", exception);
            }
        }

        private static MedicineCategory MapCategory(SqlDataReader reader)
        {
            return new MedicineCategory
            {
                Id = (int)reader["Id"],
                Name = (string)reader["Name"],
                Description = reader["Description"] == DBNull.Value ? null : (string)reader["Description"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };
        }
    }
}
