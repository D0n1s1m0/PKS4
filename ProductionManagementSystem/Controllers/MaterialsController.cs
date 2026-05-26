using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Models;
using ProductionManagementSystem.Data;

namespace ProductionManagementSystem.Controllers
{
    public class MaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaterialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var materials = await _context.Materials.ToListAsync();
            return View(materials);
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name,Quantity,UnitOfMeasure,MinimalStock")] Material material)
        {
            if (ModelState.IsValid)
            {
                _context.Add(material);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Материал добавлен";
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Restock(int id, decimal amount)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material != null)
            {
                material.Quantity += amount;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Добавлено {amount} {material.UnitOfMeasure}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
