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

            modelBuilder.Entity<User>().HasData(
               new User { ID = 1, RoleID = 1, FirstName = "Georges", LastName = "Chahoud", Email = "georgeschahoud@carhub-garage.com", Password = "GCH@Car#9", PhoneNumber = "+96103021684" },
               new User { ID = 2, RoleID = 1, FirstName = "Christopher", LastName = "Hanna Nehme", Email = "christopherhannanehme@carhub-garage.com", Password = "CHN@Car#3", PhoneNumber = "+96181651808" },
               new User { ID = 3, RoleID = 1, FirstName = "Elias", LastName = "Azar", Email = "eliasazar@carhub-garage.com", Password = "EAZ@Car#5", PhoneNumber = "+96171750758" },
               new User { ID = 4, RoleID = 1, FirstName = "Anthony", LastName = "Chahine", Email = "anthonychahine@carhub-garage.com", Password = "ACH@Car#7", PhoneNumber = "+96181866298" }
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
