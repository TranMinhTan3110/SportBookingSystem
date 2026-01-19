using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SportBookingSystem.Controllers
{
    [Authorize]
    public class UserADController : Controller  
    {
        [HttpGet]
        [Route("UserAD/Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}