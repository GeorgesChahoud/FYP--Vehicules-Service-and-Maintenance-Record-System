using FYP___Vehicules_Service_and_Maintenance_Record_System.Data;
using FYP___Vehicules_Service_and_Maintenance_Record_System.Models;
using FYP___Vehicules_Service_and_Maintenance_Record_System.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(
            ApplicationDbContext context,
            IEmailService emailService,
            IEncryptionService encryptionService,
            ILogger<AppointmentController> logger)
        {
            _context = context;
            _emailService = emailService;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        // Helper method to get current user ID
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        // VIEW CUSTOMER APPOINTMENTS
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            try
            {
                var appointments = await _context.Appointments
                    .Include(a => a.Car)
                    .Include(a => a.Status)
                    .Include(a => a.Service)
                    .Where(a => a.Car.UserID == userId)
                    .OrderByDescending(a => a.ScheduleAppointment)
                    .ToListAsync();

                return View(appointments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading appointments.";
                return View(new List<Appointment>());
            }
        }

        // CANCEL APPOINTMENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Car)
                    .FirstOrDefaultAsync(a => a.ID == id && a.Car.UserID == userId);

                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Appointment not found.";
                    return RedirectToAction("Index");
                }

                // Set status to Cancelled (StatusID = 5)
                appointment.StatusID = 5;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Appointment cancelled successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error cancelling appointment.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(
            int CarID,
            int ServiceID,
            DateTime ScheduleAppointment,
            string? SpecialRequest)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["ErrorMessage"] = "Please login to book an appointment.";
                return RedirectToAction("CustomerLogin", "Auth");
            }

            if (CarID == 0)
            {
                TempData["ErrorMessage"] = "Please select a car.";
                return RedirectToAction("Index", "Home");
            }

            if (ServiceID == 0)
            {
                TempData["ErrorMessage"] = "Please select a service.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Verify the car belongs to the user
                int userId = int.Parse(userIdClaim.Value);
                var car = await _context.Cars
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.ID == CarID && c.UserID == userId);

                if (car == null)
                {
                    TempData["ErrorMessage"] = "Invalid car selection.";
                    return RedirectToAction("Index", "Home");
                }

                // Verify the service exists
                var service = await _context.Services.FirstOrDefaultAsync(s => s.ID == ServiceID);
                if (service == null)
                {
                    TempData["ErrorMessage"] = "Invalid service selection.";
                    return RedirectToAction("Index", "Home");
                }

                // Create appointment with Pending status (StatusID = 1)
                var appointment = new Appointment
                {
                    CarID = CarID,
                    StatusID = 1, // Pending status
                    ServiceID = ServiceID,
                    ScheduleAppointment = ScheduleAppointment
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Appointment created successfully. ID: {appointment.ID}");

                // Send confirmation email with calendar invitation
                bool emailSent = false;
                string emailError = null;
                try
                {
                    _logger.LogInformation("Starting email sending process...");
                    
                    // Customer emails are stored as plain text (not encrypted)
                    // Only Admin and Employee emails are encrypted
                    string customerEmail = car.User.Email;
                    _logger.LogInformation($"Customer email: {customerEmail}");
                    
                    var customerName = $"{car.User.FirstName} {car.User.LastName}";
                    var carDetails = $"{car.Year} {car.Make} {car.Model} - Plate: {car.PlateNumber}";

                    _logger.LogInformation($"Sending email to {customerEmail}...");
                    
                    await _emailService.SendAppointmentConfirmationEmailAsync(
                        customerEmail,
                        customerName,
                        service.ServiceName,
                        ScheduleAppointment,
                        carDetails,
                        SpecialRequest
                    );

                    emailSent = true;
                    _logger.LogInformation($"? Appointment confirmation email sent successfully to {customerEmail}");
                }
                catch (Exception emailEx)
                {
                    emailError = emailEx.Message;
                    _logger.LogError($"? Failed to send appointment confirmation email: {emailEx.Message}");
                    _logger.LogError($"Stack trace: {emailEx.StackTrace}");
                    // Continue even if email fails - appointment is still created
                }

                // Provide feedback based on email status
                if (emailSent)
                {
                    TempData["SuccessMessage"] = $"? Appointment booked successfully for {service.ServiceName} on {ScheduleAppointment.ToString("MMM dd, yyyy hh:mm tt")}! A confirmation email with calendar invitation has been sent to your email.";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Appointment booked successfully for {service.ServiceName} on {ScheduleAppointment.ToString("MMM dd, yyyy hh:mm tt")}!";
                    TempData["ErrorMessage"] = $"?? Note: We couldn't send the confirmation email. Error: {emailError}. Please check your My Appointments page.";
                }
                
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to book appointment: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Failed to book appointment. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
