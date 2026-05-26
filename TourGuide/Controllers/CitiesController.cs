using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuide.Data;
using TourGuide.Models;

namespace TourGuide.Controllers
{
    public class CitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Главная страница - список городов
        public async Task<IActionResult> Index(string search)
        {
            var cities = _context.Cities.Include(c => c.Attractions).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                cities = cities.Where(c => c.Name.Contains(search));
                ViewBag.Search = search;
            }

            return View(await cities.ToListAsync());
        }

        // Детальная страница города
        public async Task<IActionResult> Details(int id)
        {
            var city = await _context.Cities
                .Include(c => c.Attractions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }
    }
}
