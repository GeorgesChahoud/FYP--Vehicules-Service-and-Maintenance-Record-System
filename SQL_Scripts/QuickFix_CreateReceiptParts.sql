-- =========================================
-- Quick Fix: Create ReceiptParts Table
-- =========================================
-- Run this script in SQL Server Management Studio
-- This will link your existing Receipts with Parts

USE [YOUR_DATABASE_NAME];  -- Replace with your actual database name
GO

-- Check if table already exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReceiptParts')
BEGIN
    -- Create ReceiptParts table
    CREATE TABLE [dbo].[ReceiptParts] (
        [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ReceiptID] INT NOT NULL,
        [PartID] INT NOT NULL,
        [QuantityUsed] INT NOT NULL,
        [PriceAtTime] FLOAT NOT NULL
    );

    -- Link to your existing Receipts table
    ALTER TABLE [dbo].[ReceiptParts]
    ADD CONSTRAINT [FK_ReceiptParts_Receipts] 
    FOREIGN KEY ([ReceiptID]) 
    REFERENCES [dbo].[Receipts] ([ID]) 
    ON DELETE CASCADE;

    -- Link to your existing Parts table
    ALTER TABLE [dbo].[ReceiptParts]
    ADD CONSTRAINT [FK_ReceiptParts_Parts] 
    FOREIGN KEY ([PartID]) 
    REFERENCES [dbo].[Parts] ([ID]) 
    ON DELETE CASCADE;

    -- Create indexes for better performance
    CREATE INDEX [IX_ReceiptParts_ReceiptID] ON [dbo].[ReceiptParts]([ReceiptID]);
    CREATE INDEX [IX_ReceiptParts_PartID] ON [dbo].[ReceiptParts]([PartID]);

    -- Tell Entity Framework the migration was applied
    IF EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20241204120000_AddReceiptPartsTable', N'9.0.9');
    END

    PRINT '? SUCCESS! ReceiptParts table created and linked to your existing Receipts table!';
    PRINT '? Foreign keys added';
    PRINT '? Indexes created';
    PRINT '';
    PRINT '?? Your receipts feature is now ready to use!';
END
ELSE
BEGIN
    PRINT '?? ReceiptParts table already exists!';
    PRINT 'If you are still getting errors, check:';
    PRINT '1. Foreign keys exist: SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(''ReceiptParts'')';
    PRINT '2. Migration history: SELECT * FROM __EFMigrationsHistory WHERE MigrationId LIKE ''%ReceiptParts%''';
END
GO

-- Verify everything is set up correctly
PRINT '';
PRINT '=== VERIFICATION ===';

-- Check table structure
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ReceiptParts'
ORDER BY ORDINAL_POSITION;

-- Check foreign keys
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys fk
WHERE OBJECT_NAME(fk.parent_object_id) = 'ReceiptParts';

-- Show sample join to verify it works with your existing Receipts
SELECT TOP 3
    r.ID as ReceiptID,
    r.AppointmentID,
    r.Total,
    r.DateANDTime,
    COUNT(rp.ID) as PartsCount
FROM Receipts r
LEFT JOIN ReceiptParts rp ON r.ID = rp.ReceiptID
GROUP BY r.ID, r.AppointmentID, r.Total, r.DateANDTime
ORDER BY r.DateANDTime DESC;

PRINT '';
PRINT '=== READY TO USE ===';
PRINT 'Your existing Receipts table: ? Working';
PRINT 'New ReceiptParts table: ? Created and linked';
PRINT 'You can now create receipts with parts!';
GO
