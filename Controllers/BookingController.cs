using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.ViewModels;
using SportBookingSystem.Services;
using System.Security.Claims;

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

        // GET: Hiển thị trang đặt sân
        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.Type == "Pitch")
                .Select(c => new { c.CategoryId, c.CategoryName })
                .ToListAsync();
            return View();
        }

        // API: Lấy danh sách sân theo bộ lọc (Filter)
        [HttpPost]
        public async Task<IActionResult> GetFilteredPitches([FromBody] FilterPitchesRequest request)
        {
            try
            {
                var result = await _bookingService.GetFilteredPitchesWithPaginationAsync(
                    request.Date,
                    request.SlotIds,
                    request.CategoryIds,
                    request.SpecificPitchIds,
                    request.Capacities,
                    request.StatusFilter,
                    request.Page,
                    request.PageSize
                );
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ============================================
        // API: ĐẶT SÂN (ĐÃ FIX TRẢ VỀ newBalance)
        // ============================================
        [HttpPost]
        public async Task<IActionResult> BookPitch(int pitchId, int slotId, DateTime date)
        {
            try
            {
                // Lấy userId từ Claims (hoặc dùng userId = 1 để test)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                int userId;

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out userId))
                {
                    // OK
                }
                else
                {
                    userId = 1; // Test mode
                }

                // Gọi Service xử lý (Trừ tiền -> Lưu DB -> Tạo QR)
                var result = await _bookingService.BookPitchAsync(userId, pitchId, slotId, date);

                if (result.Success)
                {
                    // ✅ LẤY SỐ DƯ MỚI SAU KHI ĐẶT SÂN
                    var user = await _context.Users.FindAsync(userId);

                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        qrCode = result.QrBase64,
                        bookingCode = result.BookingCode,
                        newBalance = user.WalletBalance  // ← TRẢ VỀ SỐ DƯ MỚI
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Controller: " + ex.Message });
            }
        }
    }
}