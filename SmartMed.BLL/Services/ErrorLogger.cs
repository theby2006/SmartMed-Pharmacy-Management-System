using System;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;

namespace SmartMed.BLL.Services
{
    public class ErrorLogger : ILogger
    {
        private readonly IErrorLogRepository _errorLogRepository;

        public ErrorLogger(IErrorLogRepository errorLogRepository)
        {
            Guard.AgainstNull(errorLogRepository, nameof(errorLogRepository));
            _errorLogRepository = errorLogRepository;
        }

        public void LogInfo(string message, string source = null)
        {
            Log("Info", message, null, source);
        }

        public void LogWarning(string message, string source = null)
        {
            Log("Warning", message, null, source);
        }

        public void LogError(string message, Exception exception = null, string source = null)
        {
            Log("Error", message, exception, source);
        }

        public void LogDebug(string message, string source = null)
        {
            Log("Debug", message, null, source);
        }

        public int GetErrorCountSince(DateTime since)
        {
            try
            {
                return _errorLogRepository.GetErrorCountSince(since);
            }
            catch
            {
                return 0;
            }
        }

        private void Log(string level, string message, Exception exception, string source)
        {
            try
            {
                ErrorLog entry = new ErrorLog
                {
                    Level = level,
                    Message = message,
                    Exception = exception?.ToString(),
                    Source = source ?? (exception?.Source),
                    StackTrace = exception?.StackTrace,
                    MachineName = Environment.MachineName,
                    CreatedDate = DateTime.UtcNow
                };
                _errorLogRepository.Add(entry);
            }
            catch
            {
            }
        }
    }
}
