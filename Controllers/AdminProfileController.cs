using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SportBookingSystem.Controllers
{
    public class AdminProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("SignIn", "Login");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.Role.RoleName != "Admin")
            {
                return RedirectToAction("SignIn", "Login");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeEmail(string newEmail)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            if (string.IsNullOrEmpty(newEmail))
            {
                return Json(new { success = false, message = "Vui lòng nhập email mới." });
            }

            if (await _context.Users.AnyAsync(u => u.Email == newEmail && u.UserId != userId))
            {
                return Json(new { success = false, message = "Email này đã được sử dụng bởi tài khoản khác." });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại." });
            }

            user.Email = newEmail;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đổi email thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePhone(string newPhone)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            if (string.IsNullOrEmpty(newPhone))
            {
                return Json(new { success = false, message = "Vui lòng nhập số điện thoại mới." });
            }

            if (await _context.Users.AnyAsync(u => u.Phone == newPhone && u.UserId != userId))
            {
                return Json(new { success = false, message = "Số điện thoại này đã được sử dụng bởi tài khoản khác." });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại." });
            }

            user.Phone = newPhone;
            user.Username = newPhone;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đổi số điện thoại thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });
            }

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "Mật khẩu xác nhận không khớp." });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại." });
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không đúng." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
        }
    }
}
