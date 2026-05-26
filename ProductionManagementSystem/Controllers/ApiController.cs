using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Models;
using ProductionManagementSystem.Data;

namespace ProductionManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/materials?low_stock=true
        [HttpGet("materials")]
        public async Task<IActionResult> GetMaterials(bool low_stock = false)
        {
            var materials = _context.Materials.AsQueryable();
            if (low_stock)
            {
                materials = materials.Where(m => m.Quantity < m.MinimalStock);
            }
            return Ok(await materials.ToListAsync());
        }

        // POST: api/materials
        [HttpPost("materials")]
        public async Task<IActionResult> AddMaterial([FromBody] Material material)
        {
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();
            return Ok(material);
        }

        // PUT: api/materials/{id}/stock
        [HttpPut("materials/{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] decimal amount)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return NotFound();
            material.Quantity += amount;
            await _context.SaveChangesAsync();
            return Ok(material);
        }

        // GET: api/products
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts(string? category)
        {
            var products = _context.Products.AsQueryable();
            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category == category);
            }
            return Ok(await products.ToListAsync());
        }

        // GET: api/products/{id}/materials
        [HttpGet("products/{id}/materials")]
        public async Task<IActionResult> GetProductMaterials(int id)
        {
            var materials = await _context.ProductMaterials
                .Include(pm => pm.Material)
                .Where(pm => pm.ProductId == id)
                .Select(pm => new { pm.MaterialId, pm.Material!.Name, pm.QuantityNeeded, pm.Material.Quantity })
                .ToListAsync();
            return Ok(materials);
        }

        // POST: api/products
        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return Ok(product);
        }

        // GET: api/lines
        [HttpGet("lines")]
        public async Task<IActionResult> GetLines(bool available = false)
        {
            var lines = _context.ProductionLines.AsQueryable();
            if (available)
            {
                lines = lines.Where(l => l.Status == "Active");
            }
            return Ok(await lines.ToListAsync());
        }

        // PUT: api/lines/{id}/status
        [HttpPut("lines/{id}/status")]
        public async Task<IActionResult> UpdateLineStatus(int id, [FromBody] string status)
        {
            var line = await _context.ProductionLines.FindAsync(id);
            if (line == null) return NotFound();
            line.Status = status;
            await _context.SaveChangesAsync();
            return Ok(line);
        }

        // GET: api/orders
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders(string? status, string? date)
        {
            var orders = _context.WorkOrders.Include(o => o.Product).AsQueryable();
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }
            if (date == "today")
            {
                var today = DateTime.Today;
                orders = orders.Where(o => o.StartDate.Date == today);
            }
            return Ok(await orders.ToListAsync());
        }

        // POST: api/orders
        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder([FromBody] WorkOrder order)
        {
            var product = await _context.Products.FindAsync(order.ProductId);
            if (product == null) return BadRequest("Product not found");

            var line = order.ProductionLineId.HasValue ? await _context.ProductionLines.FindAsync(order.ProductionLineId) : null;
            float efficiency = line?.EfficiencyFactor ?? 1.0f;
            int timeMinutes = (int)(order.Quantity * product.ProductionTimePerUnit / efficiency);
            order.EstimatedEndDate = DateTime.Now.AddMinutes(timeMinutes);
            order.StartDate = DateTime.Now;
            order.Status = "Pending";

            _context.WorkOrders.Add(order);
            await _context.SaveChangesAsync();
            return Ok(order);
        }

        // POST: api/calculate/production
        [HttpPost("calculate/production")]
        public async Task<IActionResult> CalculateProduction(int product_id, int quantity)
        {
            var product = await _context.Products.FindAsync(product_id);
            if (product == null) return BadRequest("Product not found");
            int time = quantity * product.ProductionTimePerUnit;
            return Ok(new { product_id, quantity, time_minutes = time });
        }
    }
}
