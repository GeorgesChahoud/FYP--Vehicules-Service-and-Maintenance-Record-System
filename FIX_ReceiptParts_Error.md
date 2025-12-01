# ?? QUICK FIX: Invalid object name 'ReceiptParts'

## ? **The Problem**
```
SqlException: Invalid object name 'ReceiptParts'.
```

This error means the `ReceiptParts` table doesn't exist in your database yet.

---

## ? **THE SOLUTION (Choose One)**

### **Option 1: Run SQL Script Directly** ? **EASIEST & FASTEST**

1. **Open SQL Server Management Studio (SSMS)** or **Azure Data Studio**
2. **Connect to your database**
3. **Open the file:** `SQL_Scripts\CreateReceiptPartsTable.sql`
4. **Execute the script** (Press F5 or click Execute)
5. **Done!** ?

**The script will:**
- ? Create the `ReceiptParts` table
- ? Add foreign keys to `Receipts` and `Parts`
- ? Create indexes for better performance
- ? Update migration history
- ? Show verification query

---

### **Option 2: Let Application Auto-Migrate**

The auto-migration in `Program.cs` should work, but might be failing silently.

**Steps:**
1. **Stop your application** completely
2. **Restart it**
3. **Watch the console output** for:
   ```
   ?? Checking for pending database migrations...
   ? Database migrations applied successfully!
   ```

**If you see errors instead:**
- The SQL script method (Option 1) is more reliable
- Check your connection string in `appsettings.json`
- Verify SQL Server is running

---

### **Option 3: Package Manager Console**

If you have Entity Framework tools installed:

```powershell
# In Visual Studio: Tools ? NuGet Package Manager ? Package Manager Console
Update-Database
```

---

### **Option 4: .NET CLI**

If you have `dotnet-ef` installed:

```sh
dotnet ef database update
```

---

## ?? **Recommended: Use Option 1 (SQL Script)**

**Why?**
- ? Fastest and most reliable
- ? No dependencies on EF tools
- ? You can see exactly what's happening
- ? Works even if auto-migration fails
- ? Includes verification queries

---

## ?? **After Running the Script**

### **Verify It Worked:**

```sql
-- Check if table exists
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'ReceiptParts';

-- Should return 1 row ?

-- Check table structure
SELECT 
    COLUMN_NAME, 
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ReceiptParts'
ORDER BY ORDINAL_POSITION;

-- Should return 5 rows (ID, ReceiptID, PartID, QuantityUsed, PriceAtTime) ?

-- Check foreign keys
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fc 
    ON fk.object_id = fc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'ReceiptParts';

-- Should return 2 rows (FK to Receipts and FK to Parts) ?
```

---

## ?? **Then Test Your Application**

