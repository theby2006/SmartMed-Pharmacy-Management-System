using System;
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
