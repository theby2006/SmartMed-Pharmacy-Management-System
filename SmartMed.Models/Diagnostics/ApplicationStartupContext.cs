namespace SmartMed.Models.Diagnostics
{
    public class ApplicationStartupContext
    {
        public string ApplicationName { get; set; }

        public string DataProviderName { get; set; }

        public string PrescriptionUploadRootPath { get; set; }

        public string ReportExportRootPath { get; set; }

        public int DefaultLowStockThreshold { get; set; }

        public int NearExpiryThresholdDays { get; set; }

        public int SessionTimeoutMinutes { get; set; }
    }
}
