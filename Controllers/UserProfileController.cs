using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Models.ViewModels;
using SportBookingSystem.Services;
using System.Security.Claims;

namespace SportBookingSystem.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public UserProfileController(IProfileService profileService)
        {
            _profileService = profileService;
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

            var viewModel = new UserProfileViewModel
            {
                User = user,
                UpcomingBooking = upcomingBooking
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
