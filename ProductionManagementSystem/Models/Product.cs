using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductionManagementSystem.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование продукта обязательно")]
        [Display(Name = "Наименование")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Технические характеристики")]
        public string Specifications { get; set; } = "{}";

        [Display(Name = "Категория")]
        public string Category { get; set; } = string.Empty;

        [Display(Name = "Минимальный запас")]
        public int MinimalStock { get; set; }

        [Display(Name = "Время производства (мин/шт)")]
        public int ProductionTimePerUnit { get; set; }

        public ICollection<ProductMaterial>? ProductMaterials { get; set; }
        public ICollection<WorkOrder>? WorkOrders { get; set; }
    }
}
