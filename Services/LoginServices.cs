using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;
using BC = BCrypt.Net.BCrypt;

namespace SportBookingSystem.Services
{
    public class LoginServices : ILoginServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService; 

        public LoginServices(ApplicationDbContext context, IMemoryCache cache, IEmailService emailService)
        {
            _context = context;
            _cache = cache;
            _emailService = emailService;
        }

        public async Task<Users> CheckLoginAsync(string phone, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Phone == phone);

            if (user != null && BC.Verify(password, user.Password))
            {
                return user;
            }
            return null;
        }

        public async Task<bool> IsPhoneExistsAsync(string phone)
        {
            return await _context.Users.AnyAsync(u => u.Phone == phone);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> RegisterUserAsync(SignUpViewModel model)
        {
            try
            {
                var newUser = new Users
                {
                    FullName = model.FullName,
                    Username = model.Phone,
                    Phone = model.Phone,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    RoleId = 2,
                    IsActive = true
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                System.Diagnostics.Debug.WriteLine("LỖI ĐĂNG KÝ: " + msg);
                return false;
            }
        }

        public async Task<bool> SendOtpEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                Console.WriteLine($"User không tồn tại với email: {email}");
                return false;
            }

            string otp = new Random().Next(100000, 999999).ToString();
            _cache.Set("OTP_" + email, otp, TimeSpan.FromMinutes(5));

            Console.WriteLine($"OTP được tạo: {otp} cho email: {email}");

            return await _emailService.SendOtpEmailAsync(email, otp);
        }

        public async Task<bool> VerifyOtpAndResetPasswordAsync(string email, string otp, string newPassword)
        {
            if (!_cache.TryGetValue("OTP_" + email, out string savedOtp))
            {
                return false;
            }

            if (savedOtp != otp)
            {
                return false;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _cache.Remove("OTP_" + email);

            return true;
        }

        public async Task<Users?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }
}