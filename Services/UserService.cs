
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SportBookingSystem.DTO;
using SportBookingSystem.Models.EF;

namespace SportBookingSystem.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context) {
            _context = context;
        }
        public async Task<UserStatisticsDto> countUserAsync()
        {
            var now = DateTime.Now;
            var data = new UserStatisticsDto

            {
                TotalUsers = await _context.Users.CountAsync(),
                LockedUsers = await _context.Users.Where(w => w.IsActive == false).CountAsync(),
                ActiveUsers = await _context.Users.Where(w => w.IsActive == true).CountAsync(),
                NewUsers = await _context.Users.CountAsync(w =>
            w.CreatedAt.Month == now.Month && w.CreatedAt.Year == now.Year)
            };
            return data;
        }

        public async Task<List<UserInfo>> getAllUserAsync()
        {
            var rawData = await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var datas = rawData.Select(u => new UserInfo
            {
                Id = u.UserId.ToString(),
                FullName = u.FullName,
                PhoneNumber = u.Phone,
                Email = u.Email,
                Role = u.Role != null ? u.Role.RoleName : "Chưa có quyền",
                CreatedAt = u.CreatedAt.ToString("dd/MM/yyyy"),
                IsActive = u.IsActive,
            }).ToList();

            return datas;
        }
        //hàm khóa mở một user
        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return false;
            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
