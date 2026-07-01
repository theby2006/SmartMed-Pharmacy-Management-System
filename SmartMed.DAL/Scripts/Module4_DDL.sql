-- Module 4: Supplier Management - Database Schema
-- =================================================

-- Suppliers table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Suppliers')
BEGIN
    CREATE TABLE Suppliers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SupplierCode NVARCHAR(50) NOT NULL,
        SupplierName NVARCHAR(200) NOT NULL,
        CompanyName NVARCHAR(200) NULL,
        ContactPerson NVARCHAR(100) NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        Country NVARCHAR(100) NULL,
        PostalCode NVARCHAR(20) NULL,
        TaxNumber NVARCHAR(50) NULL,
        Notes NVARCHAR(1000) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT UQ_Suppliers_SupplierCode UNIQUE (SupplierCode),
        CONSTRAINT UQ_Suppliers_SupplierName UNIQUE (SupplierName)
    );
END
GO

-- Index for supplier search (code, name, company, phone)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Suppliers_Search')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Suppliers_Search
        ON Suppliers (SupplierCode, SupplierName, CompanyName, PhoneNumber)
        WHERE IsActive = 1;
END
GO

-- Index for active supplier lookup
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Suppliers_IsActive')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Suppliers_IsActive
        ON Suppliers (IsActive)
        INCLUDE (SupplierCode, SupplierName, CompanyName);
END
GO

-- Seed data: Sample Suppliers
IF NOT EXISTS (SELECT * FROM Suppliers WHERE SupplierCode = 'SUP001')
BEGIN
    INSERT INTO Suppliers (SupplierCode, SupplierName, CompanyName, ContactPerson, PhoneNumber, Email, Address, City, Country, PostalCode, TaxNumber, Notes)
    VALUES ('SUP001', 'MediHealth Distributors', 'MediHealth Corp', 'John Smith', '+1-555-0101', 'john.smith@medihealth.com', '123 Health Ave', 'New York', 'USA', '10001', 'TAX-001', 'Primary pharmaceutical distributor');

    INSERT INTO Suppliers (SupplierCode, SupplierName, CompanyName, ContactPerson, PhoneNumber, Email, Address, City, Country, PostalCode, TaxNumber, Notes)
    VALUES ('SUP002', 'PharmaPlus Ltd', 'PharmaPlus Group', 'Sarah Johnson', '+1-555-0102', 'sarah.j@pharmaplus.com', '456 Medical Blvd', 'Los Angeles', 'USA', '90001', 'TAX-002', 'Generic medicines specialist');

    INSERT INTO Suppliers (SupplierCode, SupplierName, CompanyName, ContactPerson, PhoneNumber, Email, Address, City, Country, PostalCode, TaxNumber, Notes)
    VALUES ('SUP003', 'BioCare Solutions', 'BioCare Inc', 'Michael Chen', '+1-555-0103', 'm.chen@biocare.com', '789 Pharma Dr', 'Chicago', 'USA', '60601', 'TAX-003', 'Biotech and specialty medications');
END
GO
