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
            return await _context.Bookings
                .Include(b => b.Pitch)
                .Where(b => b.UserId == userId && b.StartTime > DateTime.Now && b.Status != 2)
                .OrderBy(b => b.StartTime)
                .FirstOrDefaultAsync();
        }
    }
}
