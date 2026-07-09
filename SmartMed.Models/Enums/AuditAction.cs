namespace SmartMed.Models.Enums
{
    public enum AuditAction
    {
        Login = 1,
        Logout = 2,
        FailedLogin = 3,
        SupplierAdded = 4,
        SupplierUpdated = 5,
        SupplierDeleted = 6,
        SaleCreated = 7,
        SaleCompleted = 8,
        SaleCancelled = 9,
        UserCreated = 10,
        UserUpdated = 11,
        UserActivated = 12,
        UserDeactivated = 13,
        PasswordReset = 14,
        SettingUpdated = 15,
        BackupCreated = 16,
        DatabaseRestored = 17,
        SystemHealthCheck = 18,
        MaintenancePerformed = 19,
        CustomerRegistered = 20,
        CustomerUpdated = 21,
        CustomerDeactivated = 22,
        OrderPlaced = 23,
        OrderStatusChanged = 24,
        OrderCancelled = 25,
        PrescriptionUploaded = 26
    }
}
