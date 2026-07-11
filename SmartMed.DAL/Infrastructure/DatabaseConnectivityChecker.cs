using System.Data.SqlClient;
using System.Diagnostics;
using SmartMed.Common.Exceptions;
using SmartMed.DAL.Interfaces;

namespace SmartMed.DAL.Infrastructure
{
    /// <summary>
    /// Verifies SQL Server connectivity once at application startup, before
    /// the login form is shown. A failure here is surfaced as a friendly
    /// <see cref="DataAccessException"/> (caught by Program.cs's existing
    /// AppException handler and shown as a plain-language MessageBox) rather
    /// than letting a raw SqlException reach the user mid-login.
    /// </summary>
    public static class DatabaseConnectivityChecker
    {
        public static void VerifyConnection(IDbConnectionFactory connectionFactory)
        {
            try
            {
                using (SqlConnection connection = connectionFactory.CreateConnection())
                {
                    connection.Open();

                    Debug.WriteLine(
                        $"[Startup] Connected to SQL Server. DataSource='{connection.DataSource}', " +
                        $"Database='{connection.Database}'.");
                }
            }
            catch (SqlException exception)
            {
                throw new DataAccessException(
                    "Unable to connect to the SmartMed database. Please verify that SQL Server is " +
                    "running and that the connection string in App.config points to the correct server instance.",
                    exception);
            }
        }
    }
}
