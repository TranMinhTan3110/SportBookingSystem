using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using SportBookingSystem.Helper;

namespace SportBookingSystem.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;

        public SmtpEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otp)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
                emailMessage.To.Add(new MailboxAddress("", toEmail));
                emailMessage.Subject = "Mã xác thực đặt lại mật khẩu";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <div style='font-family: Arial; padding: 20px; border: 1px solid #ddd;'>
                            <h2>Xác thực đặt lại mật khẩu</h2>
                            <p>Mã OTP của bạn là: <b style='font-size: 24px; color: #28a745;'>{otp}</b></p>
                            <p>Mã này có hiệu lực trong 5 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                        </div>"
                };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                // Kết nối tới SMTP Server (thường là smtp.gmail.com cổng 587)
                await client.ConnectAsync(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                
                // Xác thực tài khoản với Email và Mật khẩu ứng dụng (App Password)
                await client.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
                
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
                return false;
            }
        }
    }
}
