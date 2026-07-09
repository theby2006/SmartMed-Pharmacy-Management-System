-- ============================================
-- Module 1: Users & Audit Log
-- Script: 001_CreateUsersModule.sql
-- Description: Creates the Users and AuditLogs tables that every other
--              module's foreign keys (Sales.CashierId, Purchases.CreatedByUserId,
--              Payments.ProcessedByUserId, StockMovements.CreatedByUserId,
--              UserPreferences.UserId, BackupHistory.CreatedByUserId) depend on.
--              Column layout matches SmartMed.Models.Entities.User and
--              SmartMed.Models.Entities.AuditLogEntry exactly.
-- ============================================

-- ============================================
-- 1. Users Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Users' AND xtype = 'U')
BEGIN
    CREATE TABLE Users (
        Id                   INT             IDENTITY(1,1) NOT NULL,
        Username             NVARCHAR(100)   NOT NULL,
        PasswordHash         NVARCHAR(200)   NOT NULL,
        PasswordSalt         NVARCHAR(200)   NOT NULL,
        DisplayName          NVARCHAR(200)   NOT NULL,
        Role                 TINYINT         NOT NULL,
        Email                NVARCHAR(200)   NULL,
        FailedLoginAttempts  INT             NOT NULL DEFAULT 0,
        LockedUntil          DATETIME2       NULL,
        LastLogin            DATETIME2       NULL,
        IsActive             BIT             NOT NULL DEFAULT 1,
        CreatedDate          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate          DATETIME2       NULL,
        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Users_Username UNIQUE (Username),
        CONSTRAINT CK_Users_Role CHECK (Role BETWEEN 1 AND 4),
        CONSTRAINT CK_Users_FailedLoginAttempts CHECK (FailedLoginAttempts >= 0)
    );

    CREATE NONCLUSTERED INDEX IX_Users_Role ON Users (Role);
    CREATE NONCLUSTERED INDEX IX_Users_IsActive ON Users (IsActive);
END
GO

-- ============================================
-- 2. AuditLogs Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AuditLogs' AND xtype = 'U')
BEGIN
    CREATE TABLE AuditLogs (
        Id           INT             IDENTITY(1,1) NOT NULL,
        UserId       INT             NULL,
        Username     NVARCHAR(100)   NOT NULL,
        Action       TINYINT         NOT NULL,
        MachineName  NVARCHAR(100)   NULL,
        Timestamp    DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        Details      NVARCHAR(1000)  NULL,
        CONSTRAINT PK_AuditLogs PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_AuditLogs_UserId FOREIGN KEY (UserId) REFERENCES Users(Id)
    );

    CREATE NONCLUSTERED INDEX IX_AuditLogs_UserId ON AuditLogs (UserId);
    CREATE NONCLUSTERED INDEX IX_AuditLogs_Timestamp ON AuditLogs (Timestamp);
    CREATE NONCLUSTERED INDEX IX_AuditLogs_Action ON AuditLogs (Action);
END
GO

-- ============================================
-- Seed data: one account per staff role.
-- Password hashes were generated with the same algorithm as
-- SmartMed.BLL.Services.PasswordHasher (PBKDF2-HMAC-SHA256, 600,000
-- iterations, 32-byte derived key, base64-encoded salt/hash), so these
-- rows authenticate correctly against the real application on first run.
-- Plaintext credentials for reviewers are documented in README.md.
-- ============================================
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    -- admin / Admin@123 (RoleType.Administrator = 1)
    INSERT INTO Users (Username, PasswordHash, PasswordSalt, DisplayName, Role, Email)
    VALUES ('admin', '5LzVSKNOKHdeIhVGjCnVST3WurlXgdGrF8+ENpxf+i0=', 'CYbsmEvEAfljv2KKX0FWdw==', 'System Administrator', 1, 'admin@smartmed.local');

    -- pharmacist / Pharm@123 (RoleType.Pharmacist = 2)
    INSERT INTO Users (Username, PasswordHash, PasswordSalt, DisplayName, Role, Email)
    VALUES ('pharmacist', 'YEpPrpBgQw3Fy/EwFgx75vFTk/EPwlzN42RHEbFwuTM=', 'woc+rxF4+LCCALctD02E7Q==', 'Staff Pharmacist', 2, 'pharmacist@smartmed.local');

    -- cashier / Cash@123 (RoleType.Cashier = 3)
    INSERT INTO Users (Username, PasswordHash, PasswordSalt, DisplayName, Role, Email)
    VALUES ('cashier', 'uzdMqCDkL0s+Oh3kOcCO8u5ehVTbhGje5CXtMcOFUjY=', '40vi+cUflQD7yQKUtmXsWQ==', 'Front Desk Cashier', 3, 'cashier@smartmed.local');
END
GO
