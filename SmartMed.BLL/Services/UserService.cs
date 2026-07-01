using System.Collections.Generic;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuditLogRepository _auditLogRepository;

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IAuditLogRepository auditLogRepository)
        {
            Guard.AgainstNull(userRepository, nameof(userRepository));
            Guard.AgainstNull(passwordHasher, nameof(passwordHasher));
            Guard.AgainstNull(auditLogRepository, nameof(auditLogRepository));
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _auditLogRepository = auditLogRepository;
        }

        public OperationResult<List<User>> GetAllUsers()
        {
            try
            {
                return OperationResult<List<User>>.Success(_userRepository.GetAll());
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<User>>.Failure(ex.Message);
            }
        }

        public OperationResult<User> GetUserById(int id)
        {
            try
            {
                User user = _userRepository.GetById(id);
                return user != null
                    ? OperationResult<User>.Success(user)
                    : OperationResult<User>.Failure($"User with ID {id} not found.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult<User>.Failure(ex.Message);
            }
        }

        public OperationResult<List<User>> SearchUsers(string keyword)
        {
            try
            {
                return OperationResult<List<User>>.Success(_userRepository.Search(keyword));
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<User>>.Failure(ex.Message);
            }
        }

        public OperationResult<int> CreateUser(User user, string password)
        {
            try
            {
                if (user == null)
                    return OperationResult<int>.Failure("User information is required.");
                if (string.IsNullOrWhiteSpace(password))
                    return OperationResult<int>.Failure("Password is required.");
                if (string.IsNullOrWhiteSpace(user.Username))
                    return OperationResult<int>.Failure("Username is required.");
                if (string.IsNullOrWhiteSpace(user.DisplayName))
                    return OperationResult<int>.Failure("Display name is required.");

                User existing = _userRepository.GetByUsername(user.Username);
                if (existing != null)
                    return OperationResult<int>.Failure($"Username '{user.Username}' is already taken.");

                string salt = _passwordHasher.GenerateSalt();
                user.PasswordHash = _passwordHasher.HashPassword(password, salt);
                user.PasswordSalt = salt;

                int id = _userRepository.Add(user);
                return OperationResult<int>.Success(id, "User created successfully.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult<int>.Failure(ex.Message);
            }
        }

        public OperationResult UpdateUser(User user)
        {
            try
            {
                if (user == null)
                    return OperationResult.Failure("User information is required.");

                User existing = _userRepository.GetById(user.Id);
                if (existing == null)
                    return OperationResult.Failure($"User with ID {user.Id} not found.");

                _userRepository.Update(user);
                return OperationResult.Success("User updated successfully.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult ResetPassword(int userId, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPassword))
                    return OperationResult.Failure("New password is required.");

                User user = _userRepository.GetById(userId);
                if (user == null)
                    return OperationResult.Failure($"User with ID {userId} not found.");

                string salt = _passwordHasher.GenerateSalt();
                string hash = _passwordHasher.HashPassword(newPassword, salt);
                _userRepository.UpdatePassword(userId, hash, salt);
                return OperationResult.Success("Password reset successfully.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult ActivateUser(int userId)
        {
            try
            {
                User user = _userRepository.GetById(userId);
                if (user == null)
                    return OperationResult.Failure($"User with ID {userId} not found.");
                if (user.IsActive)
                    return OperationResult.Failure("User is already active.");

                _userRepository.UpdateActiveStatus(userId, true);
                _userRepository.ResetFailedAttempts(userId);
                return OperationResult.Success("User activated successfully.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult DeactivateUser(int userId)
        {
            try
            {
                User user = _userRepository.GetById(userId);
                if (user == null)
                    return OperationResult.Failure($"User with ID {userId} not found.");
                if (!user.IsActive)
                    return OperationResult.Failure("User is already deactivated.");

                _userRepository.UpdateActiveStatus(userId, false);
                return OperationResult.Success("User deactivated successfully.");
            }
            catch (DataAccessException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }
    }
}
