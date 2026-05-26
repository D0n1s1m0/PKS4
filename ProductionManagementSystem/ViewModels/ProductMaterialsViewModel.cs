using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.ViewModels
{
    public class ProductMaterialsViewModel
    {
        public Product Product { get; set; } = new Product();
        public List<ProductMaterial> Materials { get; set; } = new List<ProductMaterial>();
        public List<Material> AvailableMaterials { get; set; } = new List<Material>();
        public List<string> Categories { get; set; } = new List<string>();
        public string NewCategory { get; set; } = string.Empty;
    }
}
