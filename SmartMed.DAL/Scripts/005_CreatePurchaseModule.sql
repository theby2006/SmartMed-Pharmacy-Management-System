-- Module 5: Purchase Management
-- Creates Purchases, PurchaseItems, StockBatches, StockMovements tables

-- Drop existing tables in reverse dependency order
IF OBJECT_ID('dbo.StockMovements', 'U') IS NOT NULL DROP TABLE dbo.StockMovements;
IF OBJECT_ID('dbo.StockBatches', 'U') IS NOT NULL DROP TABLE dbo.StockBatches;
IF OBJECT_ID('dbo.PurchaseItems', 'U') IS NOT NULL DROP TABLE dbo.PurchaseItems;
IF OBJECT_ID('dbo.Purchases', 'U') IS NOT NULL DROP TABLE dbo.Purchases;

-- Purchases table
CREATE TABLE dbo.Purchases
(
    Id              INT             NOT NULL IDENTITY(1,1),
    PurchaseNumber  NVARCHAR(50)    NOT NULL,
    PurchaseDate    DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    SupplierId      INT             NOT NULL,
    InvoiceNumber   NVARCHAR(100)   NULL,
    Remarks         NVARCHAR(500)   NULL,
    CreatedByUserId INT             NOT NULL,
    CreatedDate     DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate     DATETIME2       NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    Status          TINYINT         NOT NULL DEFAULT 1,
    ConfirmedDate   DATETIME2       NULL,
    TotalAmount     DECIMAL(18,2)   NOT NULL DEFAULT 0,
    CONSTRAINT PK_Purchases PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Purchases_PurchaseNumber UNIQUE (PurchaseNumber),
    CONSTRAINT FK_Purchases_SupplierId FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT CK_Purchases_TotalAmount CHECK (TotalAmount >= 0)
);

CREATE NONCLUSTERED INDEX IX_Purchases_SupplierId ON dbo.Purchases(SupplierId);
CREATE NONCLUSTERED INDEX IX_Purchases_PurchaseDate ON dbo.Purchases(PurchaseDate);
CREATE NONCLUSTERED INDEX IX_Purchases_Status ON dbo.Purchases(Status);

