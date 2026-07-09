-- ============================================
-- SmartMed Pharmacy Management System
-- Master Database Setup Script
-- ============================================
-- Creates the SmartMedDb database (if it does not already exist) and then
-- runs every module script in dependency order. Every referenced script is
-- idempotent, so re-running this file against an already-provisioned
-- database is safe.
--
-- HOW TO RUN
--   Option A (SQL Server Management Studio): open this file, enable
--   "SQLCMD Mode" from the Query menu, then Execute. SQLCMD Mode is what
--   makes the ":r" include directives below work.
--   Option B (command line): sqlcmd -S <server> -i 000_SetupDatabase.sql
--   sqlcmd supports ":r" natively, no extra mode switch required.
--
-- If neither option is available, the scripts below can instead be run
-- individually, in the numeric/name order listed in the RUN ORDER section.
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SmartMedDb')
BEGIN
    CREATE DATABASE SmartMedDb;
    PRINT 'Created database SmartMedDb.';
END
ELSE
BEGIN
    PRINT 'Database SmartMedDb already exists.';
END
GO

USE SmartMedDb;
GO

-- ============================================
-- RUN ORDER
-- 1. 001_CreateUsersModule.sql       (Users, AuditLogs)
-- 2. Module3_DDL.sql                 (MedicineCategories, Medicines)
-- 3. Module4_DDL.sql                 (Suppliers)
-- 4. 005_CreatePurchaseModule.sql    (Purchases, PurchaseItems, StockBatches, StockMovements)
-- 5. 006_CreateSalesModule.sql       (Sales, SaleItems, Payments)
-- 6. 002_CreateCustomersModule.sql   (Customers)
-- 7. 003_AlterSales_AddCustomerId.sql (Sales.CustomerId + FK)
-- 8. 007_CreateOrdersModule.sql      (Orders, OrderItems)
-- 9. 008_CreateSettingsModule.sql    (Settings, BackupHistory, UserPreferences, ErrorLogs, PerformanceLogs)
-- 10. 009_AlterMedicines_AddPromotion.sql (Medicines.DiscountPercent/PromotionLabel/RequiresPrescription)
-- ============================================

:r ".\001_CreateUsersModule.sql"
:r ".\Module3_DDL.sql"
:r ".\Module4_DDL.sql"
:r ".\005_CreatePurchaseModule.sql"
:r ".\006_CreateSalesModule.sql"
:r ".\002_CreateCustomersModule.sql"
:r ".\003_AlterSales_AddCustomerId.sql"
:r ".\007_CreateOrdersModule.sql"
:r ".\008_CreateSettingsModule.sql"
:r ".\009_AlterMedicines_AddPromotion.sql"

PRINT '============================================';
PRINT 'SmartMedDb setup complete.';
PRINT 'Seeded staff accounts: admin / pharmacist / cashier (see README.md for passwords).';
PRINT 'Seeded sample customers: see 002_CreateCustomersModule.sql.';
PRINT '============================================';
GO
