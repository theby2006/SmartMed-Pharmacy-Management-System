-- Module 8: Administration, Security, Backup & Audit
-- Settings table for application and pharmacy configuration

CREATE TABLE Settings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    [Key] NVARCHAR(100) NOT NULL,
    [Value] NVARCHAR(MAX) NULL,
    [Description] NVARCHAR(500) NULL,
    Category NVARCHAR(100) NOT NULL DEFAULT N'General',
    IsSystem BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NULL,
    CONSTRAINT UQ_Settings_Key UNIQUE ([Key])
);

-- Pharmacy Profile settings
INSERT INTO Settings ([Key], [Value], [Description], Category, IsSystem) VALUES
('PharmacyName', 'SmartMed Pharmacy', 'Pharmacy display name', 'Pharmacy', 1),
('PharmacyAddress', '', 'Pharmacy physical address', 'Pharmacy', 1),
('PharmacyPhone', '', 'Pharmacy contact phone number', 'Pharmacy', 1),
('PharmacyEmail', '', 'Pharmacy contact email', 'Pharmacy', 1),
('PharmacyLicenseNumber', '', 'Pharmacy license/registration number', 'Pharmacy', 1),
('PharmacyTaxId', '', 'Pharmacy tax identification number', 'Pharmacy', 1);

-- Security settings
INSERT INTO Settings ([Key], [Value], [Description], Category, IsSystem) VALUES
('MinPasswordLength', '8', 'Minimum required password length', 'Security', 1),
('RequireUppercase', 'true', 'Password must contain uppercase letter', 'Security', 1),
('RequireLowercase', 'true', 'Password must contain lowercase letter', 'Security', 1),
('RequireDigit', 'true', 'Password must contain digit', 'Security', 1),
('RequireSpecialChar', 'true', 'Password must contain special character', 'Security', 1),
('PasswordExpiryDays', '90', 'Days until password expires', 'Security', 1),
('MaxSessionsPerUser', '1', 'Maximum concurrent sessions per user', 'Security', 0),
('SessionIdleTimeoutMinutes', '20', 'Session idle timeout in minutes', 'Security', 1);

-- Notification settings
INSERT INTO Settings ([Key], [Value], [Description], Category, IsSystem) VALUES
('LowStockNotification', 'true', 'Enable low stock notifications', 'Notifications', 0),
('NearExpiryNotification', 'true', 'Enable near expiry notifications', 'Notifications', 0),
('BackupReminderDays', '7', 'Days between backup reminders', 'Notifications', 0);

-- Backup settings
INSERT INTO Settings ([Key], [Value], [Description], Category, IsSystem) VALUES
('BackupDirectory', 'App_Data\Backups', 'Default backup directory path', 'Backup', 0),
('AutoBackupEnabled', 'false', 'Enable automatic backups', 'Backup', 0),
('AutoBackupIntervalHours', '24', 'Hours between automatic backups', 'Backup', 0),
('MaxBackupFiles', '30', 'Maximum backup files to retain', 'Backup', 0);

-- BackupHistory table
CREATE TABLE BackupHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FileName NVARCHAR(500) NOT NULL,
    FilePath NVARCHAR(1000) NOT NULL,
    FileSizeBytes BIGINT NOT NULL DEFAULT 0,
    [DatabaseName] NVARCHAR(100) NOT NULL,
    BackupType NVARCHAR(50) NOT NULL DEFAULT N'Full',
    [Status] NVARCHAR(50) NOT NULL DEFAULT N'Completed',
    [ErrorMessage] NVARCHAR(MAX) NULL,
    CreatedByUserId INT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_BackupHistory_Users FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

-- UserPreferences table
CREATE TABLE UserPreferences (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    [Key] NVARCHAR(100) NOT NULL,
    [Value] NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NULL,
    CONSTRAINT UQ_UserPreferences_UserKey UNIQUE (UserId, [Key]),
    CONSTRAINT FK_UserPreferences_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- ErrorLog table for ILogger
CREATE TABLE ErrorLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    [Level] NVARCHAR(50) NOT NULL DEFAULT N'Error',
    [Message] NVARCHAR(MAX) NOT NULL,
    [Exception] NVARCHAR(MAX) NULL,
    [Source] NVARCHAR(500) NULL,
    [StackTrace] NVARCHAR(MAX) NULL,
    MachineName NVARCHAR(100) NULL,
    UserId INT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- PerformanceLogs table
CREATE TABLE PerformanceLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OperationName NVARCHAR(200) NOT NULL,
    DurationMs INT NOT NULL,
    IsSlow BIT NOT NULL DEFAULT 0,
    MachineName NVARCHAR(100) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
