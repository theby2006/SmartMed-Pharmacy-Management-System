using System.Data.SqlClient;
using System.Diagnostics;

namespace SmartMed.DAL.Infrastructure
{
    /// <summary>
    /// TEMPORARY DIAGNOSTIC helper — delete this whole file once the
    /// customer/admin login SqlException root cause is confirmed and fixed.
    /// Centralizes the verbose SqlException field dump used by
    /// CustomerRepository.GetByPhoneOrEmail and UserRepository.GetByUsername
    /// during live debugging.
    /// </summary>
    internal static class SqlDiagnostics
    {
        public static void LogSqlException(string tag, SqlException exception, string dataSource, string database)
        {
            Debug.WriteLine($"[{tag}] *** SqlException caught ***");
            Debug.WriteLine($"[{tag}] Number={exception.Number}");
            Debug.WriteLine($"[{tag}] Message='{exception.Message}'");
            Debug.WriteLine($"[{tag}] State={exception.State}");
            Debug.WriteLine($"[{tag}] LineNumber={exception.LineNumber}");
            Debug.WriteLine($"[{tag}] Class={exception.Class}");
            Debug.WriteLine($"[{tag}] Procedure='{exception.Procedure}'");
            Debug.WriteLine($"[{tag}] Server='{exception.Server}'");
            Debug.WriteLine($"[{tag}] DataSource='{dataSource}'");
            Debug.WriteLine($"[{tag}] Database='{database}'");
            Debug.WriteLine($"[{tag}] StackTrace={exception.StackTrace}");
        }
    }
}
