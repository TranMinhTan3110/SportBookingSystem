using Microsoft.AspNetCore.Mvc;

namespace SportBookingSystem.Controllers
{
    public class SportProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
