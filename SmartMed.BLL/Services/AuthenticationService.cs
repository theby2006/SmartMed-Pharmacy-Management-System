using System;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Enums;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;
using SmartMed.Models.Session;

namespace SmartMed.BLL.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ISessionManager _sessionManager;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly int _maxFailedAttempts;
        private readonly int _lockoutDurationMinutes;

        public AuthenticationService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ISessionManager sessionManager,
            IAuditLogRepository auditLogRepository)
        {
            Guard.AgainstNull(userRepository, nameof(userRepository));
            Guard.AgainstNull(passwordHasher, nameof(passwordHasher));
            Guard.AgainstNull(sessionManager, nameof(sessionManager));
            Guard.AgainstNull(auditLogRepository, nameof(auditLogRepository));

            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _sessionManager = sessionManager;
            _auditLogRepository = auditLogRepository;
            _maxFailedAttempts = AppSettings.GetRequiredInt(ConfigKeys.MaxFailedLoginAttempts);
            _lockoutDurationMinutes = AppSettings.GetRequiredInt(ConfigKeys.LockoutDurationMinutes);
        }

        public OperationResult<SessionContext> LoginAdmin(string username, string password)
        {
            try
            {
                Guard.AgainstNullOrWhiteSpace(username, nameof(username));
                Guard.AgainstNullOrWhiteSpace(password, nameof(password));
            }
            catch (ValidationException ex)
            {
                return OperationResult<SessionContext>.Failure(ex.Message);
            }

            User user = _userRepository.GetByUsername(username);

            if (user == null)
            {
                string machineName = Environment.MachineName;
                _auditLogRepository.LogFailedAttempt(username, machineName, "Username not found");
                return OperationResult<SessionContext>.Failure("Invalid username or password.");
            }

            if (!user.IsActive)
            {
                string machineName = Environment.MachineName;
                _auditLogRepository.LogFailedAttempt(username, machineName, "Account is disabled");
                return OperationResult<SessionContext>.Failure(
                    "This account has been disabled. Please contact your system administrator.");
            }

            if (user.LockedUntil.HasValue)
            {
                if (user.LockedUntil.Value > DateTime.UtcNow)
                {
                    double remainingMinutes = Math.Ceiling((user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes);
                    return OperationResult<SessionContext>.Failure(
                        $"Account is temporarily locked. Please try again after {(int)remainingMinutes} minute(s).");
                }

                _userRepository.SetLockedUntil(user.Id, null);
                user.LockedUntil = null;
            }

            bool passwordValid = _passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);

            if (!passwordValid)
            {
                string machineName = Environment.MachineName;
                _userRepository.IncrementFailedAttempts(user.Id);
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= _maxFailedAttempts)
                {
                    DateTime lockedUntil = DateTime.UtcNow.AddMinutes(_lockoutDurationMinutes);
                    _userRepository.SetLockedUntil(user.Id, lockedUntil);
                }

                _auditLogRepository.LogFailedAttempt(username, machineName, "Invalid password");
                return OperationResult<SessionContext>.Failure("Invalid username or password.");
            }

            _userRepository.ResetFailedAttempts(user.Id);
            _userRepository.UpdateLastLogin(user.Id, DateTime.UtcNow);

            string machine = Environment.MachineName;
            _auditLogRepository.LogLogin(user.Id, user.Username, machine);

            SessionContext session = _sessionManager.StartSession(user);

            return OperationResult<SessionContext>.Success(session, "Login successful.");
        }

        public OperationResult<SessionContext> LoginCustomer(string identifier, string pin = null)
        {
            try
            {
                Guard.AgainstNullOrWhiteSpace(identifier, nameof(identifier));
            }
            catch (ValidationException ex)
            {
                return OperationResult<SessionContext>.Failure(ex.Message);
            }

            bool customerPinEnabled = AppSettings.GetRequiredBool(ConfigKeys.CustomerPinEnabled);

            if (customerPinEnabled && string.IsNullOrWhiteSpace(pin))
            {
                return OperationResult<SessionContext>.Failure("PIN is required for customer login.");
            }

            if (customerPinEnabled && (pin.Length < 4 || pin.Length > 8 || !IsAllDigits(pin)))
            {
                return OperationResult<SessionContext>.Failure("Invalid PIN format.");
            }

            User user = _userRepository.GetByUsername(identifier);

            if (user == null || user.Role != RoleType.Customer)
            {
                string machineName = Environment.MachineName;
                _auditLogRepository.LogFailedAttempt(identifier, machineName, "Customer not found");
                return OperationResult<SessionContext>.Failure("Customer not found. Please verify the information.");
            }

            if (customerPinEnabled)
            {
                bool pinValid = _passwordHasher.VerifyPassword(pin, user.PasswordHash, user.PasswordSalt);
                if (!pinValid)
                {
                    string machineName = Environment.MachineName;
                    _auditLogRepository.LogFailedAttempt(identifier, machineName, "Invalid customer PIN");
                    return OperationResult<SessionContext>.Failure("Invalid PIN.");
                }
            }

            string machine = Environment.MachineName;
            _auditLogRepository.LogLogin(user.Id, user.Username, machine);

            SessionContext session = _sessionManager.StartSession(user);

            return OperationResult<SessionContext>.Success(session, "Customer login successful.");
        }

        public OperationResult Logout()
        {
            try
            {
                SessionContext session = CurrentSession;

                if (session != null)
                {
                    string machineName = Environment.MachineName;
                    _auditLogRepository.LogLogout(session.UserId, session.Username, machineName);
                }

                _sessionManager.EndSession();

                return OperationResult.Success("Logged out successfully.");
            }
            catch (Exception ex) when (!(ex is AppException))
            {
                return OperationResult.Failure("An error occurred during logout.");
            }
        }

        public bool IsAuthenticated
        {
            get { return _sessionManager.IsActive; }
        }

        public SessionContext CurrentSession
        {
            get { return _sessionManager.CurrentSession; }
        }

        private static bool IsAllDigits(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsDigit(value[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
