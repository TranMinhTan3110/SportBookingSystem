using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Constants;
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

            var upcomingBookingsCount = await _context.PitchSlots
                .Where(ps => ps.UserId == userId && ps.Status != -1 && ps.Status != -2)
                .CountAsync();

            var pendingBookingsCount = await _context.PitchSlots
                .Where(ps => ps.UserId == userId && ps.Status == 1)
                .CountAsync();

            var recentTransactions = await _context.Transactions
           .Where(t => t.UserId == userId
                    && t.TransactionType != "Chuyển tiền"
                    && t.TransactionType != "Nhận tiền")  
           .OrderByDescending(t => t.TransactionDate)
           .Take(5)
           .ToListAsync();

            // Top 3 Sân bóng được đặt thành công nhiều nhất
            var topPitchGroups = await _context.PitchSlots
                .Where(ps => ps.Status == BookingStatus.CheckedIn || ps.Status == BookingStatus.Completed)
                .GroupBy(ps => ps.PitchId)
                .OrderByDescending(g => g.Count())
                .Select(g => new { PitchId = g.Key, Count = g.Count() })
                .Take(3)
                .ToListAsync();

            var topPitchIds = topPitchGroups.Select(x => x.PitchId).ToList();

            var pitchesFromHistory = await _context.Pitches
                .Include(p => p.Category)
                .Where(p => topPitchIds.Contains(p.PitchId))
                .ToListAsync();

            // Sắp xếp lại theo đúng thứ tự phổ biến
            var featuredPitches = topPitchIds
                .Select(id => pitchesFromHistory.First(p => p.PitchId == id))
                .ToList();

            // Nếu không đủ 3 sân từ lịch sử, lấy thêm các sân mới nhất để đủ 3
            if (featuredPitches.Count < 3)
            {
                var extraPitches = await _context.Pitches
                    .Include(p => p.Category)
                    .Where(p => !topPitchIds.Contains(p.PitchId))
                    .OrderByDescending(p => p.PitchId)
                    .Take(3 - featuredPitches.Count)
                    .ToListAsync();
                featuredPitches.AddRange(extraPitches);
            }

            // Top 3 Đồ ăn & Thức uống (Category Type: Product) bán chạy nhất
            var topFoodIds = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .ThenInclude(p => p.Category)
                .Where(od => od.Order.Status == 1 && od.Product.Category.Type == "Product")
                .GroupBy(od => od.ProductId)
                .OrderByDescending(g => g.Sum(x => x.Quantity))
                .Select(g => g.Key)
                .Take(3)
                .ToListAsync();

            var featuredFood = await _context.Products
                .Include(p => p.Category)
                .Where(p => topFoodIds.Contains(p.ProductId) && p.Status == true)
                .ToListAsync();

            if (featuredFood.Count < 3)
            {
                var extraFood = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => !topFoodIds.Contains(p.ProductId) && p.Status == true && p.Category.Type == "Product")
                    .OrderByDescending(p => p.ProductId)
                    .Take(3 - featuredFood.Count)
                    .ToListAsync();
                featuredFood.AddRange(extraFood);
            }

            // Top 3 Đồ dùng thể thao bán chạy nhất
            var topSupplyIds = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .ThenInclude(p => p.Category)
                .Where(od => od.Order.Status == 1 && od.Product.Category.Type == "Service")
                .GroupBy(od => od.ProductId)
                .OrderByDescending(g => g.Sum(x => x.Quantity))
                .Select(g => g.Key)
                .Take(3)
                .ToListAsync();

            var featuredSupplies = await _context.Products
                .Include(p => p.Category)
                .Where(p => topSupplyIds.Contains(p.ProductId) && p.Status == true)
                .ToListAsync();

            if (featuredSupplies.Count < 3)
            {
                var extraSupplies = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => !topSupplyIds.Contains(p.ProductId) && p.Status == true && p.Category.Type == "Service")
                    .OrderByDescending(p => p.ProductId)
                    .Take(3 - featuredSupplies.Count)
                    .ToListAsync();
                featuredSupplies.AddRange(extraSupplies);
            }

            var viewModel = new HomeViewModel
            {
                User = user,
                UpcomingBookingsCount = upcomingBookingsCount,
                PendingBookingsCount = pendingBookingsCount,
                RecentTransactions = recentTransactions,
                FeaturedPitches = featuredPitches,
                FeaturedFoodItems = featuredFood,
                FeaturedSupplies = featuredSupplies
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
