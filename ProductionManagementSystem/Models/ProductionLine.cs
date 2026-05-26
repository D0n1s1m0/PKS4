using System.ComponentModel.DataAnnotations;

namespace ProductionManagementSystem.Models
{
    public class ProductionLine
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название линии")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Статус")]
        public string Status { get; set; } = "Stopped";

        [Range(0.5, 2.0)]
        [Display(Name = "Коэффициент эффективности")]
        public float EfficiencyFactor { get; set; } = 1.0f;

        [Display(Name = "Текущий заказ")]
        public int? CurrentWorkOrderId { get; set; }

        public WorkOrder? CurrentWorkOrder { get; set; }
        public ICollection<WorkOrder>? WorkOrders { get; set; }
    }
}
