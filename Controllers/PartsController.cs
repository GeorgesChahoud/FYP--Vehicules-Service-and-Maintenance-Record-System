using FYP___Vehicules_Service_and_Maintenance_Record_System.Data;
using FYP___Vehicules_Service_and_Maintenance_Record_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Controllers
{
    public class PartsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PartsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // CREATE: Show Form
        public IActionResult Create()
        {
            return View();
        }

        // STORE: Save new part
        [HttpPost]
        public async Task<IActionResult> Store(Part part)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", part);
            }
            await _context.Parts.AddAsync(part);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // EDIT: Show edit form
        public async Task<IActionResult> Edit(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null)
            {
                return NotFound();
            }
            return View(part);
        }

        // UPDATE: Modify part
        [HttpPost]
        public async Task<IActionResult> Edit(Part part)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", part);
            }
            _context.Parts.Update(part);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // DELETE: Remove a part
        public async Task<IActionResult> Delete(int id)
        {
            var part = _context.Parts.Find(id);
            if (part == null)
            {
                return NotFound();
            }
            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var parts = await _context.Parts.ToListAsync();
            return View(parts);
        }
    }
}
