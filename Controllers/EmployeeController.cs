using FYP___Vehicules_Service_and_Maintenance_Record_System.Data;
using FYP___Vehicules_Service_and_Maintenance_Record_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // CREATE: Show Form
        public IActionResult Create()
        {
            return View();
        }

        // STORE: Save new employee
        [HttpPost]
        public async Task<IActionResult> Store(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", employee);
            }
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // EDIT: Show edit form
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // UPDATE: Modify employee
        [HttpPost]
        public async Task<IActionResult> Edit(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", employee);
            }
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // DELETE: Remove a employee
        public async Task<IActionResult> Delete(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees.ToListAsync();
            return View(employees);
        }
    }
}
