using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Models;
using ProductionManagementSystem.ViewModels;
using ProductionManagementSystem.Data;

namespace ProductionManagementSystem.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string category, string search)
        {
            var productsQuery = _context.Products
                .Include(p => p.ProductMaterials)!
                .ThenInclude(pm => pm.Material)
                .AsQueryable();

            var products = await productsQuery.ToListAsync();

            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category == category).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search)).ToList();
            }

            var categories = await _context.Products.Select(p => p.Category).Distinct().ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;

            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new ProductMaterialsViewModel
            {
                AvailableMaterials = await _context.Materials.ToListAsync(),
                Categories = new List<string> { "Электроника", "Мебель", "Одежда", "Обувь", "Продукты питания", "Другое" }
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, List<int> materialIds, List<decimal> quantities, string newCategory)
        {
            // Обработка категории
            if (product.Category == "Другое" && !string.IsNullOrEmpty(newCategory))
            {
                product.Category = newCategory;
            }
            
            if (string.IsNullOrEmpty(product.Category))
            {
                TempData["Error"] = "Пожалуйста, выберите категорию";
                var viewModel = new ProductMaterialsViewModel
                {
                    AvailableMaterials = await _context.Materials.ToListAsync(),
                    Categories = new List<string> { "Электроника", "Мебель", "Одежда", "Обувь", "Продукты питания", "Другое" }
                };
                return View(viewModel);
            }
            
            if (string.IsNullOrEmpty(product.Name))
            {
                TempData["Error"] = "Введите название продукта";
                var viewModel = new ProductMaterialsViewModel
                {
                    AvailableMaterials = await _context.Materials.ToListAsync(),
                    Categories = new List<string> { "Электроника", "Мебель", "Одежда", "Обувь", "Продукты питания", "Другое" }
                };
                return View(viewModel);
            }
            
            if (product.ProductionTimePerUnit <= 0)
            {
                TempData["Error"] = "Введите корректное время производства";
                var viewModel = new ProductMaterialsViewModel
                {
                    AvailableMaterials = await _context.Materials.ToListAsync(),
                    Categories = new List<string> { "Электроника", "Мебель", "Одежда", "Обувь", "Продукты питания", "Другое" }
                };
                return View(viewModel);
            }
            
            try
            {
                // Устанавливаем значения по умолчанию
                product.Specifications = "{}";
                if (product.MinimalStock == 0) product.MinimalStock = 1;
                
                Console.WriteLine($"Создание продукта: {product.Name}, Категория: {product.Category}, Время: {product.ProductionTimePerUnit}");
                
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Продукт сохранен с ID: {product.Id}");
                
                // Добавляем материалы если есть
                if (materialIds != null && quantities != null)
                {
                    for (int i = 0; i < materialIds.Count; i++)
                    {
                        if (materialIds[i] > 0 && quantities[i] > 0)
                        {
                            _context.ProductMaterials.Add(new ProductMaterial
                            {
                                ProductId = product.Id,
                                MaterialId = materialIds[i],
                                QuantityNeeded = quantities[i]
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                
                TempData["Success"] = $"Продукт \"{product.Name}\" успешно создан!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                TempData["Error"] = $"Ошибка при создании: {ex.Message}";
                var viewModel = new ProductMaterialsViewModel
                {
                    AvailableMaterials = await _context.Materials.ToListAsync(),
                    Categories = new List<string> { "Электроника", "Мебель", "Одежда", "Обувь", "Продукты питания", "Другое" }
                };
                return View(viewModel);
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Продукт \"{product.Name}\" удален";
            }
            return RedirectToAction(nameof(Index));
        }
        
        // API метод для получения всех продуктов (для отладки)
        public async Task<IActionResult> DebugList()
        {
            var products = await _context.Products.ToListAsync();
            return Json(products);
        }
    }
}