-- PurchaseItems table
CREATE TABLE dbo.PurchaseItems
(
    Id              INT             NOT NULL IDENTITY(1,1),
    PurchaseId      INT             NOT NULL,
    MedicineId      INT             NOT NULL,
    BatchNumber     NVARCHAR(100)   NOT NULL,
    ExpiryDate      DATETIME2       NOT NULL,
    Quantity        INT             NOT NULL,
    PurchasePrice   DECIMAL(18,2)   NOT NULL,
    SellingPrice    DECIMAL(18,2)   NOT NULL,
    DiscountPercent DECIMAL(5,2)    NOT NULL DEFAULT 0,
    TaxPercent      DECIMAL(5,2)    NOT NULL DEFAULT 0,
    LineTotal       DECIMAL(18,2)   NOT NULL DEFAULT 0,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedDate     DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate     DATETIME2       NULL,
    CONSTRAINT PK_PurchaseItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_PurchaseItems_PurchaseId FOREIGN KEY (PurchaseId) REFERENCES dbo.Purchases(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PurchaseItems_MedicineId FOREIGN KEY (MedicineId) REFERENCES dbo.Medicines(Id),
    CONSTRAINT CK_PurchaseItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_PurchaseItems_PurchasePrice CHECK (PurchasePrice > 0),
    CONSTRAINT CK_PurchaseItems_SellingPrice CHECK (SellingPrice >= 0),
    CONSTRAINT CK_PurchaseItems_DiscountPercent CHECK (DiscountPercent >= 0 AND DiscountPercent <= 100),
    CONSTRAINT CK_PurchaseItems_TaxPercent CHECK (TaxPercent >= 0 AND TaxPercent <= 100),
    CONSTRAINT CK_PurchaseItems_LineTotal CHECK (LineTotal >= 0)
);

CREATE NONCLUSTERED INDEX IX_PurchaseItems_PurchaseId ON dbo.PurchaseItems(PurchaseId);
CREATE NONCLUSTERED INDEX IX_PurchaseItems_MedicineId ON dbo.PurchaseItems(MedicineId);

-- StockBatches table
CREATE TABLE dbo.StockBatches
(
    Id              INT             NOT NULL IDENTITY(1,1),
    MedicineId      INT             NOT NULL,
    BatchNumber     NVARCHAR(100)   NOT NULL,
    ExpiryDate      DATETIME2       NOT NULL,
    PurchasePrice   DECIMAL(18,2)   NOT NULL,
    SellingPrice    DECIMAL(18,2)   NOT NULL,
    CurrentQuantity INT             NOT NULL DEFAULT 0,
    InitialQuantity INT             NOT NULL DEFAULT 0,
    PurchaseItemId  INT             NOT NULL,
    BatchStatus     TINYINT         NOT NULL DEFAULT 1,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedDate     DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate     DATETIME2       NULL,
    CONSTRAINT PK_StockBatches PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_StockBatches_MedicineId FOREIGN KEY (MedicineId) REFERENCES dbo.Medicines(Id),
    CONSTRAINT FK_StockBatches_PurchaseItemId FOREIGN KEY (PurchaseItemId) REFERENCES dbo.PurchaseItems(Id),
    CONSTRAINT CK_StockBatches_CurrentQuantity CHECK (CurrentQuantity >= 0),
    CONSTRAINT CK_StockBatches_InitialQuantity CHECK (InitialQuantity > 0),
    CONSTRAINT CK_StockBatches_PurchasePrice CHECK (PurchasePrice > 0),
    CONSTRAINT CK_StockBatches_SellingPrice CHECK (SellingPrice >= 0),
    CONSTRAINT CK_StockBatches_BatchStatus CHECK (BatchStatus BETWEEN 1 AND 4)
);

CREATE UNIQUE NONCLUSTERED INDEX UQ_StockBatches_MedicineId_BatchNumber ON dbo.StockBatches(MedicineId, BatchNumber) WHERE IsActive = 1;
CREATE NONCLUSTERED INDEX IX_StockBatches_MedicineId ON dbo.StockBatches(MedicineId);
CREATE NONCLUSTERED INDEX IX_StockBatches_ExpiryDate ON dbo.StockBatches(MedicineId, ExpiryDate);

-- StockMovements table
CREATE TABLE dbo.StockMovements
(
    Id              INT             NOT NULL IDENTITY(1,1),
    StockBatchId    INT             NOT NULL,
    MedicineId      INT             NOT NULL,
    MovementType    TINYINT         NOT NULL,
    Quantity        INT             NOT NULL,
    ReferenceType   NVARCHAR(50)    NOT NULL,
    ReferenceId     INT             NOT NULL,
    UnitPrice       DECIMAL(18,2)   NOT NULL,
    TotalAmount     DECIMAL(18,2)   NOT NULL,
    CreatedByUserId INT             NOT NULL,
    CreatedDate     DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate     DATETIME2       NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    Notes           NVARCHAR(500)   NULL,
    CONSTRAINT PK_StockMovements PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_StockMovements_StockBatchId FOREIGN KEY (StockBatchId) REFERENCES dbo.StockBatches(Id),
    CONSTRAINT FK_StockMovements_MedicineId FOREIGN KEY (MedicineId) REFERENCES dbo.Medicines(Id),
    CONSTRAINT CK_StockMovements_MovementType CHECK (MovementType IN (1, 2)),
    CONSTRAINT CK_StockMovements_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_StockMovements_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT CK_StockMovements_TotalAmount CHECK (TotalAmount >= 0)
);

CREATE NONCLUSTERED INDEX IX_StockMovements_StockBatchId ON dbo.StockMovements(StockBatchId);
CREATE NONCLUSTERED INDEX IX_StockMovements_MedicineId ON dbo.StockMovements(MedicineId);
CREATE NONCLUSTERED INDEX IX_StockMovements_Reference ON dbo.StockMovements(ReferenceType, ReferenceId);
CREATE NONCLUSTERED INDEX IX_StockMovements_CreatedDate ON dbo.StockMovements(CreatedDate);
