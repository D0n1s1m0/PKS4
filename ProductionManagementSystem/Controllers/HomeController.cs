using Microsoft.AspNetCore.Mvc;
using ProductionManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace ProductionManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalProducts = await _context.Products.CountAsync();
            var activeLines = await _context.ProductionLines.CountAsync(l => l.Status == "Active");
            var pendingOrders = await _context.WorkOrders.CountAsync(o => o.Status == "Pending");
            
            // Исправляем проблему с decimal - сначала получаем данные, потом суммируем в памяти
            var materials = await _context.Materials.ToListAsync();
            var totalMaterials = materials.Sum(m => m.Quantity);

            ViewBag.TotalProducts = totalProducts;
            ViewBag.ActiveLines = activeLines;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.TotalMaterials = totalMaterials;

            var recentOrders = await _context.WorkOrders
                .Include(o => o.Product)
                .OrderByDescending(o => o.StartDate)
                .Take(5)
                .ToListAsync();

            return View(recentOrders);
        }
    }
}
