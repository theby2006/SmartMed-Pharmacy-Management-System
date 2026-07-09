-- ============================================
-- Module 9: Customer Domain
-- Script: 007_CreateOrdersModule.sql
-- Description: Creates Orders and OrderItems, backing customer order
--              placement and the admin order-management workflow. Status
--              values map to SmartMed.Models.Enums.OrderStatus:
--              1=Pending, 2=PrescriptionReviewRequired, 3=Approved,
--              4=Processing, 5=Completed, 6=Cancelled, 7=Rejected.
--              Must run after 002_CreateCustomersModule.sql and
--              Module3_DDL.sql (Medicines).
-- ============================================

-- ============================================
-- 1. Orders Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Orders' AND xtype = 'U')
BEGIN
    CREATE TABLE Orders (
        Id                    INT             IDENTITY(1,1) NOT NULL,
        OrderNumber           NVARCHAR(50)    NOT NULL,
        CustomerId            INT             NOT NULL,
        OrderDate             DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        Status                TINYINT         NOT NULL DEFAULT 1,
        SubTotal              DECIMAL(18,2)   NOT NULL DEFAULT 0,
        DiscountAmount        DECIMAL(18,2)   NOT NULL DEFAULT 0,
        TaxAmount             DECIMAL(18,2)   NOT NULL DEFAULT 0,
        GrandTotal            DECIMAL(18,2)   NOT NULL DEFAULT 0,
        PrescriptionFilePath  NVARCHAR(500)   NULL,
        Notes                 NVARCHAR(500)   NULL,
        IsActive              BIT             NOT NULL DEFAULT 1,
        CreatedDate           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate           DATETIME2       NULL,
        CONSTRAINT PK_Orders PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Orders_OrderNumber UNIQUE (OrderNumber),
        CONSTRAINT FK_Orders_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
        CONSTRAINT CK_Orders_Status CHECK (Status BETWEEN 1 AND 7),
        CONSTRAINT CK_Orders_SubTotal CHECK (SubTotal >= 0),
        CONSTRAINT CK_Orders_DiscountAmount CHECK (DiscountAmount >= 0),
        CONSTRAINT CK_Orders_TaxAmount CHECK (TaxAmount >= 0),
        CONSTRAINT CK_Orders_GrandTotal CHECK (GrandTotal >= 0)
    );

    CREATE NONCLUSTERED INDEX IX_Orders_CustomerId ON Orders (CustomerId);
    CREATE NONCLUSTERED INDEX IX_Orders_Status ON Orders (Status);
    CREATE NONCLUSTERED INDEX IX_Orders_OrderDate ON Orders (OrderDate);
END
GO

-- ============================================
-- 2. OrderItems Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'OrderItems' AND xtype = 'U')
BEGIN
    CREATE TABLE OrderItems (
        Id              INT             IDENTITY(1,1) NOT NULL,
        OrderId         INT             NOT NULL,
        MedicineId      INT             NOT NULL,
        Quantity        INT             NOT NULL,
        UnitPrice       DECIMAL(18,2)   NOT NULL,
        DiscountPercent DECIMAL(5,2)    NOT NULL DEFAULT 0,
        LineTotal       DECIMAL(18,2)   NOT NULL DEFAULT 0,
        IsActive        BIT             NOT NULL DEFAULT 1,
        CreatedDate     DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate     DATETIME2       NULL,
        CONSTRAINT PK_OrderItems PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_OrderItems_OrderId FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_MedicineId FOREIGN KEY (MedicineId) REFERENCES Medicines(Id),
        CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0),
        CONSTRAINT CK_OrderItems_UnitPrice CHECK (UnitPrice >= 0),
        CONSTRAINT CK_OrderItems_DiscountPercent CHECK (DiscountPercent >= 0 AND DiscountPercent <= 100),
        CONSTRAINT CK_OrderItems_LineTotal CHECK (LineTotal >= 0)
    );

    CREATE NONCLUSTERED INDEX IX_OrderItems_OrderId ON OrderItems (OrderId);
    CREATE NONCLUSTERED INDEX IX_OrderItems_MedicineId ON OrderItems (MedicineId);
END
GO
