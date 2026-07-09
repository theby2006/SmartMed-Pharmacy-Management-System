using System;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Session;

namespace SmartMed.BLL.Services
{
    public class SessionManager : ISessionManager
    {
        private readonly object _lock = new object();
        private readonly int _sessionTimeoutMinutes;
        private SessionContext _currentSession;

        public SessionManager()
        {
            _sessionTimeoutMinutes = AppSettings.GetRequiredInt(ConfigKeys.SessionTimeoutMinutes);
        }

        public SessionContext StartSession(User user)
        {
            lock (_lock)
            {
                DateTime utcNow = DateTime.UtcNow;

                _currentSession = new SessionContext
                {
                    UserId = user.Id,
                    CustomerId = null,
                    Username = user.Username,
                    DisplayName = user.DisplayName,
                    Role = user.Role,
                    LoginTimeUtc = utcNow,
                    LastActivityTimeUtc = utcNow
                };

                return _currentSession;
            }
        }

        public SessionContext StartCustomerSession(Customer customer)
        {
            lock (_lock)
            {
                DateTime utcNow = DateTime.UtcNow;

                _currentSession = new SessionContext
                {
                    UserId = null,
                    CustomerId = customer.Id,
                    Username = customer.PhoneNumber,
                    DisplayName = customer.FullName,
                    Role = RoleType.Customer,
                    LoginTimeUtc = utcNow,
                    LastActivityTimeUtc = utcNow
                };

                return _currentSession;
            }
        }

        public void EndSession()
        {
            lock (_lock)
            {
                _currentSession = null;
            }
        }

        public SessionContext CurrentSession
        {
            get
            {
                lock (_lock)
                {
                    if (_currentSession == null)
                    {
                        return null;
                    }

                    if ((DateTime.UtcNow - _currentSession.LastActivityTimeUtc).TotalMinutes > _sessionTimeoutMinutes)
                    {
                        _currentSession = null;
                        return null;
                    }

                    _currentSession.LastActivityTimeUtc = DateTime.UtcNow;
                    return _currentSession;
                }
            }
        }

        public bool IsActive
        {
            get { return CurrentSession != null; }
        }

        public bool HasRole(RoleType role)
        {
            SessionContext session = CurrentSession;
            return session != null && session.Role == role;
        }
    }
}
