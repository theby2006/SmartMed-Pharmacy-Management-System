using SmartMed.BLL.Interfaces;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Diagnostics;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class StartupDiagnosticsService : IStartupDiagnosticsService
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public StartupDiagnosticsService(IDbConnectionFactory dbConnectionFactory)
        {
            Guard.AgainstNull(dbConnectionFactory, nameof(dbConnectionFactory));
            _dbConnectionFactory = dbConnectionFactory;
        }

        public OperationResult<ApplicationStartupContext> BuildContext()
        {
            var startupContext = new ApplicationStartupContext
            {
                ApplicationName = "SmartMed Pharmacy Management System",
                DataProviderName = _dbConnectionFactory.CreateConnection().GetType().Name,
                PrescriptionUploadRootPath = AppSettings.GetRequiredString(ConfigKeys.PrescriptionUploadRootPath),
                ReportExportRootPath = AppSettings.GetRequiredString(ConfigKeys.ReportExportRootPath),
                DefaultLowStockThreshold = AppSettings.GetRequiredInt(ConfigKeys.DefaultLowStockThreshold),
                NearExpiryThresholdDays = AppSettings.GetRequiredInt(ConfigKeys.NearExpiryThresholdDays),
                SessionTimeoutMinutes = AppSettings.GetRequiredInt(ConfigKeys.SessionTimeoutMinutes)
            };

            return OperationResult<ApplicationStartupContext>.Success(
                startupContext,
                "Application foundation services were initialized successfully.");
        }
    }
}
