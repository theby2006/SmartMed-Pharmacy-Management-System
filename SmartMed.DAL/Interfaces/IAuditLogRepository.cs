namespace SmartMed.DAL.Interfaces
{
    public interface IAuditLogRepository
    {
        void LogLogin(int userId, string username, string machineName);

        void LogLogout(int? userId, string username, string machineName);

        void LogFailedAttempt(string username, string machineName, string details);
    }
}
