using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Models.ViewModels;
using SportBookingSystem.Services;
using System.Security.Claims;

namespace SportBookingSystem.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly IQrService _qrService;

        public UserProfileController(IProfileService profileService, IQrService qrService)
        {
            _profileService = profileService;
            _qrService = qrService;
        }

        [HttpGet]
        public IActionResult GenerateBookingQr(string checkInCode)
        {
            if (string.IsNullOrEmpty(checkInCode)) return BadRequest("Mã check-in không hợp lệ");
            string qrContent = $"BOOKING:{checkInCode}";
            string qrBase64 = _qrService.GenerateQrCode(qrContent);
            return Json(new { success = true, qrCode = qrBase64 });
        }

        [HttpGet]
        public async Task<IActionResult> GenerateOrderQr(int orderId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId)) return Unauthorized();

            var order = await _profileService.GetOrderByIdAsync(orderId, userId);
            if (order == null) return NotFound("Không tìm thấy đơn hàng");

            var detail = order.OrderDetails.FirstOrDefault();
            if (detail == null) return BadRequest("Đơn hàng lỗi (không có sản phẩm)");

            string qrContent = $"PURCHASE:{order.OrderId}:{userId}:{detail.ProductId}:{detail.Quantity}";
            string qrBase64 = _qrService.GenerateQrCode(qrContent);

            var remainingSeconds = (int)(order.OrderDate.AddMinutes(15) - DateTime.Now).TotalSeconds;
            if (remainingSeconds < 0) remainingSeconds = 0;

            var transaction = await _qrService.GetTransactionByOrderIdAsync(orderId);
            string orderCode = transaction?.TransactionCode ?? (string.IsNullOrEmpty(order.OrderCode) ? $"ORD-{order.OrderId}" : order.OrderCode);

            return Json(new { success = true, qrCode = qrBase64, remainingSeconds = remainingSeconds, orderCode = orderCode });
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("SignIn", "Login");
            }

            var user = await _profileService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("SignIn", "Login");
            }

            var upcomingBooking = await _profileService.GetUpcomingBookingAsync(userId);
            var pendingOrders = await _profileService.GetPendingOrdersAsync(userId);

            var viewModel = new UserProfileViewModel
            {
                User = user,
                UpcomingBooking = upcomingBooking,
                PendingOrders = pendingOrders
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeEmail(string newEmail)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            var result = await _profileService.ChangeEmailAsync(userId, newEmail);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePhone(string newPhone)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            var result = await _profileService.ChangePhoneAsync(userId, newPhone);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "Mật khẩu xác nhận không khớp." });
            }

            var result = await _profileService.ChangePasswordAsync(userId, currentPassword, newPassword);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
