using Microsoft.AspNetCore.Mvc;

namespace SportBookingSystem.Controllers
{
    public class AdminLayoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
