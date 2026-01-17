using Microsoft.AspNetCore.Mvc;

namespace SportBookingSystem.Controllers
{
    public class AdminProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
