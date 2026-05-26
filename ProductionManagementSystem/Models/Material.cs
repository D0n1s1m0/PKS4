using System.ComponentModel.DataAnnotations;

namespace ProductionManagementSystem.Models
{
    public class Material
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Наименование")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Количество")]
        public decimal Quantity { get; set; }

        [Display(Name = "Единица измерения")]
        public string UnitOfMeasure { get; set; } = "шт";

        [Display(Name = "Минимальный запас")]
        public decimal MinimalStock { get; set; }

        public ICollection<ProductMaterial>? ProductMaterials { get; set; }
    }
}
