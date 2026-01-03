using Microsoft.AspNetCore.Mvc;

namespace SportBookingSystem.Controllers
{
    public class FoodController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
