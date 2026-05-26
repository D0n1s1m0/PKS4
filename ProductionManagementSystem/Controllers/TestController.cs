using Microsoft.AspNetCore.Mvc;

namespace ProductionManagementSystem.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return Content("Контроллер работает!", "text/plain");
        }
    }
}
