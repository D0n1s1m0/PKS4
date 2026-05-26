using System.ComponentModel.DataAnnotations;

namespace TourGuide.Models
{
    public class City
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название города обязательно")]
        [Display(Name = "Название города")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Регион")]
        public string Region { get; set; } = string.Empty;

        [Display(Name = "Население")]
        public int Population { get; set; }

        [Display(Name = "История")]
        public string History { get; set; } = string.Empty;

        [Display(Name = "Герб")]
        public string CoatOfArms { get; set; } = string.Empty;

        [Display(Name = "Фотография")]
        public string ImageUrl { get; set; } = string.Empty;

        // Навигационное свойство
        public ICollection<Attraction>? Attractions { get; set; }
    }
}
