using Microsoft.AspNetCore.Mvc;

namespace SportBookingSystem.Controllers
{
    public class HistoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
