using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Services;

namespace SportBookingSystem.Controllers
{
    public class ManageBookingADController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;

        public ManageBookingADController(IBookingService bookingService, ApplicationDbContext context)
        {
            _bookingService = bookingService;
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories.Where(c => c.Type == "Pitch").ToList();
            ViewBag.Categories = categories;
            ViewBag.TimeSlots = _context.TimeSlots.Where(ts => ts.IsActive).OrderBy(ts => ts.StartTime).ToList();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings(DateTime? fromDate, DateTime? toDate, int? timeSlotId, int? categoryId, string? search)
        {
            var data = await _bookingService.GetPendingBookingsAsync(fromDate, toDate, timeSlotId, categoryId, search);
            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveBooking(int id)
        {
            var result = await _bookingService.ApproveBookingAsync(id);
            if (result.Success) return Ok(new { success = true, message = result.Message });
            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> RejectBooking(int id)
        {
            var result = await _bookingService.RejectBookingAsync(id);
            if (result.Success) return Ok(new { success = true, message = result.Message });
            return BadRequest(new { success = false, message = result.Message });
        }
    }
}
