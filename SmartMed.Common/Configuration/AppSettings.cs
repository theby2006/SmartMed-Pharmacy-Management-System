using System.Configuration;
using SmartMed.Common.Helpers;

namespace SmartMed.Common.Configuration
{
    public static class AppSettings
    {
        public static string GetRequiredString(string key)
        {
            Guard.AgainstNullOrWhiteSpace(key, nameof(key));

            string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exceptions.ConfigurationException($"Missing required application setting '{key}'.");
            }

            return value.Trim();
        }

        public static int GetRequiredInt(string key)
        {
            string value = GetRequiredString(key);
            if (!int.TryParse(value, out int parsedValue))
            {
                throw new Exceptions.ConfigurationException(
                    $"Application setting '{key}' must be a valid integer. Current value: '{value}'.");
            }

            return parsedValue;
        }

        public static bool GetRequiredBool(string key)
        {
            string value = GetRequiredString(key);
            if (!bool.TryParse(value, out bool parsedValue))
            {
                throw new Exceptions.ConfigurationException(
                    $"Application setting '{key}' must be a valid boolean. Current value: '{value}'.");
            }

            return parsedValue;
        }

        public static string GetConnectionString(string connectionStringName)
        {
            Guard.AgainstNullOrWhiteSpace(connectionStringName, nameof(connectionStringName));

            ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionString == null || string.IsNullOrWhiteSpace(connectionString.ConnectionString))
            {
                throw new Exceptions.ConfigurationException(
                    $"Missing required connection string '{connectionStringName}'.");
            }

            return connectionString.ConnectionString.Trim();
        }
    }
}
