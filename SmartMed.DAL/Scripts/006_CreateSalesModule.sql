-- ============================================
-- Module 6: Sales & Billing Management
-- Script: 006_CreateSalesModule.sql
-- Description: Creates Sales, SaleItems, and Payments tables
-- ============================================

-- ============================================
-- 1. Sales Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Sales' AND xtype = 'U')
BEGIN
    CREATE TABLE Sales (
        Id INT IDENTITY(1,1) NOT NULL,
        SaleNumber NVARCHAR(50) NOT NULL,
        SaleDate DATETIME2 NOT NULL,
        CashierId INT NOT NULL,
        CustomerName NVARCHAR(200) NULL,
        CustomerPhone NVARCHAR(50) NULL,
        DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
        TaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
        SubTotal DECIMAL(18,2) NOT NULL,
        DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        GrandTotal DECIMAL(18,2) NOT NULL,
        Status TINYINT NOT NULL DEFAULT 1,
        Notes NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT PK_Sales PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Sales_SaleNumber UNIQUE (SaleNumber),
        CONSTRAINT FK_Sales_CashierId FOREIGN KEY (CashierId) REFERENCES Users(Id),
        CONSTRAINT CK_Sales_DiscountPercent CHECK (DiscountPercent >= 0 AND DiscountPercent <= 100),
        CONSTRAINT CK_Sales_TaxPercent CHECK (TaxPercent >= 0 AND TaxPercent <= 100),
        CONSTRAINT CK_Sales_Status CHECK (Status >= 1 AND Status <= 3)
    );

    CREATE NONCLUSTERED INDEX IX_Sales_SaleDate ON Sales (SaleDate);
    CREATE NONCLUSTERED INDEX IX_Sales_CashierId ON Sales (CashierId);
    CREATE NONCLUSTERED INDEX IX_Sales_Status ON Sales (Status);
END

-- ============================================
-- 2. SaleItems Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'SaleItems' AND xtype = 'U')
BEGIN
    CREATE TABLE SaleItems (
        Id INT IDENTITY(1,1) NOT NULL,
        SaleId INT NOT NULL,
        MedicineId INT NOT NULL,
        StockBatchId INT NOT NULL,
        BatchNumber NVARCHAR(100) NOT NULL,
        ExpiryDate DATETIME2 NOT NULL,
        Quantity INT NOT NULL,
        SellingPrice DECIMAL(18,2) NOT NULL,
        DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
        TaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
        LineTotal DECIMAL(18,2) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT PK_SaleItems PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_SaleItems_SaleId FOREIGN KEY (SaleId) REFERENCES Sales(Id),
        CONSTRAINT FK_SaleItems_MedicineId FOREIGN KEY (MedicineId) REFERENCES Medicines(Id),
        CONSTRAINT FK_SaleItems_StockBatchId FOREIGN KEY (StockBatchId) REFERENCES StockBatches(Id),
        CONSTRAINT CK_SaleItems_Quantity CHECK (Quantity > 0),
        CONSTRAINT CK_SaleItems_SellingPrice CHECK (SellingPrice > 0),
        CONSTRAINT CK_SaleItems_DiscountPercent CHECK (DiscountPercent >= 0 AND DiscountPercent <= 100),
        CONSTRAINT CK_SaleItems_TaxPercent CHECK (TaxPercent >= 0 AND TaxPercent <= 100)
    );

    CREATE NONCLUSTERED INDEX IX_SaleItems_SaleId ON SaleItems (SaleId);
    CREATE NONCLUSTERED INDEX IX_SaleItems_MedicineId ON SaleItems (MedicineId);
    CREATE NONCLUSTERED INDEX IX_SaleItems_StockBatchId ON SaleItems (StockBatchId);
END

-- ============================================
-- 3. Payments Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Payments' AND xtype = 'U')
BEGIN
    CREATE TABLE Payments (
        Id INT IDENTITY(1,1) NOT NULL,
        SaleId INT NOT NULL,
        PaymentMethod TINYINT NOT NULL DEFAULT 1,
        AmountPaid DECIMAL(18,2) NOT NULL,
        ChangeAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        PaymentStatus TINYINT NOT NULL DEFAULT 2,
        TransactionReference NVARCHAR(100) NULL,
        ProcessedByUserId INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT PK_Payments PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Payments_SaleId FOREIGN KEY (SaleId) REFERENCES Sales(Id),
        CONSTRAINT FK_Payments_ProcessedByUserId FOREIGN KEY (ProcessedByUserId) REFERENCES Users(Id),
        CONSTRAINT CK_Payments_AmountPaid CHECK (AmountPaid >= 0),
        CONSTRAINT CK_Payments_ChangeAmount CHECK (ChangeAmount >= 0),
        CONSTRAINT CK_Payments_PaymentMethod CHECK (PaymentMethod >= 1 AND PaymentMethod <= 4),
        CONSTRAINT CK_Payments_PaymentStatus CHECK (PaymentStatus >= 1 AND PaymentStatus <= 3)
    );

    CREATE NONCLUSTERED INDEX IX_Payments_SaleId ON Payments (SaleId);
END
