using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.ViewModels;
using SportBookingSystem.Services;

namespace SportBookingSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;

        public BookingController(IBookingService bookingService, ApplicationDbContext context)
        {
            _bookingService = bookingService;
            _context = context;
        }

        // GET: /Booking/Index
        public async Task<IActionResult> Index()
        {
            // Truyền Categories cho bộ lọc
            ViewBag.Categories = await _context.Categories
                .Where(c => c.Type == "Pitch")
                .Select(c => new { c.CategoryId, c.CategoryName })
                .ToListAsync();

            // Truyền TimeSlots cho bộ lọc
            ViewBag.TimeSlots = await _context.TimeSlots
                .Where(ts => ts.IsActive)
                .OrderBy(ts => ts.StartTime)
                .Select(ts => new
                {
                    ts.SlotId,
                    ts.SlotName,
                    ts.StartTime,
                    ts.EndTime,
                    TimeRange = ts.StartTime.ToString(@"hh\:mm") + " - " + ts.EndTime.ToString(@"hh\:mm")
                })
                .ToListAsync();

            return View();
        }

        // POST: /Booking/GetFilteredPitches
        [HttpPost]
        public async Task<IActionResult> GetFilteredPitches([FromBody] FilterPitchesRequest request)
        {
            try
            {
                var result = await _bookingService.GetFilteredPitchesAsync(
                    request.Date,
                    request.SlotId,
                    request.CategoryIds,
                    request.MinPrice,
                    request.MaxPrice,
                    request.StatusFilter
                );

                return Json(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }

        // GET: /Booking/GetFilteredPitches (hỗ trợ GET request)
        [HttpGet]
        public async Task<IActionResult> GetFilteredPitches(
            DateTime? date,
            int? slotId,
            string? categoryIds,
            decimal? minPrice,
            decimal? maxPrice)
        {
            try
            {
                var filterDate = date ?? DateTime.Today;
                List<int>? categoryIdList = null;

                if (!string.IsNullOrEmpty(categoryIds))
                {
                    categoryIdList = categoryIds.Split(',')
                        .Select(id => int.TryParse(id, out var result) ? result : 0)
                        .Where(id => id > 0)
                        .ToList();
                }

                var result = await _bookingService.GetAvailablePitchesAsync(
                    filterDate,
                    slotId,
                    categoryIdList,
                    minPrice,
                    maxPrice
                );

                return Json(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }
    }
}