using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProductionManagementSystem.ViewModels
{
    public class CreateOrderViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int? ProductionLineId { get; set; }
        public int ProductionTimePerUnit { get; set; }
        public float EfficiencyFactor { get; set; } = 1.0f;
        public int CalculatedTimeMinutes { get; set; }
        public DateTime EstimatedEndDate { get; set; }

        public SelectList? Products { get; set; }
        public SelectList? ProductionLines { get; set; }
    }
}
