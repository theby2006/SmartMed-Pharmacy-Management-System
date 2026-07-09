using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;

namespace SmartMed.DAL.Infrastructure
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            Guard.AgainstNullOrWhiteSpace(connectionString, nameof(connectionString));
            _connectionString = connectionString;
        }

        public SqlConnection CreateConnection()
        {
            // TEMPORARY DIAGNOSTIC — remove after diagnosing
            System.Diagnostics.Debug.WriteLine(
                $"[CUSTOMER-LOOKUP-DEBUG] SqlConnectionFactory.CreateConnection(): before 'new SqlConnection(...)', " +
                $"connectionString='{_connectionString}'");

            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);

                // TEMPORARY DIAGNOSTIC — remove after diagnosing
                System.Diagnostics.Debug.WriteLine(
                    $"[CUSTOMER-LOOKUP-DEBUG] SqlConnectionFactory.CreateConnection(): after 'new SqlConnection(...)', " +
                    $"DataSource='{connection.DataSource}', Database='{connection.Database}', State={connection.State}");

                return connection;
            }
            catch (SqlException exception)
            {
                // TEMPORARY DIAGNOSTIC — remove after diagnosing
                System.Diagnostics.Debug.WriteLine(
                    $"[CUSTOMER-LOOKUP-DEBUG] SqlConnectionFactory.CreateConnection(): SqlException Number={exception.Number}, Message='{exception.Message}'");

                throw new DataAccessException("Failed to create a SQL Server connection.", exception);
            }
        }
    }
}
