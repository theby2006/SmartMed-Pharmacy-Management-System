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
            string rawConfigConnectionString =
                System.Configuration.ConfigurationManager.ConnectionStrings["SmartMedDb"]?.ConnectionString
                ?? "(no 'SmartMedDb' entry found in ConfigurationManager.ConnectionStrings)";
            System.Diagnostics.Debug.WriteLine(
                $"[CONN-DEBUG] SqlConnectionFactory.CreateConnection(): before 'new SqlConnection(...)'");
            System.Diagnostics.Debug.WriteLine(
                $"[CONN-DEBUG]   _connectionString (passed to factory) = '{_connectionString}'");
            System.Diagnostics.Debug.WriteLine(
                $"[CONN-DEBUG]   ConfigurationManager.ConnectionStrings[\"SmartMedDb\"] = '{rawConfigConnectionString}'");

            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);

                // TEMPORARY DIAGNOSTIC — remove after diagnosing
                System.Diagnostics.Debug.WriteLine(
                    $"[CONN-DEBUG] SqlConnectionFactory.CreateConnection(): after 'new SqlConnection(...)'");
                System.Diagnostics.Debug.WriteLine(
                    $"[CONN-DEBUG]   connection.ConnectionString = '{connection.ConnectionString}'");
                System.Diagnostics.Debug.WriteLine(
                    $"[CONN-DEBUG]   connection.DataSource = '{connection.DataSource}'");
                System.Diagnostics.Debug.WriteLine(
                    $"[CONN-DEBUG]   connection.Database = '{connection.Database}'");
                System.Diagnostics.Debug.WriteLine(
                    $"[CONN-DEBUG]   connection.State = {connection.State}");

                return connection;
            }
            catch (SqlException exception)
            {
                // TEMPORARY DIAGNOSTIC — remove after diagnosing
                System.Diagnostics.Debug.WriteLine(
                    $"[CONN-DEBUG] SqlConnectionFactory.CreateConnection(): SqlException Number={exception.Number}, " +
                    $"State={exception.State}, Class={exception.Class}, Message='{exception.Message}'");

                throw new DataAccessException("Failed to create a SQL Server connection.", exception);
            }
        }
    }
}
