using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Models.ViewModels;    
using SportBookingSystem.Services;

namespace SportBookingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await _dashboardService.GetDashboardDataAsync();
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu dashboard.";
                return View(new DashboardViewModel()); 
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData()
        {
            try
            {
                var chartData = await _dashboardService.GetLast7DaysRevenueAsync();
                return Json(new { success = true, data = chartData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var revenueData = await _dashboardService.GetTodayRevenueAsync();
                var bookingData = await _dashboardService.GetTodayBookingsCountAsync();
                var pitchData = await _dashboardService.GetActivePitchesCountAsync();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        todayRevenue = revenueData.todayRevenue,
                        revenueGrowth = revenueData.growthPercent,
                        isRevenuePositive = revenueData.isPositive,
                        todayBookings = bookingData.todayCount,
                        bookingGrowth = bookingData.growthPercent,
                        isBookingPositive = bookingData.isPositive,
                        activePitches = pitchData.activeCount,
                        totalPitches = pitchData.totalCount
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
    }
}