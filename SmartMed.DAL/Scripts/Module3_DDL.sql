-- Module 3: Medicine Management - Database Schema
-- ================================================

-- MedicineCategories table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MedicineCategories')
BEGIN
    CREATE TABLE MedicineCategories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT UQ_MedicineCategories_Name UNIQUE (Name)
    );
END
GO

-- Medicines table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Medicines')
BEGIN
    CREATE TABLE Medicines (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CategoryId INT NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Brand NVARCHAR(200) NULL,
        DosageForm INT NOT NULL,
        Strength NVARCHAR(50) NULL,
        Unit NVARCHAR(50) NOT NULL,
        StockQuantity INT NOT NULL DEFAULT 0,
        ReorderLevel INT NOT NULL DEFAULT 10,
        UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        ExpiryDate DATETIME2 NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT FK_Medicines_CategoryId FOREIGN KEY (CategoryId) REFERENCES MedicineCategories(Id),
        CONSTRAINT UQ_Medicines_Name_Brand UNIQUE (Name, Brand)
    );
END
GO

-- Seed data: Medicine Categories
IF NOT EXISTS (SELECT * FROM MedicineCategories WHERE Name = 'Antibiotics')
BEGIN
    INSERT INTO MedicineCategories (Name, Description) VALUES ('Antibiotics', 'Medications used to treat bacterial infections');
    INSERT INTO MedicineCategories (Name, Description) VALUES ('Analgesics', 'Pain relief medications');
    INSERT INTO MedicineCategories (Name, Description) VALUES ('Antipyretics', 'Medications used to reduce fever');
    INSERT INTO MedicineCategories (Name, Description) VALUES ('Antihistamines', 'Medications for allergy relief');
    INSERT INTO MedicineCategories (Name, Description) VALUES ('Antacids', 'Medications for digestive health and acid relief');
    INSERT INTO MedicineCategories (Name, Description) VALUES ('Vitamins and Supplements', 'Dietary supplements and vitamin formulations');
    INSERT INTO MedicineCategories (Name, Description) VALUES ('Respiratory', 'Medications for respiratory conditions');
    INSERT INTO MedicineCategories (Name, Description) VALUES ('Dermatological', 'Skin care and topical treatments');
END
GO

-- Seed data: Medicines
IF NOT EXISTS (SELECT * FROM Medicines WHERE Name = 'Amoxicillin')
BEGIN
    DECLARE @antibiotics INT = (SELECT Id FROM MedicineCategories WHERE Name = 'Antibiotics');
    DECLARE @analgesics INT = (SELECT Id FROM MedicineCategories WHERE Name = 'Analgesics');
    DECLARE @antipyretics INT = (SELECT Id FROM MedicineCategories WHERE Name = 'Antipyretics');
    DECLARE @antihistamines INT = (SELECT Id FROM MedicineCategories WHERE Name = 'Antihistamines');
    DECLARE @antacids INT = (SELECT Id FROM MedicineCategories WHERE Name = 'Antacids');
    DECLARE @vitamins INT = (SELECT Id FROM MedicineCategories WHERE Name = 'Vitamins and Supplements');
    DECLARE @respiratory INT = (SELECT Id FROM MedicineCategories WHERE Name = 'Respiratory');
    DECLARE @dermatological INT = (SELECT Id FROM MedicineCategories WHERE Name = 'Dermatological');

    -- Antibiotics
    INSERT INTO Medicines (CategoryId, Name, Brand, DosageForm, Strength, Unit, StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description)
    VALUES (@antibiotics, 'Amoxicillin', 'Amoxil', 1, '500mg', 'mg', 500, 50, 12.50, DATEADD(MONTH, 18, GETUTCDATE()), 'Broad-spectrum penicillin antibiotic');
    INSERT INTO Medicines (CategoryId, Name, Brand, DosageForm, Strength, Unit, StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description)
    VALUES (@antibiotics, 'Azithromycin', 'Zithromax', 2, '250mg', 'mg', 300, 30, 15.00, DATEADD(MONTH, 18, GETUTCDATE()), 'Macrolide antibiotic for respiratory infections');
    INSERT INTO Medicines (CategoryId, Name, Brand, DosageForm, Strength, Unit, StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description)
    VALUES (@antibiotics, 'Ciprofloxacin', 'Cipro', 1, '500mg', 'mg', 200, 20, 18.75, DATEADD(MONTH, 15, GETUTCDATE()), 'Fluoroquinolone antibiotic');
    INSERT INTO Medicines (CategoryId, Name, Brand, DosageForm, Strength, Unit, StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description)
    VALUES (@antibiotics, 'Doxycycline', 'Vibramycin', 2, '100mg', 'mg', 400, 40, 9.99, DATEADD(MONTH, 20, GETUTCDATE()), 'Tetracycline antibiotic');

    -- Analgesics
    INSERT INTO Medicines (CategoryId, Name, Brand, DosageForm, Strength, Unit, StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description)
    VALUES (@analgesics, 'Paracetamol', 'Panadol', 1, '500mg', 'mg', 1000, 100, 4.50, DATEADD(MONTH, 24, GETUTCDATE()), 'Mild analgesic and antipyretic');
    INSERT INTO Medicines (CategoryId, Name, Brand, DosageForm, Strength, Unit, StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description)
    VALUES (@analgesics, 'Ibuprofen', 'Nurofen', 1, '400mg', 'mg', 800, 80, 6.00, DATEADD(MONTH, 22, GETUTCDATE()), 'NSAID for pain and inflammation');

    -- Antihistamines
    INSERT INTO Medicines (CategoryId, Name, Brand, DosageForm, Strength, Unit, StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description)
    VALUES (@antihistamines, 'Cetirizine', 'Zyrtec', 1, '10mg', 'mg', 600, 60, 7.25, DATEADD(MONTH, 18, GETUTCDATE()), 'Second-generation antihistamine');
    INSERT INTO Medicines (CategoryId, Name, Brand, DosageForm, Strength, Unit, StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description)
    VALUES (@antihistamines, 'Loratadine', 'Claritin', 1, '10mg', 'mg', 550, 55, 8.00, DATEADD(MONTH, 18, GETUTCDATE()), 'Non-drowsy antihistamine');

    -- Respiratory
    INSERT INTO Medicines (CategoryId, Name, Brand, DosageForm, Strength, Unit, StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description)
    VALUES (@respiratory, 'Salbutamol', 'Ventolin', 8, '100mcg', 'mcg', 150, 15, 22.00, DATEADD(MONTH, 12, GETUTCDATE()), 'Short-acting bronchodilator for asthma');
    INSERT INTO Medicines (CategoryId, Name, Brand, DosageForm, Strength, Unit, StockQuantity, ReorderLevel, UnitPrice, ExpiryDate, Description)
    VALUES (@respiratory, 'Fluticasone', 'Flixotide', 8, '250mcg', 'mcg', 100, 10, 28.50, DATEADD(MONTH, 14, GETUTCDATE()), 'Inhaled corticosteroid for asthma');
END
GO
