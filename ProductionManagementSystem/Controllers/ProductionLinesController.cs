using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Models;
using ProductionManagementSystem.Data;

namespace ProductionManagementSystem.Controllers
{
    public class ProductionLinesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductionLinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var lines = await _context.ProductionLines
                .Include(l => l.CurrentWorkOrder)
                .ThenInclude(wo => wo!.Product)
                .ToListAsync();
            return View(lines);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var line = await _context.ProductionLines.FindAsync(id);
            if (line != null)
            {
                line.Status = line.Status == "Active" ? "Stopped" : "Active";
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Линия {line.Name} {(line.Status == "Active" ? "запущена" : "остановлена")}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEfficiency(int id, float factor)
        {
            var line = await _context.ProductionLines.FindAsync(id);
            if (line != null && factor >= 0.5f && factor <= 2.0f)
            {
                line.EfficiencyFactor = factor;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Коэффициент эффективности изменен на {factor}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
