using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductionLine> ProductionLines { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<ProductMaterial> ProductMaterials { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductMaterial>()
                .HasKey(pm => new { pm.ProductId, pm.MaterialId });

            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Product)
                .WithMany(p => p.ProductMaterials)
                .HasForeignKey(pm => pm.ProductId);

            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Material)
                .WithMany(m => m.ProductMaterials)
                .HasForeignKey(pm => pm.MaterialId);

            modelBuilder.Entity<WorkOrder>()
                .HasOne(wo => wo.Product)
                .WithMany(p => p.WorkOrders)
                .HasForeignKey(wo => wo.ProductId);

            modelBuilder.Entity<WorkOrder>()
                .HasOne(wo => wo.ProductionLine)
                .WithMany(pl => pl.WorkOrders)
                .HasForeignKey(wo => wo.ProductionLineId);

            modelBuilder.Entity<ProductionLine>()
                .HasOne(pl => pl.CurrentWorkOrder)
                .WithMany()
                .HasForeignKey(pl => pl.CurrentWorkOrderId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Material>()
                .Property(m => m.Quantity)
                .HasColumnType("REAL");

            modelBuilder.Entity<Material>()
                .Property(m => m.MinimalStock)
                .HasColumnType("REAL");

            modelBuilder.Entity<ProductMaterial>()
                .Property(pm => pm.QuantityNeeded)
                .HasColumnType("REAL");

            modelBuilder.Entity<ProductionLine>().HasData(
                new ProductionLine { Id = 1, Name = "Линия А", Status = "Active", EfficiencyFactor = 1.0f },
                new ProductionLine { Id = 2, Name = "Линия Б", Status = "Active", EfficiencyFactor = 1.2f },
                new ProductionLine { Id = 3, Name = "Линия В", Status = "Stopped", EfficiencyFactor = 0.8f }
            );

            modelBuilder.Entity<Material>().HasData(
                new Material { Id = 1, Name = "Сталь", Quantity = 500, UnitOfMeasure = "кг", MinimalStock = 100 },
                new Material { Id = 2, Name = "Пластик", Quantity = 300, UnitOfMeasure = "кг", MinimalStock = 80 },
                new Material { Id = 3, Name = "Электроника", Quantity = 50, UnitOfMeasure = "шт", MinimalStock = 20 }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Смартфон", Description = "Современный смартфон", Category = "Электроника", MinimalStock = 10, ProductionTimePerUnit = 5 },
                new Product { Id = 2, Name = "Ноутбук", Description = "Мощный ноутбук", Category = "Электроника", MinimalStock = 5, ProductionTimePerUnit = 15 },
                new Product { Id = 3, Name = "Стул", Description = "Деревянный стул", Category = "Мебель", MinimalStock = 20, ProductionTimePerUnit = 8 }
            );

            modelBuilder.Entity<ProductMaterial>().HasData(
                new ProductMaterial { ProductId = 1, MaterialId = 1, QuantityNeeded = 0.5m },
                new ProductMaterial { ProductId = 1, MaterialId = 2, QuantityNeeded = 0.3m },
                new ProductMaterial { ProductId = 2, MaterialId = 1, QuantityNeeded = 1.5m },
                new ProductMaterial { ProductId = 2, MaterialId = 3, QuantityNeeded = 1.0m },
                new ProductMaterial { ProductId = 3, MaterialId = 2, QuantityNeeded = 2.0m }
            );
        }
    }
}
