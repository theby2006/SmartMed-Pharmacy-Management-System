-- ============================================
-- Module 9: Customer Domain
-- Script: 003_AlterSales_AddCustomerId.sql
-- Description: Fixes schema drift between the Sale model (which has always
--              had a nullable CustomerId property) and the Sales table
--              (006_CreateSalesModule.sql never created that column), which
--              previously made ReportRepository.GetCustomerSales query a
--              column that did not exist. Must run after both
--              006_CreateSalesModule.sql and 002_CreateCustomersModule.sql.
-- ============================================

IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Sales') AND name = 'CustomerId'
)
BEGIN
    ALTER TABLE Sales ADD CustomerId INT NULL;
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.foreign_keys WHERE name = 'FK_Sales_CustomerId'
)
BEGIN
    ALTER TABLE Sales ADD CONSTRAINT FK_Sales_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers(Id);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sales_CustomerId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Sales_CustomerId ON Sales (CustomerId);
END
GO
