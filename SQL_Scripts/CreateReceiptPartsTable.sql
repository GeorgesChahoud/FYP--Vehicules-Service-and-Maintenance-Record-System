-- ========================================
-- Create ReceiptParts Table
-- ========================================

-- Step 1: Create the ReceiptParts table
CREATE TABLE [dbo].[ReceiptParts] (
    [ID] INT IDENTITY(1,1) NOT NULL,
    [ReceiptID] INT NOT NULL,
    [PartID] INT NOT NULL,
    [QuantityUsed] INT NOT NULL,
    [PriceAtTime] FLOAT NOT NULL,
    CONSTRAINT [PK_ReceiptParts] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

-- Step 2: Add Foreign Key to Receipts
ALTER TABLE [dbo].[ReceiptParts]
ADD CONSTRAINT [FK_ReceiptParts_Receipts_ReceiptID] 
FOREIGN KEY ([ReceiptID]) 
REFERENCES [dbo].[Receipts] ([ID]) 
ON DELETE CASCADE;
GO

-- Step 3: Add Foreign Key to Parts
ALTER TABLE [dbo].[ReceiptParts]
ADD CONSTRAINT [FK_ReceiptParts_Parts_PartID] 
FOREIGN KEY ([PartID]) 
REFERENCES [dbo].[Parts] ([ID]) 
ON DELETE CASCADE;
GO

-- Step 4: Create Index on ReceiptID
CREATE NONCLUSTERED INDEX [IX_ReceiptParts_ReceiptID] 
ON [dbo].[ReceiptParts]([ReceiptID] ASC);
GO

-- Step 5: Create Index on PartID  
CREATE NONCLUSTERED INDEX [IX_ReceiptParts_PartID] 
ON [dbo].[ReceiptParts]([PartID] ASC);
GO

-- Step 6: Add migration history entry (tells EF the migration was applied)
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241204120000_AddReceiptPartsTable', N'9.0.9');
GO

-- Verification Query
SELECT 
    TABLE_NAME, 
    COLUMN_NAME, 
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ReceiptParts'
ORDER BY ORDINAL_POSITION;
GO

PRINT '? ReceiptParts table created successfully!';
PRINT '? Foreign keys added';
PRINT '? Indexes created';
PRINT '? Migration history updated';
PRINT '';
PRINT '?? You can now use the receipts feature!';
GO