1. **Refresh your application** (F5 or reload page)
2. **Navigate to:** `/Employee/MyReceipts`
3. **You should now see:**
   - ? "No receipts yet" message (if you haven't created any)
   - ? OR your list of receipts (if you have created some)
   - ? NO ERROR!

---

## ?? **Create Your First Receipt**

1. **Go to:** `/Employee/MyAppointments`
2. **Find an appointment** with status "In Progress"
   - If none exist, click "Start Service" on a confirmed appointment
3. **Click:** "Create Receipt" button
4. **Fill the form:**
   - Select parts (optional)
   - Enter quantities
   - Review total
5. **Submit**
6. **Success!** Your receipt is created! ??

---

## ?? **What the SQL Script Does**

```sql
-- 1. Creates the ReceiptParts table
CREATE TABLE [ReceiptParts] (
    [ID] INT IDENTITY(1,1) PRIMARY KEY,
    [ReceiptID] INT NOT NULL,
    [PartID] INT NOT NULL,
    [QuantityUsed] INT NOT NULL,
    [PriceAtTime] FLOAT NOT NULL
);

-- 2. Links it to Receipts table
ALTER TABLE [ReceiptParts]
ADD FOREIGN KEY ([ReceiptID]) 
REFERENCES [Receipts] ([ID]) 
ON DELETE CASCADE;

-- 3. Links it to Parts table
ALTER TABLE [ReceiptParts]
ADD FOREIGN KEY ([PartID]) 
REFERENCES [Parts] ([ID]) 
ON DELETE CASCADE;

-- 4. Creates performance indexes
CREATE INDEX [IX_ReceiptParts_ReceiptID] ON [ReceiptParts]([ReceiptID]);
CREATE INDEX [IX_ReceiptParts_PartID] ON [ReceiptParts]([PartID]);

-- 5. Tells Entity Framework the migration was applied
INSERT INTO [__EFMigrationsHistory] 
VALUES (N'20241204120000_AddReceiptPartsTable', N'9.0.9');
```

---

## ?? **Troubleshooting**

### **Error: "There is already an object named 'ReceiptParts'"**

**Solution:** The table already exists! Just update migration history:

```sql
-- Check if migration is in history
SELECT * FROM __EFMigrationsHistory 
WHERE MigrationId LIKE '%ReceiptParts%';

-- If NOT there, add it:
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241204120000_AddReceiptPartsTable', N'9.0.9');
```

---

### **Error: "Cannot find table 'Receipts' or 'Parts'"**

**Solution:** Those tables need to exist first. Run:

```sql
-- Check if they exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN ('Receipts', 'Parts');

-- Should return 2 rows
-- If not, you need to run all previous migrations first
```

---

### **Error: "Cannot open database"**

**Solution:**
1. Check your connection string in `appsettings.json`
2. Verify SQL Server is running
3. Confirm you have permissions to the database

---

## ?? **Files Reference**

### **Created Migration Files:**
- `Migrations\20241204120000_AddReceiptPartsTable.cs` ?
- `SQL_Scripts\CreateReceiptPartsTable.sql` ?

### **Model File:**
- `Models\ReceiptPart.cs` ? (already exists)

### **Controller:**
- `Controllers\EmployeeController.cs` ? (already has MyReceipts action)

### **Views:**
- `Views\Employee\MyReceipts.cshtml` ?
- `Views\Employee\ReceiptDetail.cshtml` ?

---

## ?? **Time to Fix**

- **Option 1 (SQL Script):** 2 minutes ?
- **Option 2 (Auto-migrate):** 5 minutes
- **Option 3 (PM Console):** 3 minutes
- **Option 4 (.NET CLI):** 3 minutes

---

## ? **Success Checklist**

- [ ] SQL script executed successfully
- [ ] Verification queries return expected results
- [ ] No errors in SQL Server
- [ ] Application starts without errors
- [ ] `/Employee/MyReceipts` page loads
- [ ] Can create a receipt
- [ ] Receipt appears in the list

---

## ?? **After the Fix**

Your receipts system will be **fully functional**:

- ? Professional receipts list
- ? Statistics dashboard
- ? Detailed receipt view
- ? Print functionality
- ? Monthly overview
- ? Parts tracking

---

## ?? **Still Having Issues?**

### **Check These:**

1. **Database connection:** 
   ```json
   // appsettings.json
   "ConnectionStrings": {
     "DefaultConnection": "Server=...;Database=...;..."
   }
   ```

2. **SQL Server status:**
   ```sql
   SELECT @@VERSION;  -- Should return version info
   ```

3. **Database permissions:**
   ```sql
   -- You should be able to:
   CREATE TABLE test (id INT);
   DROP TABLE test;
   ```

4. **Migration history:**
   ```sql
   SELECT * FROM __EFMigrationsHistory 
   ORDER BY MigrationId DESC;
   ```

---

## ?? **Pro Tip**

After running the SQL script, **restart your application** to ensure all changes are picked up. Then navigate to `/Employee/MyReceipts` and you should be good to go!

---

**Created:** December 2024  
**Status:** ? **Ready to Use**  
**Time to Fix:** ?? **2-5 minutes**  
**Difficulty:** ? **Easy**

?? **Just run the SQL script and you're done!**
