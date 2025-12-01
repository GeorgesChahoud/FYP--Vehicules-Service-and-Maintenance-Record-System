# ?? Entity Framework Migrations - Complete Guide

## ? **GOOD NEWS: Everything is Already Set Up!**

Your project is configured with **Entity Framework Core Migrations** - this means:
- ? **NO SQL scripts needed!**
- ? **Automatic database creation**
- ? **Automatic table updates**
- ? **Version control for database changes**
- ? **Easy deployment**

---

## ?? **Your Current Migrations**

All your database structure is managed by these migration files:

| Migration File | What It Does | Status |
|----------------|--------------|--------|
| `20251119125801_AllOnNewDB.cs` | Creates all core tables | ? Applied |
| `20251119131605_AddServiceID.cs` | Adds ServiceID to Appointments | ? Applied |
| `20251128130407_AddEmployeeToAppointment.cs` | Adds EmployeeID to Appointments | ? Applied |
| `20241204120000_AddReceiptPartsTable.cs` | Creates ReceiptParts table | ? Applied |

---

## ?? **How Auto-Migration Works**

### **In Your `Program.cs`:**

```csharp
// Auto-apply pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("?? Checking for pending database migrations...");
    context.Database.Migrate(); // ? Magic happens here!
    logger.LogInformation("? Database migrations applied successfully!");
}
```

**What this does:**
1. **On application startup**, checks for pending migrations
2. **Automatically applies** any new migrations
3. **Creates database** if it doesn't exist
4. **Updates tables** to match your models
5. **Inserts seed data** (Roles, Status, Admin users)

---

## ?? **What Gets Created Automatically**

### **When you start your app:**

? **All Tables:**
- Users, Roles, Admins, Employees, Customers
- Cars, Appointments, Status
- Services, Parts
- Receipts, **ReceiptParts** ?
- ReceiptServices, PartServices
- PasswordResets, RegistrationVerifications
- AspNetUsers, AspNetRoles (Identity tables)

? **All Relationships:**
- Foreign keys
- Indexes
- Constraints

? **All Seed Data:**
- Roles: Admin, Employee, Customer
- Status: Pending, Confirmed, In Progress, Completed, Cancelled
- 4 Admin users with BCrypt hashed passwords

---

## ?? **How to Use Migrations**

### **Option 1: Auto-Migration (Already Set Up!)** ? **RECOMMENDED**

Just run your application:
```
Press F5 in Visual Studio
```

The database will:
- ? Be created if it doesn't exist
- ? Apply all pending migrations
- ? Be ready to use!

**Watch the console for:**
```
?? Checking for pending database migrations...
Applying migration '20251119125801_AllOnNewDB'.
Applying migration '20251119131605_AddServiceID'.
Applying migration '20251128130407_AddEmployeeToAppointment'.
Applying migration '20241204120000_AddReceiptPartsTable'.
? Database migrations applied successfully!
```

---

### **Option 2: Package Manager Console**

If you want manual control:

```powershell
# Apply all pending migrations
Update-Database

# Rollback to a specific migration
Update-Database -Migration AddServiceID

# Generate SQL script (without applying)
Script-Migration

# Remove last migration (if not applied)
Remove-Migration
```

---

### **Option 3: .NET CLI**

```sh
# Apply all pending migrations
dotnet ef database update

# Rollback to specific migration
dotnet ef database update AddServiceID

# Generate SQL script
dotnet ef migrations script

# Remove last migration
dotnet ef migrations remove
```

---

## ? **Creating New Migrations**

### **When to create a new migration:**
- Adding a new table
- Adding/removing columns
- Changing data types
- Adding relationships

### **How to create:**

#### **Package Manager Console:**
```powershell
Add-Migration YourMigrationName
```

#### **.NET CLI:**
```sh
dotnet ef migrations add YourMigrationName
```

### **Example: Adding a new field**

1. **Update your model:**
```csharp
public class Employee
{
    public int ID { get; set; }
    // ...existing properties...
    public string Specialization { get; set; } // NEW!
}
```

2. **Create migration:**
```powershell
Add-Migration AddSpecializationToEmployee
```

3. **Apply migration:**
```powershell
Update-Database
```

Or just **run your app** - auto-migration will apply it!

---

## ??? **Your ApplicationDbContext**

### **Seed Data Configured:**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Roles
    modelBuilder.Entity<Role>().HasData(
        new Role { ID = 1, Name = "Admin"},
        new Role { ID = 2, Name = "Employee"},
        new Role { ID = 3, Name = "Customer"}
    );

    // Admin Users
    modelBuilder.Entity<User>().HasData(
        new User { ID = 1, RoleID = 1, FirstName = "Georges", ... },
        new User { ID = 2, RoleID = 1, FirstName = "Christopher", ... },
        new User { ID = 3, RoleID = 1, FirstName = "Elias", ... },
        new User { ID = 4, RoleID = 1, FirstName = "Anthony", ... }
    );

    // Status
    modelBuilder.Entity<Status>().HasData(
        new Status { ID = 1, Name = "Pending" },
        new Status { ID = 2, Name = "Confirmed" },
        new Status { ID = 3, Name = "In Progress" },
        new Status { ID = 4, Name = "Completed" },
        new Status { ID = 5, Name = "Cancelled" }
    );
}
```

This data is **automatically inserted** by migrations!

---

## ?? **Checking Migration Status**

### **See which migrations are applied:**

```powershell
# Package Manager Console
Get-Migration

