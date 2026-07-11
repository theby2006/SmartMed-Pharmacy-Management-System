using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;

namespace SmartMed.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public User GetById(int userId)
        {
            const string sql = "SELECT Id, Username, PasswordHash, PasswordSalt, DisplayName, Role, Email, " +
                               "FailedLoginAttempts, LockedUntil, LastLogin, IsActive, CreatedDate, UpdatedDate " +
                               "FROM Users WHERE Id = @Id";

            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapUser(reader);
                        }
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve user by Id.", exception);
            }

            return null;
        }

        public User GetByUsername(string username)
        {
            Guard.AgainstNullOrWhiteSpace(username, nameof(username));

            const string sql = "SELECT Id, Username, PasswordHash, PasswordSalt, DisplayName, Role, Email, " +
                               "FailedLoginAttempts, LockedUntil, LastLogin, IsActive, CreatedDate, UpdatedDate " +
                               "FROM Users WHERE Username = @Username";

            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapUser(reader);
                        }
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve user by username.", exception);
            }

            return null;
        }

        public void IncrementFailedAttempts(int userId)
        {
            const string sql = "UPDATE Users SET FailedLoginAttempts = FailedLoginAttempts + 1 WHERE Id = @Id";

            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to increment failed login attempts.", exception);
            }
        }

        public void ResetFailedAttempts(int userId)
        {
            const string sql = "UPDATE Users SET FailedLoginAttempts = 0, LockedUntil = NULL WHERE Id = @Id";

            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to reset failed login attempts.", exception);
            }
        }

        public void SetLockedUntil(int userId, DateTime? lockedUntil)
        {
            const string sql = "UPDATE Users SET LockedUntil = @LockedUntil WHERE Id = @Id";

            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);
                    command.Parameters.AddWithValue("@LockedUntil", (object)lockedUntil ?? DBNull.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to set locked until date.", exception);
            }
        }

        public void UpdateLastLogin(int userId, DateTime loginTime)
        {
            const string sql = "UPDATE Users SET LastLogin = @LastLogin WHERE Id = @Id";

            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);
                    command.Parameters.AddWithValue("@LastLogin", loginTime);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update last login time.", exception);
            }
        }

        public List<User> GetAll()
        {
            const string sql = "SELECT Id, Username, PasswordHash, PasswordSalt, DisplayName, Role, Email, " +
                               "FailedLoginAttempts, LockedUntil, LastLogin, IsActive, CreatedDate, UpdatedDate " +
                               "FROM Users ORDER BY DisplayName";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<User> users = new List<User>();
                        while (reader.Read())
                            users.Add(MapUser(reader));
                        return users;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to retrieve all users.", exception);
            }
        }

        public List<User> Search(string keyword)
        {
            Guard.AgainstNullOrWhiteSpace(keyword, nameof(keyword));
            const string sql = "SELECT Id, Username, PasswordHash, PasswordSalt, DisplayName, Role, Email, " +
                               "FailedLoginAttempts, LockedUntil, LastLogin, IsActive, CreatedDate, UpdatedDate " +
                               "FROM Users WHERE DisplayName LIKE @Keyword OR Username LIKE @Keyword OR Email LIKE @Keyword ORDER BY DisplayName";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<User> users = new List<User>();
                        while (reader.Read())
                            users.Add(MapUser(reader));
                        return users;
                    }
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to search users.", exception);
            }
        }

        public int Add(User user)
        {
            Guard.AgainstNull(user, nameof(user));
            const string sql = "INSERT INTO Users (Username, PasswordHash, PasswordSalt, DisplayName, Role, Email, IsActive, CreatedDate) " +
                               "VALUES (@Username, @PasswordHash, @PasswordSalt, @DisplayName, @Role, @Email, 1, GETUTCDATE()); SELECT CAST(SCOPE_IDENTITY() AS INT)";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@PasswordSalt", user.PasswordSalt);
                    command.Parameters.AddWithValue("@DisplayName", user.DisplayName);
                    command.Parameters.AddWithValue("@Role", (int)user.Role);
                    command.Parameters.AddWithValue("@Email", (object)user.Email ?? DBNull.Value);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to add user.", exception);
            }
        }

        public void Update(User user)
        {
            Guard.AgainstNull(user, nameof(user));
            const string sql = "UPDATE Users SET DisplayName = @DisplayName, Role = @Role, Email = @Email, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", user.Id);
                    command.Parameters.AddWithValue("@DisplayName", user.DisplayName);
                    command.Parameters.AddWithValue("@Role", (int)user.Role);
                    command.Parameters.AddWithValue("@Email", (object)user.Email ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update user.", exception);
            }
        }

        public void UpdatePassword(int userId, string passwordHash, string passwordSalt)
        {
            const string sql = "UPDATE Users SET PasswordHash = @PasswordHash, PasswordSalt = @PasswordSalt, FailedLoginAttempts = 0, LockedUntil = NULL, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    command.Parameters.AddWithValue("@PasswordSalt", passwordSalt);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update password.", exception);
            }
        }

        public void UpdateActiveStatus(int userId, bool isActive)
        {
            const string sql = "UPDATE Users SET IsActive = @IsActive, UpdatedDate = GETUTCDATE() WHERE Id = @Id";
            try
            {
                using (SqlConnection connection = _connectionFactory.CreateConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);
                    command.Parameters.AddWithValue("@IsActive", isActive);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to update user active status.", exception);
            }
        }

        private static User MapUser(SqlDataReader reader)
        {
            User user = new User
            {
                Id = (int)reader["Id"],
                Username = (string)reader["Username"],
                PasswordHash = (string)reader["PasswordHash"],
                PasswordSalt = (string)reader["PasswordSalt"],
                DisplayName = (string)reader["DisplayName"],
                Role = (RoleType)(int)reader["Role"],
                Email = reader["Email"] == DBNull.Value ? null : (string)reader["Email"],
                FailedLoginAttempts = (int)reader["FailedLoginAttempts"],
                LockedUntil = reader["LockedUntil"] == DBNull.Value ? null : (DateTime?)reader["LockedUntil"],
                LastLogin = reader["LastLogin"] == DBNull.Value ? null : (DateTime?)reader["LastLogin"],
                IsActive = (bool)reader["IsActive"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDate"]
            };

            return user;
        }
    }
}
