using SendGrid;
using SendGrid.Helpers.Mail;

namespace SportBookingSystem.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public SendGridEmailService(IConfiguration configuration)
        {
            _apiKey = configuration["SendGrid:ApiKey"];
            _fromEmail = configuration["SendGrid:FromEmail"];
            _fromName = configuration["SendGrid:FromName"];
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otp)
        {
            try
            {
                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail);
                var subject = "Mã xác thực đặt lại mật khẩu";

                var htmlContent = $@"
                    <div style='font-family: Arial; padding: 20px; border: 1px solid #ddd;'>
                        <h2>Xác thực đặt lại mật khẩu</h2>
                        <p>Mã OTP của bạn là: <b style='font-size: 24px; color: #28a745;'>{otp}</b></p>
                        <p>Mã này có hiệu lực trong 5 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                    </div>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                var response = await client.SendEmailAsync(msg);

                Console.WriteLine($"SendGrid Response: {response.StatusCode}");

                // SendGrid trả về 202 Accepted khi thành công
                return response.StatusCode == System.Net.HttpStatusCode.Accepted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendGrid Error: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                return false;
            }
        }
    }
}