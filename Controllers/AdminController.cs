using FYP___Vehicules_Service_and_Maintenance_Record_System.Data;
using FYP___Vehicules_Service_and_Maintenance_Record_System.Models;
using FYP___Vehicules_Service_and_Maintenance_Record_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEncryptionService _encryptionService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            IPasswordHasher passwordHasher,
            IEncryptionService encryptionService,
            IEmailService emailService,
            ILogger<AdminController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _encryptionService = encryptionService;
            _emailService = emailService;
            _logger = logger;
        }

        // DASHBOARD
        public async Task<IActionResult> Index()
        {
            var employeeCount = await _context.Employees.CountAsync();
            var customerCount = await _context.Customers.CountAsync();
            var serviceCount = await _context.Services.CountAsync();
            var partCount = await _context.Parts.CountAsync();
            var appointmentCount = await _context.Appointments.CountAsync();

            ViewBag.EmployeeCount = employeeCount;
            ViewBag.CustomerCount = customerCount;
            ViewBag.ServiceCount = serviceCount;
            ViewBag.PartCount = partCount;
            ViewBag.AppointmentCount = appointmentCount;

            return View();
        }

        #region EMPLOYEE MANAGEMENT

        // LIST ALL EMPLOYEES
        public async Task<IActionResult> Employees()
        {
            try
            {
                var employees = await _context.Employees
                    .Include(e => e.User)
                    .ToListAsync();
                
                // Decrypt emails for display
                if (employees != null && employees.Any())
                {
                    foreach (var employee in employees)
                    {
                        if (employee.User != null && !string.IsNullOrEmpty(employee.User.Email))
                        {
                            try
                            {
                                employee.User.Email = _encryptionService.Decrypt(employee.User.Email);
                            }
                            catch (Exception)
                            {
                                // If decryption fails, leave email as is or set to error message
                                employee.User.Email = "Error decrypting email";
                            }
                        }
                    }
                }

                return View(employees ?? new List<Employee>());
            }
            catch (Exception ex)
            {
                // Log the error and show friendly message
                TempData["ErrorMessage"] = $"Error loading employees: {ex.Message}";
                return View(new List<Employee>());
            }
        }

        // CREATE EMPLOYEE - GET
        public IActionResult CreateEmployee()
        {
            return View();
        }

        // CREATE EMPLOYEE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(
            string FirstName,
            string LastName,
            string Email,
            string PhoneNumber,
            string Password,
            string Shift,
            string WorkHours,
            double FeeByService)
        {
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) ||
                string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) ||
                string.IsNullOrEmpty(PhoneNumber) || string.IsNullOrEmpty(Shift) ||
                string.IsNullOrEmpty(WorkHours))
            {
                ViewBag.ErrorMessage = "All fields are required.";
                return View();
            }

            try
            {
                var encryptedEmail = _encryptionService.Encrypt(Email);

                // Check if email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == encryptedEmail);

                if (existingUser != null)
                {
                    ViewBag.ErrorMessage = "An account with this email already exists.";
                    return View();
                }

                // Create User
                var user = new User
                {
                    RoleID = 2, // Employee role
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = encryptedEmail,
                    Password = _passwordHasher.HashPassword(Password),
                    PhoneNumber = PhoneNumber
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create Employee profile
                var employee = new Employee
                {
                    UserID = user.ID,
                    Shift = Shift,
                    WorkHours = WorkHours,
                    FeeByService = FeeByService
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Employee created successfully!";
                return RedirectToAction("Employees");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while creating the employee.";
                return View();
            }
        }

        // EDIT EMPLOYEE - GET
        public async Task<IActionResult> EditEmployee(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.ID == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Decrypt email for display
            employee.User.Email = _encryptionService.Decrypt(employee.User.Email);

            return View(employee);
        }

        // EDIT EMPLOYEE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(
            int ID,
            int UserID,
            string FirstName,
            string LastName,
            string Email,
            string PhoneNumber,
            string Password,
            string Shift,
            string WorkHours,
            double FeeByService)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.ID == ID);

            if (employee == null)
            {
                return NotFound();
            }

            try
            {
                var encryptedEmail = _encryptionService.Encrypt(Email);

                // Check if email is being changed and if new email already exists
                if (employee.User.Email != encryptedEmail)
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == encryptedEmail && u.ID != UserID);

                    if (existingUser != null)
                    {
                        ViewBag.ErrorMessage = "An account with this email already exists.";
                        employee.User.Email = _encryptionService.Decrypt(employee.User.Email);
                        return View(employee);
                    }
                }

                // Update User
                employee.User.FirstName = FirstName;
                employee.User.LastName = LastName;
                employee.User.Email = encryptedEmail;
                employee.User.PhoneNumber = PhoneNumber;

                // Update password only if provided
                if (!string.IsNullOrEmpty(Password))
                {
                    employee.User.Password = _passwordHasher.HashPassword(Password);
                }

                // Update Employee
                employee.Shift = Shift;
                employee.WorkHours = WorkHours;
                employee.FeeByService = FeeByService;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Employee updated successfully!";
                return RedirectToAction("Employees");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while updating the employee.";
                employee.User.Email = _encryptionService.Decrypt(employee.User.Email);
                return View(employee);
            }
        }

        // DELETE EMPLOYEE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.ID == id);

            if (employee == null)
            {
                return NotFound();
            }

            try
            {
                _context.Employees.Remove(employee);
                _context.Users.Remove(employee.User);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Employee deleted successfully!";
                return RedirectToAction("Employees");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the employee.";
                return RedirectToAction("Employees");
            }
        }

        #endregion

        #region SERVICE MANAGEMENT

        // LIST ALL SERVICES
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services.ToListAsync();
            return View(services);
        }

        // CREATE SERVICE - GET
        public IActionResult CreateService()
        {
            return View();
        }

        // CREATE SERVICE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(Service service)
        {
            if (string.IsNullOrEmpty(service.ServiceName) || string.IsNullOrEmpty(service.Description))
            {
                ViewBag.ErrorMessage = "All fields are required.";
                return View(service);
            }

            try
            {
                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Service created successfully!";
                return RedirectToAction("Services");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while creating the service.";
                return View(service);
            }
        }

        // EDIT SERVICE - GET
        public async Task<IActionResult> EditService(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // EDIT SERVICE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(Service service)
        {
            if (string.IsNullOrEmpty(service.ServiceName) || string.IsNullOrEmpty(service.Description))
            {
                ViewBag.ErrorMessage = "All fields are required.";
                return View(service);
            }

            try
            {
                _context.Services.Update(service);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Service updated successfully!";
                return RedirectToAction("Services");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while updating the service.";
                return View(service);
            }
        }

        // DELETE SERVICE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            try
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Service deleted successfully!";
                return RedirectToAction("Services");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the service.";
                return RedirectToAction("Services");
            }
        }

        #endregion

        #region PART MANAGEMENT

        // LIST ALL PARTS
        public async Task<IActionResult> Parts()
        {
            var parts = await _context.Parts.ToListAsync();
            return View(parts);
        }

        // CREATE PART - GET
        public IActionResult CreatePart()
        {
            return View();
        }

        // CREATE PART - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePart(Part part)
        {
            if (string.IsNullOrEmpty(part.PartName))
            {
                ViewBag.ErrorMessage = "Part name is required.";
                return View(part);
            }

            try
            {
                // Set stock status based on quantity
                part.Stock = part.Quantity > 0;

                _context.Parts.Add(part);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Part created successfully!";
                return RedirectToAction("Parts");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while creating the part.";
                return View(part);
            }
        }

        // EDIT PART - GET
        public async Task<IActionResult> EditPart(int id)
        {
            var part = await _context.Parts.FindAsync(id);

            if (part == null)
            {
                return NotFound();
            }

            return View(part);
        }

        // EDIT PART - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPart(Part part)
        {
            if (string.IsNullOrEmpty(part.PartName))
            {
                ViewBag.ErrorMessage = "Part name is required.";
                return View(part);
            }

            try
            {
                // Update stock status based on quantity
                part.Stock = part.Quantity > 0;

                _context.Parts.Update(part);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Part updated successfully!";
                return RedirectToAction("Parts");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while updating the part.";
                return View(part);
            }
        }

        // DELETE PART
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePart(int id)
        {
            var part = await _context.Parts.FindAsync(id);

            if (part == null)
            {
                return NotFound();
            }

            try
            {
                _context.Parts.Remove(part);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Part deleted successfully!";
                return RedirectToAction("Parts");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the part.";
                return RedirectToAction("Parts");
            }
        }

        #endregion

        #region CUSTOMER MANAGEMENT

        // VIEW ALL CUSTOMERS
        public async Task<IActionResult> Customers()
        {
            try
            {
                var customers = await _context.Customers
                    .Include(c => c.User)
                        .ThenInclude(u => u.Cars)
                    .ToListAsync();

                // Customer emails are stored as plain text (not encrypted)
                // No need to decrypt them
                return View(customers ?? new List<Customer>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading customers: {ex.Message}";
                return View(new List<Customer>());
            }
        }

        // CREATE CUSTOMER - GET
        public IActionResult CreateCustomer()
        {
            return View();
        }

        // CREATE CUSTOMER - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomer(
            string FirstName,
            string LastName,
            string Email,
            string PhoneNumber,
            string Address,
            string Password)
        {
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) ||
                string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) ||
                string.IsNullOrEmpty(Address) || string.IsNullOrEmpty(PhoneNumber))
            {
                ViewBag.ErrorMessage = "All fields are required.";
                return View();
            }

            try
            {
                // Customer emails are stored as plain text (not encrypted)
                // Check if email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == Email);

                if (existingUser != null)
                {
                    ViewBag.ErrorMessage = "An account with this email already exists.";
                    return View();
                }

                // Create User (customer email is plain text)
                var user = new User
                {
                    RoleID = 3, // Customer role
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,  // Store as plain text
                    Password = _passwordHasher.HashPassword(Password),
                    PhoneNumber = PhoneNumber
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create Customer profile
                var customer = new Customer
                {
                    UserID = user.ID,
                    Address = Address
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Customer created successfully!";
                return RedirectToAction("Customers");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while creating the customer.";
                return View();
            }
        }

        // EDIT CUSTOMER - GET
        public async Task<IActionResult> EditCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Customer email is stored as plain text (no need to decrypt)
            return View(customer);
        }

        // EDIT CUSTOMER - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(
            int ID,
            int UserID,
            string FirstName,
            string LastName,
            string Email,
            string PhoneNumber,
            string Address,
            string Password)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ID == ID);

            if (customer == null)
            {
                return NotFound();
            }

            try
            {
                // Customer emails are stored as plain text (not encrypted)
                // Check if email is being changed and if new email already exists
                if (customer.User.Email != Email)
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == Email && u.ID != UserID);

                    if (existingUser != null)
                    {
                        ViewBag.ErrorMessage = "An account with this email already exists.";
                        return View(customer);
                    }
                }

                // Update User (customer email is plain text)
                customer.User.FirstName = FirstName;
                customer.User.LastName = LastName;
                customer.User.Email = Email;  // Store as plain text
                customer.User.PhoneNumber = PhoneNumber;

                // Update password only if provided
                if (!string.IsNullOrEmpty(Password))
                {
                    customer.User.Password = _passwordHasher.HashPassword(Password);
                }

                // Update Customer
                customer.Address = Address;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Customer updated successfully!";
                return RedirectToAction("Customers");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while updating the customer.";
                return View(customer);
            }
        }

        // DELETE CUSTOMER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                    .ThenInclude(u => u.Cars)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (customer == null)
            {
                return NotFound();
            }

            try
            {
                // Remove associated cars first
                if (customer.User.Cars != null && customer.User.Cars.Any())
                {
                    _context.Cars.RemoveRange(customer.User.Cars);
                }

                // Remove customer and user
                _context.Customers.Remove(customer);
                _context.Users.Remove(customer.User);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Customer deleted successfully!";
                return RedirectToAction("Customers");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the customer.";
                return RedirectToAction("Customers");
            }
        }

        #endregion

        #region APPOINTMENT MANAGEMENT

        // LIST ALL APPOINTMENTS
        public async Task<IActionResult> Appointments()
        {
            try
            {
                var appointments = await _context.Appointments
                    .Include(a => a.Car)
                        .ThenInclude(c => c.User)
                    .Include(a => a.Status)
                    .Include(a => a.Service)
                    .OrderByDescending(a => a.ScheduleAppointment)
                    .ToListAsync();

                // Customer emails are stored as plain text (not encrypted)
                // No need to decrypt them
                return View(appointments ?? new List<Appointment>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading appointments: {ex.Message}";
                return View(new List<Appointment>());
            }
        }

        // UPDATE APPOINTMENT STATUS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, int statusId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Car)
                        .ThenInclude(c => c.User)
                    .Include(a => a.Status)
                    .Include(a => a.Service)
                    .FirstOrDefaultAsync(a => a.ID == id);
                
                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Appointment not found.";
                    return RedirectToAction("Appointments");
                }

                // Check if appointment is already cancelled - prevent any status changes
                if (appointment.Status?.Name == "Cancelled")
                {
                    _logger.LogWarning($"Attempted to modify cancelled appointment {id}");
                    TempData["ErrorMessage"] = "⚠️ Cannot modify a cancelled appointment. Cancelled appointments are locked and cannot be updated.";
                    return RedirectToAction("Appointments");
                }

                // Get the old status name for logging
                var oldStatusName = appointment.Status?.Name ?? "Unknown";

                // Update the status
                appointment.StatusID = statusId;
                await _context.SaveChangesAsync();

                // Reload to get the new status name
                await _context.Entry(appointment).Reference(a => a.Status).LoadAsync();
                var newStatusName = appointment.Status?.Name ?? "Unknown";

                _logger.LogInformation($"Appointment {id} status updated from '{oldStatusName}' to '{newStatusName}'");

                // Send email notification to customer
                try
                {
                    if (appointment.Car?.User != null)
                    {
                        // Customer emails are stored as plain text
                        string customerEmail = appointment.Car.User.Email;
                        string customerName = $"{appointment.Car.User.FirstName} {appointment.Car.User.LastName}";
                        string carDetails = $"{appointment.Car.Year} {appointment.Car.Make} {appointment.Car.Model} - Plate: {appointment.Car.PlateNumber}";
                        string serviceName = appointment.Service?.ServiceName ?? "Service";

                        _logger.LogInformation($"Sending status update email to {customerEmail}");

                        await _emailService.SendAppointmentStatusUpdateEmailAsync(
                            customerEmail,
                            customerName,
                            newStatusName,
                            serviceName,
                            appointment.ScheduleAppointment,
                            carDetails
                        );

                        _logger.LogInformation($"✅ Status update email sent successfully to {customerEmail}");
                        TempData["SuccessMessage"] = $"Appointment status updated to '{newStatusName}' and customer has been notified via email!";
                    }
                    else
                    {
                        _logger.LogWarning($"No customer information found for appointment {id}");
                        TempData["SuccessMessage"] = "Appointment status updated successfully!";
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError($"❌ Failed to send status update email: {emailEx.Message}");
                    TempData["SuccessMessage"] = $"Appointment status updated to '{newStatusName}' successfully!";
                    TempData["ErrorMessage"] = $"⚠️ Note: Could not send email notification. Error: {emailEx.Message}";
                }

                return RedirectToAction("Appointments");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating appointment status: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while updating the appointment status.";
                return RedirectToAction("Appointments");
            }
        }

        // DELETE APPOINTMENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);

                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Appointment not found.";
                    return RedirectToAction("Appointments");
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Appointment deleted successfully!";
                return RedirectToAction("Appointments");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the appointment.";
                return RedirectToAction("Appointments");
            }
        }

        #endregion
    }
}
