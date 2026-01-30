using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using BC = BCrypt.Net.BCrypt;

namespace SportBookingSystem.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;

        public ProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Users?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<Users?> GetAdminByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<(bool Success, string Message)> ChangeEmailAsync(int userId, string newEmail)
        {
            if (string.IsNullOrEmpty(newEmail))
                return (false, "Vui lòng nhập email mới.");

            if (await _context.Users.AnyAsync(u => u.Email == newEmail && u.UserId != userId))
                return (false, "Email này đã được sử dụng bởi tài khoản khác.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return (false, "Người dùng không tồn tại.");

            user.Email = newEmail;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return (true, "Đổi email thành công!");
        }

        public async Task<(bool Success, string Message)> ChangePhoneAsync(int userId, string newPhone)
        {
            if (string.IsNullOrEmpty(newPhone))
                return (false, "Vui lòng nhập số điện thoại mới.");

            if (await _context.Users.AnyAsync(u => u.Phone == newPhone && u.UserId != userId))
                return (false, "Số điện thoại này đã được sử dụng bởi tài khoản khác.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return (false, "Người dùng không tồn tại.");

            user.Phone = newPhone;
            user.Username = newPhone;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return (true, "Đổi số điện thoại thành công!");
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
                return (false, "Vui lòng nhập đầy đủ thông tin.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return (false, "Người dùng không tồn tại.");

            if (!BC.Verify(currentPassword, user.Password))
                return (false, "Mật khẩu hiện tại không đúng.");

            user.Password = BC.HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return (true, "Đổi mật khẩu thành công!");
        }

        public async Task<Bookings?> GetUpcomingBookingAsync(int userId)
        {
            var threshold = DateTime.Now.AddMinutes(-30);
            var thresholdDate = threshold.Date;
            var thresholdTime = threshold.TimeOfDay;

            var slot = await _context.PitchSlots
                .Include(ps => ps.Pitch)
                .Include(ps => ps.TimeSlot)
                .Where(ps => ps.UserId == userId 
                         && ps.Status == 1 // Chỉ lấy đơn Chờ xác nhận (PendingConfirm)
                         && (ps.PlayDate > thresholdDate || (ps.PlayDate == thresholdDate && ps.TimeSlot.StartTime > thresholdTime)))
                .OrderBy(ps => ps.PlayDate)
                .ThenBy(ps => ps.TimeSlot.StartTime)
                .FirstOrDefaultAsync();

            if (slot == null) return null;

            // Ánh xạ sang Bookings để tương thích với View
            return new Bookings
            {
                Pitch = slot.Pitch,
                StartTime = slot.PlayDate.Date.Add(slot.TimeSlot.StartTime),
                EndTime = slot.PlayDate.Date.Add(slot.TimeSlot.EndTime),
                CheckInCode = slot.BookingCode,
                Status = slot.Status,
                PitchId = slot.PitchId,
                UserId = slot.UserId
            };
        }

        public async Task<Orders?> GetLatestPendingOrderAsync(int userId)
        {

            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.UserId == userId && o.Status == 0) 
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Orders>> GetPendingOrdersAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.UserId == userId && o.Status == 0)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Orders?> GetOrderByIdAsync(int orderId, int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);
        }
        // ...
        public async Task<List<Bookings>> GetActiveBookingsAsync(int userId)
        {
            var slots = await _context.PitchSlots
                .Include(ps => ps.Pitch)
                .Include(ps => ps.TimeSlot)
                .Where(ps => ps.UserId == userId
                          && (ps.Status == 0 || ps.Status == 1) // 0: Chờ nhận, 1: Đã nhận
                          && ps.PlayDate >= DateTime.Today)
                .OrderBy(ps => ps.PlayDate)
                .ThenBy(ps => ps.TimeSlot.StartTime)
                .ToListAsync();

            return slots.Select(slot => new Bookings
            {
                Pitch = slot.Pitch,
                TimeSlot = slot.TimeSlot,
                StartTime = slot.PlayDate.Date.Add(slot.TimeSlot.StartTime),
                EndTime = slot.PlayDate.Date.Add(slot.TimeSlot.EndTime),
                CheckInCode = slot.BookingCode,
                Status = slot.Status,
                PitchId = slot.PitchId,
                UserId = slot.UserId
            }).ToList();
        }
     

    }
}
