using FYP___Vehicules_Service_and_Maintenance_Record_System.Data;
using FYP___Vehicules_Service_and_Maintenance_Record_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
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

                // Combine service info with special request
                string requestNote = $"Service: {service.ServiceName}";
                if (!string.IsNullOrEmpty(SpecialRequest))
                {
                    requestNote += $" | Special Request: {SpecialRequest}";
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

                // Store the service selection in a session or ViewBag for admin to see
                // For now, we'll add it as a note. Later you can create AppointmentService table.

                TempData["SuccessMessage"] = $"Appointment booked successfully for {service.ServiceName} on {ScheduleAppointment.ToString("MMM dd, yyyy hh:mm tt")}!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to book appointment. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
