using FYP___Vehicules_Service_and_Maintenance_Record_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Status> Status { get; set; }  
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<ReceiptService> ReceiptServices { get; set; }
        public DbSet<PartService> PartServices { get; set; }
        public DbSet<ReceiptPart> ReceiptParts { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<RegistrationVerification> RegistrationVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().HasData(
                new Role { ID = 1, Name = "Admin"},
                new Role { ID = 2, Name = "Employee"},
                new Role { ID = 3, Name = "Customer"}
            );

            // Seed users with precomputed BCrypt hashed passwords (stable for migrations)
            modelBuilder.Entity<User>().HasData(
                new User { ID = 1, RoleID = 1, FirstName = "Georges", LastName = "Chahoud", Email = "georgeschahoud@carhub-garage.com", Password = "$2a$11$VR6vwW/QVwDI8GGet8J4M.00B4rDjAHYBuBl4SC3xIsn5ZQ72KXU.", PhoneNumber = "+96103021684" },
                new User { ID = 2, RoleID = 1, FirstName = "Christopher", LastName = "Hanna Nehme", Email = "christopherhannanehme@carhub-garage.com", Password = "$2a$11$YLoFs2ydeRPaR5Vjg5twNeiHOVusJypXzv5YrFrSASL9cBkggEyfO", PhoneNumber = "+96181651808" },
                new User { ID = 3, RoleID = 1, FirstName = "Elias", LastName = "Azar", Email = "eliasazar@carhub-garage.com", Password = "$2a$11$Y7B.ir.5SY3eJ46hJkU0JufQtZTdFo13TK5C1i6FVHgQAD6wz68Fu", PhoneNumber = "+96171750758" },
                new User { ID = 4, RoleID = 1, FirstName = "Anthony", LastName = "Chahine", Email = "anthonychahine@carhub-garage.com", Password = "$2a$11$n1K1/r8vUJ2pKXA9SmT2qegZbSKm5KQyy1WvAlnK9umNeK3W/4DXC", PhoneNumber = "+96181866298" }
            );

            modelBuilder.Entity<Admin>().HasData(
                new Admin { ID = 1, UserID = 1 },
                new Admin { ID = 2, UserID = 2 },
                new Admin { ID = 3, UserID = 3 },
                new Admin { ID = 4, UserID = 4 }
            );

            modelBuilder.Entity<Status>().HasData(
               new Status { ID = 1, Name = "Pending" },
               new Status { ID = 2, Name = "Confirmed" },
               new Status { ID = 3, Name = "In Progress" },
               new Status { ID = 4, Name = "Completed" },
               new Status { ID = 5, Name = "Cancelled" }
           );


            //// ---------------- Admin <-> User ----------------
            //modelBuilder.Entity<Admin>()
            //    .HasOne(c => c.User)
            //    .WithOne()
            //    .HasForeignKey<User>(c => c.ID)
            //    .OnDelete(DeleteBehavior.Restrict);
            //// ---------------- Employee <-> User ----------------
            //modelBuilder.Entity<Employee>()
            //    .HasOne(c => c.User)
            //    .WithOne()
            //    .HasForeignKey<User>(c => c.ID)
            //    .OnDelete(DeleteBehavior.Restrict);
            //// ---------------- Customer <-> User ----------------
            //modelBuilder.Entity<Customer>()
            //    .HasOne(c => c.User)
            //    .WithOne()
            //    .HasForeignKey<User>(c => c.ID)
            //    .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
