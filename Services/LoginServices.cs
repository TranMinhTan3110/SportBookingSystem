using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;
using BC = BCrypt.Net.BCrypt;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace SportBookingSystem.Services
{
    public class LoginServices : ILoginServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache; 

        public LoginServices(ApplicationDbContext context, IConfiguration configuration, IMemoryCache cache)
        {
            _context = context;
            _configuration = configuration;
            _cache = cache;
        }
        public async Task<Users> CheckLoginAsync(string phone, string password)
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
            if (user == null) return false;

            var mailSettings = _configuration.GetSection("MailSettings");

            string otp = new Random().Next(100000, 999999).ToString();

            _cache.Set("OTP_" + email, otp, TimeSpan.FromMinutes(5));

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(mailSettings["DisplayName"], mailSettings["Mail"]));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = "Mã xác thực đặt lại mật khẩu";
            emailMessage.Body = new TextPart("html")
            {
                Text = $@"<div style='font-family: Arial; padding: 20px; border: 1px solid #ddd;'>
                        <h2>Xác thực đặt lại mật khẩu</h2>
                        <p>Mã OTP của bạn là: <b style='font-size: 24px; color: #28a745;'>{otp}</b></p>
                        <p>Mã này có hiệu lực trong 5 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                      </div>"
            };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(mailSettings["Host"], int.Parse(mailSettings["Port"]), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(mailSettings["Mail"], mailSettings["Password"]);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi gửi mail: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> VerifyOtpAndResetPasswordAsync(string email, string otp, string newPassword)
        {
            if (!_cache.TryGetValue("OTP_" + email, out string savedOtp))
            {
                return false; // OTP đã hết hạn
            }

            if (savedOtp != otp)
            {
                return false; // Sai mã OTP
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