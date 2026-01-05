using Microsoft.AspNetCore.Mvc;

namespace SportBookingSystem.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
