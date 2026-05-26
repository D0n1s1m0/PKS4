using System.ComponentModel.DataAnnotations;

namespace ProductionManagementSystem.Models
{
    public class WorkOrder
    {
        public int Id { get; set; }

        [Display(Name = "Продукт")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Display(Name = "Производственная линия")]
        public int? ProductionLineId { get; set; }
        public ProductionLine? ProductionLine { get; set; }

        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        [Display(Name = "Дата начала")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Расчетная дата завершения")]
        public DateTime EstimatedEndDate { get; set; }

        [Display(Name = "Статус")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Прогресс выполнения (%)")]
        public int Progress { get; set; } = 0;
        
        [Display(Name = "Время последнего ручного изменения")]
        public DateTime? LastManualUpdate { get; set; }
    }
}
