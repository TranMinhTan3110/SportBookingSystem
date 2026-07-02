using System.Net;
using System.Text.Json;

namespace SportBookingSystem.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Cho phép Request đi tiếp vào Controllers
                await _next(context);
            }
            catch (Exception ex)
            {
                // 1. Chụp lại lỗi hệ thống và ghi Log ẩn để Developer (bạn) xem
                _logger.LogError(ex, "Lỗi hệ thống không mong muốn: {Message}", ex.Message);

                // 2. Format lại lỗi trả về cho Frontend
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Ép kiểu phản hồi về dạng JSON
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Tự định nghĩa câu trả lời cho Frontend (Không bao giờ trả ex.Message thật)
            var response = new
            {
                success = false,
                message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau!",
                // devMessage = exception.Message // Chú ý: Chỉ mở dòng này khi đang Code ở máy tính (Local)
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
