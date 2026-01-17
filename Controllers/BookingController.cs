using Microsoft.AspNetCore.Mvc;

namespace SportBookingSystem.Controllers
{
    public class BookingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
