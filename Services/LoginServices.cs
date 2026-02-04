using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;
using BC = BCrypt.Net.BCrypt;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SportBookingSystem.Helper;

namespace SportBookingSystem.Services
{
    public class LoginServices : ILoginServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly MailSettings _mailSettings;

        public LoginServices(ApplicationDbContext context, IMemoryCache cache, IOptions<MailSettings> mailSettings)
        {
            _context = context;
            _cache = cache;
            _mailSettings = mailSettings.Value;
        }

        // Khớp với Interface: Trả về Users?
        public async Task<Users?> CheckLoginAsync(string phone, string password)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Phone == phone);

            if (user != null && BC.Verify(password, user.Password))
            {
                return user;
            }
            return null;
        }

        public async Task<bool> IsPhoneExistsAsync(string phone)
            => await _context.Users.AnyAsync(u => u.Phone == phone);

        public async Task<bool> IsEmailExistsAsync(string email)
            => await _context.Users.AnyAsync(u => u.Email == email);

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
                    Password = BC.HashPassword(model.Password),
                    RoleId = 2,
                    IsActive = true
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> SendOtpEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            string otp = new Random().Next(100000, 999999).ToString();
            _cache.Set("OTP_" + email, otp, TimeSpan.FromMinutes(5));

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = "Mã xác thực đặt lại mật khẩu";
            emailMessage.Body = new TextPart("html") { Text = $"Mã OTP của bạn là: <b>{otp}</b>" };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi SMTP: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> VerifyOtpAndResetPasswordAsync(string email, string otp, string newPassword)
        {
            if (!_cache.TryGetValue("OTP_" + email, out string? savedOtp) || savedOtp != otp)
                return false;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            user.Password = BC.HashPassword(newPassword);
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