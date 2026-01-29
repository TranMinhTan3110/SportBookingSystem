using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Controllers
{
    public class TimeSlotController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TimeSlotController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. API Lấy danh sách khung giờ (Cho Modal)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var slots = await _context.TimeSlots.OrderBy(t => t.StartTime).ToListAsync();
            return Json(new { success = true, data = slots });
        }

        // 2. API Thêm khung giờ
        [HttpPost]
        public async Task<IActionResult> Create(string startTime, string endTime)
        {
            try
            {
                var start = TimeSpan.Parse(startTime);
                var end = TimeSpan.Parse(endTime);

                if (start >= end) return Json(new { success = false, message = "Giờ kết thúc phải lớn hơn bắt đầu!" });

                var newSlot = new TimeSlots
                {
                    SlotName = $"{start:hh\\:mm} - {end:hh\\:mm}",
                    StartTime = start,
                    EndTime = end,
                    IsActive = true
                };

                _context.TimeSlots.Add(newSlot);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Thêm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 3. API Xóa khung giờ
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var slot = await _context.TimeSlots.FindAsync(id);
            if (slot == null) return Json(new { success = false, message = "Không tìm thấy!" });

            _context.TimeSlots.Remove(slot);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa!" });
        }
    }
}