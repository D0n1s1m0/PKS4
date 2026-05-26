using System.ComponentModel.DataAnnotations;

namespace TourGuide.Models
{
    public class Attraction
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название достопримечательности обязательно")]
        [Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "История")]
        public string History { get; set; } = string.Empty;

        [Display(Name = "Фотография")]
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Часы работы")]
        public string WorkingHours { get; set; } = string.Empty;

        [Display(Name = "Стоимость посещения")]
        public string Price { get; set; } = string.Empty;

        [Display(Name = "Краткое описание")]
        public string ShortDescription { get; set; } = string.Empty;

        // Внешний ключ
        public int CityId { get; set; }
        
        // Навигационное свойство
        public City? City { get; set; }
    }
}
