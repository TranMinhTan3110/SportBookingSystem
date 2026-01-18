using Microsoft.AspNetCore.Mvc;

namespace SportBookingSystem.Controllers
{
    public class UserProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
