using System;

namespace SmartMed.BLL.Interfaces
{
    public interface ILogger
    {
        void LogInfo(string message, string source = null);
        void LogWarning(string message, string source = null);
        void LogError(string message, Exception exception = null, string source = null);
        void LogDebug(string message, string source = null);
        int GetErrorCountSince(DateTime since);
    }
}
