using Microsoft.AspNetCore.Mvc;

namespace SportBookingSystem.Controllers
{
    public class UserADController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
