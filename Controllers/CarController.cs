using FYP___Vehicules_Service_and_Maintenance_Record_System.Data;
using FYP___Vehicules_Service_and_Maintenance_Record_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Controllers
{
    public class CarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarController(ApplicationDbContext context)
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

        // LIST: Show all cars for current customer
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            var cars = await _context.Cars
                .Where(c => c.UserID == userId)
                .OrderByDescending(c => c.ID)
                .ToListAsync();

            return View(cars);
        }

        // CREATE: Show Form
        public IActionResult Create()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }
            return View();
        }

        // STORE: Save new car
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car car)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            if (string.IsNullOrEmpty(car.Make) || string.IsNullOrEmpty(car.Model) || 
                string.IsNullOrEmpty(car.PlateNumber) || car.Year == 0)
            {
                ViewBag.ErrorMessage = "Please fill all required fields.";
                return View(car);
            }

            try
            {
                // Check if plate number already exists for this user
                var existingCar = await _context.Cars
                    .FirstOrDefaultAsync(c => c.PlateNumber == car.PlateNumber && c.UserID == userId);

                if (existingCar != null)
                {
                    ViewBag.ErrorMessage = "A car with this plate number already exists in your garage.";
                    return View(car);
                }

                car.UserID = userId;
                await _context.Cars.AddAsync(car);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Car added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An error occurred while adding the car.";
                return View(car);
            }
        }

        // EDIT: Show edit form
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            var car = await _context.Cars
                .FirstOrDefaultAsync(c => c.ID == id && c.UserID == userId);

            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // UPDATE: Modify car
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Car car)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            if (string.IsNullOrEmpty(car.Make) || string.IsNullOrEmpty(car.Model) || 
                string.IsNullOrEmpty(car.PlateNumber) || car.Year == 0)
            {
                ViewBag.ErrorMessage = "Please fill all required fields.";
                return View(car);
            }

            try
            {
                var existingCar = await _context.Cars
                    .FirstOrDefaultAsync(c => c.ID == car.ID && c.UserID == userId);

                if (existingCar == null)
                {
                    return NotFound();
                }

                // Check if plate number already exists for another car
                var duplicateCar = await _context.Cars
                    .FirstOrDefaultAsync(c => c.PlateNumber == car.PlateNumber && 
                                            c.ID != car.ID && c.UserID == userId);

                if (duplicateCar != null)
                {
                    ViewBag.ErrorMessage = "Another car with this plate number already exists in your garage.";
                    return View(car);
                }

                existingCar.Make = car.Make;
                existingCar.Model = car.Model;
                existingCar.Year = car.Year;
                existingCar.PlateNumber = car.PlateNumber;
                existingCar.Vin = car.Vin;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Car updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An error occurred while updating the car.";
                return View(car);
            }
        }

        // DELETE: Remove a car
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            try
            {
                var car = await _context.Cars
                    .FirstOrDefaultAsync(c => c.ID == id && c.UserID == userId);

                if (car == null)
                {
                    return NotFound();
                }

                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Car deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the car.";
                return RedirectToAction("Index");
            }
        }
    }
}
