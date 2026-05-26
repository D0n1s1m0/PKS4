using Microsoft.EntityFrameworkCore;
using TourGuide.Models;

namespace TourGuide.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<Attraction> Attractions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Связь City - Attraction
            modelBuilder.Entity<Attraction>()
                .HasOne(a => a.City)
                .WithMany(c => c.Attractions)
                .HasForeignKey(a => a.CityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data - Города
            modelBuilder.Entity<City>().HasData(
                new City 
                { 
                    Id = 1, 
                    Name = "Москва", 
                    Region = "Центральный федеральный округ", 
                    Population = 12500000,
                    History = "Москва - столица России, один из крупнейших городов мира. Впервые упоминается в 1147 году. Основана князем Юрием Долгоруким.",
                    CoatOfArms = "На гербе изображен Георгий Победоносец, поражающий змея.",
                    ImageUrl = "/images/cities/moscow.jpg"
                },
                new City 
                { 
                    Id = 2, 
                    Name = "Санкт-Петербург", 
                    Region = "Северо-Западный федеральный округ", 
                    Population = 5400000,
                    History = "Санкт-Петербург основан в 1703 году императором Петром I. Был столицей Российской империи более 200 лет.",
                    CoatOfArms = "На гербе изображены два якоря - морской и речной, и скипетр.",
                    ImageUrl = "/images/cities/spb.jpg"
                },
                new City 
                { 
                    Id = 3, 
                    Name = "Казань", 
                    Region = "Республика Татарстан", 
                    Population = 1257000,
                    History = "Казань - один из древнейших городов России, основан в 1005 году. Столица Татарстана.",
                    CoatOfArms = "На гербе изображен дракон Зилант.",
                    ImageUrl = "/images/cities/kazan.jpg"
                },
                new City 
                { 
                    Id = 4, 
                    Name = "Новосибирск", 
                    Region = "Сибирский федеральный округ", 
                    Population = 1620000,
                    History = "Новосибирск основан в 1893 году как поселок строителей Транссибирской магистрали.",
                    CoatOfArms = "На гербе изображены соболь и река Обь.",
                    ImageUrl = "/images/cities/novosibirsk.jpg"
                }
            );

            // Seed data - Достопримечательности
            modelBuilder.Entity<Attraction>().HasData(
                // Москва
                new Attraction 
                { 
                    Id = 1, 
                    Name = "Красная площадь", 
                    History = "Главная площадь Москвы, образовалась в XV веке.",
                    ImageUrl = "/images/attractions/red_square.jpg",
                    WorkingHours = "Круглосуточно",
                    Price = "Бесплатно",
                    ShortDescription = "Главная площадь страны, сердце Москвы",
                    CityId = 1
                },
                new Attraction 
                { 
                    Id = 2, 
                    Name = "Московский Кремль", 
                    History = "Древнейшая часть Москвы, резиденция президента.",
                    ImageUrl = "/images/attractions/kremlin.jpg",
                    WorkingHours = "10:00 - 17:00 (кроме четверга)",
                    Price = "500-1000 руб.",
                    ShortDescription = "Символ России, древняя крепость",
                    CityId = 1
                },
                // Санкт-Петербург
                new Attraction 
                { 
                    Id = 3, 
                    Name = "Эрмитаж", 
                    History = "Один из крупнейших художественных музеев мира.",
                    ImageUrl = "/images/attractions/hermitage.jpg",
                    WorkingHours = "11:00 - 18:00 (кроме понедельника)",
                    Price = "500 руб.",
                    ShortDescription = "Величайший музей мира",
                    CityId = 2
                },
                new Attraction 
                { 
                    Id = 4, 
                    Name = "Петропавловская крепость", 
                    History = "Крепость основана Петром I в 1703 году.",
                    ImageUrl = "/images/attractions/peterpaul.jpg",
                    WorkingHours = "10:00 - 18:00",
                    Price = "350 руб.",
                    ShortDescription = "Место основания города",
                    CityId = 2
                },
                // Казань
                new Attraction 
                { 
                    Id = 5, 
                    Name = "Казанский Кремль", 
                    History = "Главная достопримечательность Казани, объект ЮНЕСКО.",
                    ImageUrl = "/images/attractions/kazan_kremlin.jpg",
                    WorkingHours = "Круглосуточно",
                    Price = "Бесплатно",
                    ShortDescription = "Белокаменная крепость",
                    CityId = 3
                },
                // Новосибирск
                new Attraction 
                { 
                    Id = 6, 
                    Name = "Новосибирский зоопарк", 
                    History = "Один из крупнейших зоопарков России.",
                    ImageUrl = "/images/attractions/zoo.jpg",
                    WorkingHours = "09:00 - 20:00",
                    Price = "400 руб.",
                    ShortDescription = "Дом для редких животных",
                    CityId = 4
                }
            );
        }
    }
}
