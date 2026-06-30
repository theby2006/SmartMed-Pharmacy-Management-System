namespace SmartMed.Common.Constants
{
    public static class ConfigKeys
    {
        public const string PrimaryConnectionStringName = "SmartMedDb";
        public const string PrescriptionUploadRootPath = "PrescriptionUploadRootPath";
        public const string ReportExportRootPath = "ReportExportRootPath";
        public const string DefaultLowStockThreshold = "DefaultLowStockThreshold";
        public const string NearExpiryThresholdDays = "NearExpiryThresholdDays";
        public const string SessionTimeoutMinutes = "SessionTimeoutMinutes";
        public const string HashIterations = "HashIterations";
        public const string MaxFailedLoginAttempts = "MaxFailedLoginAttempts";
        public const string LockoutDurationMinutes = "LockoutDurationMinutes";
        public const string CustomerPinEnabled = "CustomerPinEnabled";
    }
}