# .NET CLI
dotnet ef migrations list
```

### **Check in database:**

```sql
SELECT * FROM __EFMigrationsHistory
ORDER BY MigrationId DESC;
```

This shows all applied migrations:
```
MigrationId                                  ProductVersion
20251119125801_AllOnNewDB                    9.0.9
20251119131605_AddServiceID                  9.0.9
20251128130407_AddEmployeeToAppointment      9.0.9
20241204120000_AddReceiptPartsTable          9.0.9
```

---

## ?? **Database Recreation Workflow**

### **If you need to start fresh:**

1. **Delete the database:**
```sql
USE master;
DROP DATABASE CarHubDB;
```

2. **Start your application** (F5)

3. **Migrations run automatically!**
   - Database created ?
   - All tables created ?
   - All relationships created ?
   - Seed data inserted ?

4. **Ready to use!** ??

---

## ??? **Troubleshooting**

### **Issue: "Pending model changes"**

**Solution:**
```powershell
Add-Migration FixPendingChanges
Update-Database
```

---

### **Issue: Migration fails**

**Check:**
1. **DbContext** properly registered in `Program.cs`
2. **Connection string** is correct in `appsettings.json`
3. **SQL Server** is running
4. **No syntax errors** in model classes

**Fix:**
```powershell
# Rollback to previous migration
Update-Database -Migration PreviousMigrationName

# Remove failed migration
Remove-Migration

# Fix the issue and try again
Add-Migration FixedMigrationName
Update-Database
```

---

### **Issue: "Migration already applied"**

**Solution:**
Migrations are idempotent - running them again is safe!

```powershell
Update-Database
```

---

## ?? **Migration Best Practices**

### **? DO:**
- Create migrations for every model change
- Use descriptive migration names
- Test migrations before deploying
- Keep migrations small and focused
- Commit migrations to Git

### **? DON'T:**
- Edit applied migrations
- Delete migration files manually
- Skip migrations
- Modify the database directly (without migrations)

---

## ?? **Common Scenarios**

### **Scenario 1: New developer joins**

```
1. Clone repository
2. Run the application (F5)
3. Done! Database created automatically
```

---

### **Scenario 2: Update production database**

```
1. Deploy new code (with migrations)
2. Application starts
3. Auto-migration applies new changes
4. Production updated!
```

Or manually:
```powershell
# Generate SQL script
Script-Migration -From LastAppliedMigration -To NewMigration

# Run script on production
```

---

### **Scenario 3: Rollback a migration**

```powershell
# Rollback to specific migration
Update-Database -Migration PreviousMigrationName

# Or use CLI
dotnet ef database update PreviousMigrationName
```

---

## ?? **Project Structure**

```
YourProject/
??? Migrations/
?   ??? 20251119125801_AllOnNewDB.cs
?   ??? 20251119131605_AddServiceID.cs
?   ??? 20251128130407_AddEmployeeToAppointment.cs
?   ??? 20241204120000_AddReceiptPartsTable.cs ?
?   ??? ApplicationDbContextModelSnapshot.cs
??? Data/
?   ??? ApplicationDbContext.cs
??? Models/
?   ??? Receipt.cs
?   ??? ReceiptPart.cs ?
?   ??? Part.cs
?   ??? ... (other models)
??? Program.cs (auto-migration configured)
```

---

## ? **Advantages of Migrations**

| Feature | Migrations | SQL Scripts |
|---------|-----------|-------------|
| **Automatic** | ? Yes | ? Manual |
| **Version Control** | ? Yes | ?? Requires tracking |
| **Rollback** | ? Easy | ? Complex |
| **Team Collaboration** | ? Excellent | ?? Conflicts |
| **Deployment** | ? Automatic | ? Manual |
| **Type Safety** | ? C# Code | ? Raw SQL |
| **Testing** | ? Easy | ?? Requires setup |

---

## ?? **Summary**

### **What You Have:**
? **Complete migration system**
? **Auto-migration on startup**
? **All tables defined in migrations**
? **Seed data included**
? **ReceiptParts table migration ready**

### **What You Don't Need:**
? **SQL scripts**
? **Manual table creation**
? **Manual database updates**
? **Complex deployment procedures**

### **How to Use:**
1. **Make model changes**
2. **Create migration** (if needed)
3. **Run your app** - migrations apply automatically!
4. **Done!** ??

---

## ?? **Quick Commands Reference**

| Task | Package Manager Console | .NET CLI |
|------|------------------------|----------|
| Apply migrations | `Update-Database` | `dotnet ef database update` |
| Create migration | `Add-Migration Name` | `dotnet ef migrations add Name` |
| Remove migration | `Remove-Migration` | `dotnet ef migrations remove` |
| List migrations | `Get-Migration` | `dotnet ef migrations list` |
| Generate SQL | `Script-Migration` | `dotnet ef migrations script` |
| Drop database | `Drop-Database` | `dotnet ef database drop` |

---

**Status:** ? **Fully Configured**  
**SQL Scripts:** ? **Not Needed**  
**Auto-Migration:** ? **Enabled**  
**Ready:** ?? **Yes!**

---

**Created:** December 2024  
**System:** Entity Framework Core 9.0  
**Database:** SQL Server  
**Approach:** Code-First with Migrations  

?? **Just run your app and everything works automatically!**
