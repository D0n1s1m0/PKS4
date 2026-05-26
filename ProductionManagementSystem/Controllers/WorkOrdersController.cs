using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;
using System.Timers;

namespace ProductionManagementSystem.Controllers
{
    public class WorkOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private static System.Timers.Timer? _autoProgressTimer;
        private static bool _isTimerRunning = false;

        public WorkOrdersController(ApplicationDbContext context)
        {
            _context = context;
            StartAutoProgressTimer();
        }

        private void StartAutoProgressTimer()
        {
            if (!_isTimerRunning)
            {
                _isTimerRunning = true;
                _autoProgressTimer = new System.Timers.Timer(2000);
                _autoProgressTimer.Elapsed += async (sender, e) => await AutoUpdateProgress();
                _autoProgressTimer.AutoReset = true;
                _autoProgressTimer.Start();
                Console.WriteLine("Таймер автоматического обновления прогресса запущен");
            }
        }

        private async Task AutoUpdateProgress()
        {
            try
            {
                using (var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite("Data Source=production.db")
                    .Options))
                {
                    var now = DateTime.Now;
                    var activeOrders = context.WorkOrders
                        .Where(o => o.Status == "InProgress")
                        .ToList();

                    foreach (var order in activeOrders)
                    {
                        // Рассчитываем прогресс на основе времени
                        var totalMinutes = (order.EstimatedEndDate - order.StartDate).TotalMinutes;
                        if (totalMinutes <= 0) continue;
                        
                        var elapsedMinutes = (now - order.StartDate).TotalMinutes;
                        var newProgress = (int)((elapsedMinutes / totalMinutes) * 100);
                        
                        if (newProgress > 100) newProgress = 100;
                        if (newProgress < 0) newProgress = 0;
                        
                        // Обновляем только если прогресс изменился
                        if (order.Progress != newProgress)
                        {
                            order.Progress = newProgress;
                            Console.WriteLine($"Заказ #{order.Id}: автообновление до {newProgress}% (от {order.StartDate:HH:mm:ss} до {order.EstimatedEndDate:HH:mm:ss})");
                            
                            if (newProgress >= 100)
                            {
                                order.Status = "Completed";
                                if (order.ProductionLineId.HasValue)
                                {
                                    var line = context.ProductionLines.Find(order.ProductionLineId);
                                    if (line != null) line.CurrentWorkOrderId = null;
                                }
                                Console.WriteLine($"Заказ #{order.Id} автоматически завершен!");
                            }
                            
                            await context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка автообновления: {ex.Message}");
            }
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.WorkOrders
                .Include(o => o.Product)
                .Include(o => o.ProductionLine)
                .OrderByDescending(o => o.Id)
                .ToListAsync();
            
            return View(orders);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Lines = await _context.ProductionLines.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int productId, int quantity, int? productionLineId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Продукт не найден";
                return RedirectToAction(nameof(Create));
            }

            var productMaterials = await _context.ProductMaterials
                .Include(pm => pm.Material)
                .Where(pm => pm.ProductId == productId)
                .ToListAsync();

            bool hasEnoughMaterials = true;
            foreach (var pm in productMaterials)
            {
                if (pm.Material != null)
                {
                    decimal needed = pm.QuantityNeeded * quantity;
                    if (pm.Material.Quantity < needed)
                    {
                        hasEnoughMaterials = false;
                        TempData["Error"] = $"Недостаточно материала: {pm.Material.Name}. Нужно: {needed}, есть: {pm.Material.Quantity}";
                        break;
                    }
                }
            }

            if (!hasEnoughMaterials)
            {
                ViewBag.Products = await _context.Products.ToListAsync();
                ViewBag.Lines = await _context.ProductionLines.ToListAsync();
                return View();
            }

            var line = productionLineId.HasValue ? await _context.ProductionLines.FindAsync(productionLineId) : null;
            float efficiency = line?.EfficiencyFactor ?? 1.0f;
            int timeMinutes = (int)(quantity * product.ProductionTimePerUnit / efficiency);
            DateTime estimatedEnd = DateTime.Now.AddMinutes(timeMinutes);

            var order = new WorkOrder
            {
                ProductId = productId,
                Quantity = quantity,
                ProductionLineId = productionLineId,
                StartDate = DateTime.Now,
                EstimatedEndDate = estimatedEnd,
                Status = "Pending",
                Progress = 0
            };

            _context.WorkOrders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var pm in productMaterials)
            {
                if (pm.Material != null)
                {
                    pm.Material.Quantity -= pm.QuantityNeeded * quantity;
                }
            }
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Заказ #{order.Id} создан. Расчетное время: {timeMinutes} минут";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> StartOrder(int id)
        {
            var order = await _context.WorkOrders
                .Include(o => o.ProductionLine)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order != null && order.Status == "Pending")
            {
                order.Status = "InProgress";
                order.StartDate = DateTime.Now;
                order.Progress = 0;
                
                if (order.ProductionLine != null)
                {
                    float efficiency = order.ProductionLine.EfficiencyFactor;
                    var product = await _context.Products.FindAsync(order.ProductId);
                    if (product != null)
                    {
                        int timeMinutes = (int)(order.Quantity * product.ProductionTimePerUnit / efficiency);
                        order.EstimatedEndDate = DateTime.Now.AddMinutes(timeMinutes);
                    }
                }
                
                if (order.ProductionLine != null)
                {
                    order.ProductionLine.CurrentWorkOrderId = id;
                    order.ProductionLine.Status = "Active";
                }
                
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Заказ #{order.Id} запущен в производство";
            }
            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        public async Task<IActionResult> UpdateProgress(int id, int progress)
        {
            Console.WriteLine($"=== РУЧНОЕ ОБНОВЛЕНИЕ: заказ {id}, новое значение {progress}% ===");
            
            var order = await _context.WorkOrders.FindAsync(id);
            if (order != null && order.Status == "InProgress")
            {
                var oldProgress = order.Progress;
                progress = Math.Clamp(progress, 0, 100);
                
                // Получаем общее время производства
                var product = await _context.Products.FindAsync(order.ProductId);
                var line = order.ProductionLineId.HasValue ? await _context.ProductionLines.FindAsync(order.ProductionLineId) : null;
                float efficiency = line?.EfficiencyFactor ?? 1.0f;
                int totalTimeMinutes = (int)(order.Quantity * product!.ProductionTimePerUnit / efficiency);
                
                // Рассчитываем сколько времени уже прошло на основе нового прогресса
                var elapsedMinutes = totalTimeMinutes * (progress / 100.0);
                
                // Устанавливаем StartDate так, чтобы при текущем времени прогресс был равен введенному
                var newStartDate = DateTime.Now.AddMinutes(-elapsedMinutes);
                
                // Оставшееся время
                var remainingMinutes = totalTimeMinutes * (1 - progress / 100.0);
                if (remainingMinutes < 0.1) remainingMinutes = 0.1;
                var newEstimatedEndDate = newStartDate.AddMinutes(totalTimeMinutes);
                
                // Обновляем заказ
                order.Progress = progress;
                order.StartDate = newStartDate;
                order.EstimatedEndDate = newEstimatedEndDate;
                
                await _context.SaveChangesAsync();
                Console.WriteLine($"Заказ #{id}: {oldProgress}% -> {progress}%, StartDate={newStartDate:HH:mm:ss}, EstimatedEnd={newEstimatedEndDate:HH:mm:ss}");
                
                if (progress >= 100)
                {
                    order.Status = "Completed";
                    if (order.ProductionLineId.HasValue)
                    {
                        var line2 = await _context.ProductionLines.FindAsync(order.ProductionLineId);
                        if (line2 != null) line2.CurrentWorkOrderId = null;
                    }
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Заказ #{id} завершен!");
                }
                
                return Json(new { 
                    success = true, 
                    progress = progress,
                    estimatedEndDate = order.EstimatedEndDate.ToString("dd.MM.yyyy HH:mm")
                });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            var order = await _context.WorkOrders
                .Include(o => o.ProductionLine)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order != null && order.Status == "InProgress")
            {
                order.Status = "Completed";
                order.Progress = 100;
                
                if (order.ProductionLine != null)
                {
                    order.ProductionLine.CurrentWorkOrderId = null;
                }
                
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Заказ #{order.Id} завершен!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.WorkOrders
                .Include(o => o.ProductionLine)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order != null && order.Status != "Completed")
            {
                var productMaterials = await _context.ProductMaterials
                    .Include(pm => pm.Material)
                    .Where(pm => pm.ProductId == order.ProductId)
                    .ToListAsync();
                
                foreach (var pm in productMaterials)
                {
                    if (pm.Material != null)
                    {
                        pm.Material.Quantity += pm.QuantityNeeded * order.Quantity;
                    }
                }
                
                order.Status = "Cancelled";
                order.Progress = 0;
                
                if (order.ProductionLine != null)
                {
                    order.ProductionLine.CurrentWorkOrderId = null;
                }
                
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Заказ #{order.Id} отменен";
            }
            return RedirectToAction(nameof(Index));
        }
        
        [HttpGet]
        public async Task<IActionResult> GetProgress(int id)
        {
            var order = await _context.WorkOrders.FindAsync(id);
            if (order != null)
            {
                // Пересчитываем прогресс на основе времени
                if (order.Status == "InProgress")
                {
                    var now = DateTime.Now;
                    var totalMinutes = (order.EstimatedEndDate - order.StartDate).TotalMinutes;
                    if (totalMinutes > 0)
                    {
                        var elapsedMinutes = (now - order.StartDate).TotalMinutes;
                        var currentProgress = (int)((elapsedMinutes / totalMinutes) * 100);
                        if (currentProgress > 100) currentProgress = 100;
                        if (currentProgress < 0) currentProgress = 0;
                        
                        if (order.Progress != currentProgress)
                        {
                            order.Progress = currentProgress;
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                return Json(new { 
                    progress = order.Progress, 
                    status = order.Status,
                    estimatedEndDate = order.EstimatedEndDate.ToString("dd.MM.yyyy HH:mm")
                });
            }
            return Json(new { progress = 0, status = "NotFound" });
        }
    }
}
