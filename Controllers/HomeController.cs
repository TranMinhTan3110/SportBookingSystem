using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;
using SportBookingSystem.Services;

namespace SportBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ILoginServices _loginServices;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, ILoginServices loginServices)
        {
            _logger = logger;
            _context = context;
            _loginServices = loginServices;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("SignIn", "Login");
            }

            var user = await _loginServices.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("SignIn", "Login");
            }

            var upcomingBookingsCount = await _context.Bookings
                .Where(b => b.UserId == userId && b.StartTime > DateTime.Now && b.Status != 2)
                .CountAsync();

            var recentTransactions = await _context.Transactions
                .Where(t => t.UserId == userId || t.ReceiverId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .Take(5)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                User = user,
                UpcomingBookingsCount = upcomingBookingsCount,
                RecentTransactions = recentTransactions
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
