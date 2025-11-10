using FYP___Vehicules_Service_and_Maintenance_Record_System.Data;
using FYP___Vehicules_Service_and_Maintenance_Record_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // CREATE: Show Form
        public IActionResult Create()
        {
            return View();
        }

        // STORE: Save new service
        [HttpPost]
        public async Task<IActionResult> Store(Service service)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", service);
            }
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // EDIT: Show edit form
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        // UPDATE: Modify service
        [HttpPost]
        public async Task<IActionResult> Edit(Service service)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", service);
            }
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // DELETE: Remove a service
        public async Task<IActionResult> Delete(int id)
        {
            var service = _context.Services.Find(id);
            if (service == null)
            {
                return NotFound();
            }
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.Services.ToListAsync();
            return View(services);
        }
    }
}
