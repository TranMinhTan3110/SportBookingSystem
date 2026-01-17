using Microsoft.AspNetCore.Mvc;

namespace SportBookingSystem.Controllers
{
    public class BookingADController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
