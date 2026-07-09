-- ============================================
-- Module 9: Customer Domain
-- Script: 009_AlterMedicines_AddPromotion.sql
-- Description: Adds medicine-level promotional pricing and a prescription
--              requirement flag, both consumed by the customer ordering
--              workflow (OrderService applies DiscountPercent; checkout
--              blocks items where RequiresPrescription = 1 until a
--              prescription file is attached to the order).
-- ============================================

IF NOT EXISTS (
    SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Medicines') AND name = 'DiscountPercent'
)
BEGIN
    ALTER TABLE Medicines ADD DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.check_constraints WHERE name = 'CK_Medicines_DiscountPercent'
)
BEGIN
    ALTER TABLE Medicines ADD CONSTRAINT CK_Medicines_DiscountPercent CHECK (DiscountPercent >= 0 AND DiscountPercent <= 100);
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Medicines') AND name = 'PromotionLabel'
)
BEGIN
    ALTER TABLE Medicines ADD PromotionLabel NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Medicines') AND name = 'RequiresPrescription'
)
BEGIN
    ALTER TABLE Medicines ADD RequiresPrescription BIT NOT NULL DEFAULT 0;
END
GO
