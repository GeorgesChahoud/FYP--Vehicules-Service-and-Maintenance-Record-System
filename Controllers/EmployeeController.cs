using FYP___Vehicules_Service_and_Maintenance_Record_System.Data;
using FYP___Vehicules_Service_and_Maintenance_Record_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper method to get current employee ID
        private async Task<int?> GetCurrentEmployeeIdAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == userId);
            return employee?.ID;
        }

        // DASHBOARD
        public async Task<IActionResult> Index()
        {
            var employeeId = await GetCurrentEmployeeIdAsync();
            if (employeeId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get statistics for this employee
            var myAppointments = await _context.Appointments
                .Where(a => a.EmployeeID == employeeId)
                .ToListAsync();

            var myCustomers = await _context.Appointments
                .Where(a => a.EmployeeID == employeeId)
                .Include(a => a.Car)
                    .ThenInclude(c => c.User)
                .Select(a => a.Car.User)
                .Distinct()
                .CountAsync();

            var myReceipts = await _context.Receipts
                .Where(r => r.Appointment.EmployeeID == employeeId)
                .CountAsync();

            ViewBag.MyAppointmentCount = myAppointments.Count;
            ViewBag.MyCustomerCount = myCustomers;
            ViewBag.MyReceiptsCount = myReceipts;
            ViewBag.PendingAppointments = myAppointments.Count(a => a.StatusID == 1 || a.StatusID == 2);
            ViewBag.CompletedAppointments = myAppointments.Count(a => a.StatusID == 4);
            ViewBag.TotalParts = await _context.Parts.CountAsync();

            return View();
        }

        // MY CUSTOMERS - List of customers assigned to this employee
        public async Task<IActionResult> MyCustomers()
        {
            var employeeId = await GetCurrentEmployeeIdAsync();
            if (employeeId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var customers = await _context.Appointments
                .Where(a => a.EmployeeID == employeeId)
                .Include(a => a.Car)
                    .ThenInclude(c => c.User)
                        .ThenInclude(u => u.Customer)
                .Include(a => a.Status)
                .Include(a => a.Service)
                .Select(a => new
                {
                    Customer = a.Car.User.Customer,
                    User = a.Car.User,
                    Car = a.Car,
                    LastAppointment = a.ScheduleAppointment,
                    Status = a.Status.Name
                })
                .GroupBy(x => x.Customer.ID)
                .Select(g => g.OrderByDescending(x => x.LastAppointment).First())
                .ToListAsync();

            return View(customers);
        }

        // MY APPOINTMENTS - List of appointments assigned to this employee
        public async Task<IActionResult> MyAppointments(string status = "All")
        {
            var employeeId = await GetCurrentEmployeeIdAsync();
            if (employeeId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get all appointments for this employee first
            var allAppointments = await _context.Appointments
                .Where(a => a.EmployeeID == employeeId)
                .Include(a => a.Car)
                    .ThenInclude(c => c.User)
                .Include(a => a.Status)
                .Include(a => a.Service)
                .Include(a => a.Receipt)
                .OrderByDescending(a => a.ScheduleAppointment)
                .ToListAsync();

            // Apply filter based on status for display
            var filteredAppointments = allAppointments;
            if (status != "All" && !string.IsNullOrEmpty(status))
            {
                filteredAppointments = allAppointments.Where(a => a.Status?.Name == status).ToList();
            }

            ViewBag.FilterStatus = status;
            ViewBag.AllAppointments = allAppointments; // Pass all appointments for accurate counts

            return View(filteredAppointments);
        }

        // UPDATE APPOINTMENT STATUS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, int statusId)
        {
            var employeeId = await GetCurrentEmployeeIdAsync();
            if (employeeId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var appointment = await _context.Appointments
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.ID == id && a.EmployeeID == employeeId);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found or not assigned to you.";
                return RedirectToAction("MyAppointments");
            }

            // Check if appointment is already cancelled - prevent any status changes
            if (appointment.Status?.Name == "Cancelled")
            {
                TempData["ErrorMessage"] = "⚠️ Cannot modify a cancelled appointment. Cancelled appointments are locked and cannot be updated.";
                return RedirectToAction("MyAppointments");
            }

            // Check if appointment is already completed - prevent any status changes
            if (appointment.Status?.Name == "Completed")
            {
                TempData["ErrorMessage"] = "⚠️ Cannot modify a completed appointment. Completed appointments are locked and cannot be updated.";
                return RedirectToAction("MyAppointments");
            }

            appointment.StatusID = statusId;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment status updated successfully!";
            return RedirectToAction("MyAppointments");
        }

        // PARTS INVENTORY - View only
        public async Task<IActionResult> PartsInventory()
        {
            var parts = await _context.Parts
                .OrderBy(p => p.PartName)
                .ToListAsync();

            return View(parts);
        }

        // CREATE RECEIPT - GET
        public async Task<IActionResult> CreateReceipt(int appointmentId)
        {
            var employeeId = await GetCurrentEmployeeIdAsync();
            if (employeeId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var appointment = await _context.Appointments
                .Include(a => a.Car)
                    .ThenInclude(c => c.User)
                .Include(a => a.Service)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(a => a.ID == appointmentId && a.EmployeeID == employeeId);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found or not assigned to you.";
                return RedirectToAction("MyAppointments");
            }

            // Check if receipt already exists
            var existingReceipt = await _context.Receipts
                .FirstOrDefaultAsync(r => r.AppointmentID == appointmentId);

            if (existingReceipt != null)
            {
                TempData["ErrorMessage"] = "Receipt already exists for this appointment.";
                return RedirectToAction("MyAppointments");
            }

            // Get available parts
            var parts = await _context.Parts.Where(p => p.Quantity > 0).ToListAsync();

            ViewBag.Appointment = appointment;
            ViewBag.Parts = parts;

            return View();
        }

        // CREATE RECEIPT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReceipt(int appointmentId, List<int> partIds, List<int> quantities)
        {
            var employeeId = await GetCurrentEmployeeIdAsync();
            if (employeeId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.ID == appointmentId && a.EmployeeID == employeeId);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("MyAppointments");
            }

            try
            {
                // Calculate total
                double total = appointment.Employee.FeeByService;

                var receipt = new Receipt
                {
                    AppointmentID = appointmentId,
                    DateANDTime = DateTime.Now,
                    Total = 0 // Will update after adding parts
                };

                _context.Receipts.Add(receipt);
                await _context.SaveChangesAsync();

                // Add parts to receipt
                if (partIds != null && partIds.Any())
                {
                    for (int i = 0; i < partIds.Count; i++)
                    {
                        var partId = partIds[i];
                        var quantity = quantities[i];

                        if (quantity > 0)
                        {
                            var part = await _context.Parts.FindAsync(partId);
                            if (part != null && part.Quantity >= quantity)
                            {
                                // Add to receipt
                                var receiptPart = new ReceiptPart
                                {
                                    ReceiptID = receipt.ID,
                                    PartID = partId,
                                    QuantityUsed = quantity,
                                    PriceAtTime = part.Price
                                };

                                _context.ReceiptParts.Add(receiptPart);

                                // Update part quantity
                                part.Quantity -= quantity;
                                part.Stock = part.Quantity > 0;

                                // Add to total
                                total += part.Price * quantity;
                            }
                        }
                    }
                }

                // Update receipt total
                receipt.Total = total;

                // Update appointment status to completed
                appointment.StatusID = 4; // Completed

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Receipt created successfully! Total: $" + total.ToString("F2");
                return RedirectToAction("MyAppointments");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error creating receipt: " + ex.Message;
                return RedirectToAction("CreateReceipt", new { appointmentId });
            }
        }

        // VIEW RECEIPT
        public async Task<IActionResult> ViewReceipt(int id)
        {
            var employeeId = await GetCurrentEmployeeIdAsync();
            if (employeeId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var receipt = await _context.Receipts
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Car)
                        .ThenInclude(c => c.User)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Service)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Employee)
                        .ThenInclude(e => e.User)
                .Include(r => r.ReceiptParts)
                    .ThenInclude(rp => rp.Part)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (receipt == null || receipt.Appointment.EmployeeID != employeeId)
            {
                TempData["ErrorMessage"] = "Receipt not found.";
                return RedirectToAction("MyAppointments");
            }

            return View(receipt);
        }

        // MY RECEIPTS - List all receipts created by this employee
        public async Task<IActionResult> MyReceipts()
        {
            var employeeId = await GetCurrentEmployeeIdAsync();
            if (employeeId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var receipts = await _context.Receipts
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Car)
                        .ThenInclude(c => c.User)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Service)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Employee)
                        .ThenInclude(e => e.User)
                .Include(r => r.ReceiptParts)
                    .ThenInclude(rp => rp.Part)
                .Where(r => r.Appointment.EmployeeID == employeeId)
                .OrderByDescending(r => r.DateANDTime)
                .ToListAsync();

            return View(receipts);
        }

        // RECEIPT DETAIL - View detailed receipt from My Receipts page
        public async Task<IActionResult> ReceiptDetail(int id)
        {
            var employeeId = await GetCurrentEmployeeIdAsync();
            if (employeeId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var receipt = await _context.Receipts
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Car)
                        .ThenInclude(c => c.User)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Service)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Employee)
                        .ThenInclude(e => e.User)
                .Include(r => r.ReceiptParts)
                    .ThenInclude(rp => rp.Part)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (receipt == null || receipt.Appointment.EmployeeID != employeeId)
            {
                TempData["ErrorMessage"] = "Receipt not found.";
                return RedirectToAction("MyReceipts");
            }

            return View(receipt);
        }
    }
}
