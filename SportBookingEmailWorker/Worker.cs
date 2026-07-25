using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using SportBookingSystem.DTO; // Dùng chung namespace DTO của bạn

namespace SportBookingEmailWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config; // Công cụ đọc appsettings.json

        // Truyền IConfiguration vào để lấy cấu hình MailSettings
        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var hostName = _config["RabbitMQ:HostName"] ?? "localhost";
            var factory = new ConnectionFactory() { HostName = hostName };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "invoice_email_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageString = Encoding.UTF8.GetString(body);
                var emailData = JsonSerializer.Deserialize<EmailMessageDTO>(messageString);

                if (emailData != null)
                {
                    _logger.LogInformation($"[CÓ VIỆC] Đang gửi thư SMTP cho: {emailData.ToEmail}");
                    await SendEmailAsync(emailData);
                }

                // Xong việc thì báo xóa thư khỏi Bưu điện
                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            await channel.BasicConsumeAsync(queue: "invoice_email_queue", autoAck: false, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task SendEmailAsync(EmailMessageDTO data)
        {
            try
            {
                // Móc lấy thông tin cấu hình từ file appsettings.json
                var mailSettings = _config.GetSection("MailSettings");
                var mail = new MimeMessage();
                mail.Sender = MailboxAddress.Parse(mailSettings["Mail"]);
                mail.From.Add(new MailboxAddress(mailSettings["DisplayName"], mailSettings["Mail"]));
                mail.To.Add(MailboxAddress.Parse(data.ToEmail));
                mail.Subject = $"Xác nhận đặt sân thành công - Mã {data.BookingCode}";

                var builder = new BodyBuilder();
                
                // Lọc bỏ phần chữ thừa (nếu có) để lấy đúng chuỗi Base64 gốc
                string pureBase64 = data.QrCodeBase64.Replace("data:image/png;base64,", "");
                byte[] qrBytes = Convert.FromBase64String(pureBase64);
                
                // Đính kèm hình ảnh vào thư với một cái ID bí mật là 'qrcode_image'
                var qrImage = builder.LinkedResources.Add("qrcode.png", qrBytes);
                qrImage.ContentId = "qrcode_image";

                builder.HtmlBody = $@"
                    <div style='border: 1px solid #ccc; padding: 20px; font-family: Arial;'>
                        <h2 style='color: #4CAF50;'>Cảm ơn bạn đã đặt sân!</h2>
                        <hr/>
                        <p><strong>Mã đặt sân:</strong> {data.BookingCode}</p>
                        <p><strong>Tên sân:</strong> {data.PitchName}</p>
                        <p><strong>Tổng tiền:</strong> {data.TotalAmount} VNĐ</p>
                        <div style='margin-top: 20px; margin-bottom: 20px; text-align: center;'>
                            <p><strong>Mã QR Nhận Sân:</strong></p>
                            <img src='cid:qrcode_image' alt='QR Code' width='200' style='border: 1px solid #ddd; border-radius: 8px; padding: 5px;' />
                        </div>
                        <p><i>Hệ thống tự động thông qua RabbitMQ.</i></p>
                    </div>
                ";
                mail.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                // Kết nối vào SMTP của Google/Outlook
                await smtp.ConnectAsync(mailSettings["Host"], int.Parse(mailSettings["Port"]), SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(mailSettings["Mail"], mailSettings["Password"]);
                await smtp.SendAsync(mail);
                await smtp.DisconnectAsync(true);
                
                _logger.LogInformation($"[THÀNH CÔNG] Đã bắn Email SMTP cho {data.BookingCode} rớt xuống hộp thư khách!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[LỖI CĂNG] Gửi SMTP xịt ráo: {ex.Message}");
            }
        }
    }
}
