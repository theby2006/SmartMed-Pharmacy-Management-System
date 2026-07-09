-- ============================================
-- Module 9: Customer Domain
-- Script: 002_CreateCustomersModule.sql
-- Description: Creates the Customers table backing customer registration,
--              customer lookup login, and order placement. Distinct from
--              Users: customers are identified by phone/email rather than
--              a staff username, and carry profile fields (address, city)
--              that staff accounts do not need.
-- ============================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Customers' AND xtype = 'U')
BEGIN
    CREATE TABLE Customers (
        Id            INT             IDENTITY(1,1) NOT NULL,
        FullName      NVARCHAR(200)   NOT NULL,
        PhoneNumber   NVARCHAR(20)    NOT NULL,
        Email         NVARCHAR(200)   NULL,
        PinHash       NVARCHAR(200)   NULL,
        PinSalt       NVARCHAR(200)   NULL,
        Address       NVARCHAR(500)   NULL,
        City          NVARCHAR(100)   NULL,
        IsActive      BIT             NOT NULL DEFAULT 1,
        CreatedDate   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate   DATETIME2       NULL,
        CONSTRAINT PK_Customers PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Customers_PhoneNumber UNIQUE (PhoneNumber),
        CONSTRAINT UQ_Customers_Email UNIQUE (Email)
    );

    CREATE NONCLUSTERED INDEX IX_Customers_IsActive ON Customers (IsActive);
END
GO

-- ============================================
-- Seed data: sample customers.
-- PIN hashes generated with the same PBKDF2-HMAC-SHA256 algorithm as
-- SmartMed.BLL.Services.PasswordHasher. PIN login only applies when
-- CustomerPinEnabled=true in App.config (default is false, so identifier
-- alone is sufficient to sign in as one of these sample customers).
-- ============================================
IF NOT EXISTS (SELECT * FROM Customers WHERE PhoneNumber = '0771234567')
BEGIN
    -- PIN 1234
    INSERT INTO Customers (FullName, PhoneNumber, Email, PinHash, PinSalt, Address, City)
    VALUES ('Jane Doe', '0771234567', 'jane.doe@example.com', 'mUGxXj9y/FuQpEahQnT7eK19N8nkyUwqYB8V+on/vow=', '/kjxaSSG0AwH6+J2KYOCGg==', '12 Lakeview Road', 'Colombo');

    -- PIN 5678
    INSERT INTO Customers (FullName, PhoneNumber, Email, PinHash, PinSalt, Address, City)
    VALUES ('John Smith', '0779876543', 'john.smith@example.com', '9f1QfpRJbm3WPtaumEdD/7BiMoq/xrZ7IQh/Rk/Kp6I=', 'j97aJkhX/ZcleLJ/1Rjkyw==', '45 Hilltop Avenue', 'Kandy');

    -- PIN 4321
    INSERT INTO Customers (FullName, PhoneNumber, Email, PinHash, PinSalt, Address, City)
    VALUES ('Amara Perera', '0712345678', 'amara.perera@example.com', 'eqifG8S1lOEnw3CaRKYexocBXi71D0a3hCRLF2LWMnc=', 'IRJWrfWnDpWLmGTuwR/Gmw==', '7 Galle Road', 'Galle');
END
GO
