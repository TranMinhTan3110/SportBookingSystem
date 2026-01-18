using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Models.ViewModels;
using SportBookingSystem.Services;
using System.Security.Claims;

namespace SportBookingSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILoginServices _loginServices;

        public LoginController(ILoginServices loginServices)
        {
            _loginServices = loginServices;
        }
        [HttpPost]
        public async Task<IActionResult> SignIn(string phoneNumber, string password)
        {
            var user = await _loginServices.CheckLoginAsync(phoneNumber, password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.RoleName),
                    new Claim("FullName", user.FullName ?? "")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                string redirectUrl = "/Dashboard/Index"; 

                if (user.Role.RoleName == "Admin")
                {
                    redirectUrl = "/Dashboard/Index"; 
                }
                else if (user.Role.RoleName == "User")
                {
                    redirectUrl = "/Home/Index"; 
                }
                return Json(new { status = "success", redirect = redirectUrl });
            }

            return Json(new { status = "error", message = "Số điện thoại hoặc mật khẩu không chính xác!" });
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            // Kiểm tra trùng số điện thoại
            if (await _loginServices.IsPhoneExistsAsync(model.Phone))
            {
                return Json(new { status = "error", message = "Số điện thoại này đã được đăng ký!" });
            }

            // Kiểm tra trùng Email
            if (await _loginServices.IsEmailExistsAsync(model.Email))
            {
                return Json(new { status = "error", message = "Email này đã được sử dụng!" });
            }

            var result = await _loginServices.RegisterUserAsync(model);
            if (result)
            {
                return Json(new { status = "success", message = "Đăng ký thành công!", redirect = "/Login/SignIn" });
            }

            return Json(new { status = "error", message = "Đã có lỗi xảy ra trong quá trình đăng ký." });
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var result = await _loginServices.SendOtpEmailAsync(email);
            if (result)
            {
                return Json(new { status = "success", message = "Mã OTP đã được gửi đến email của bạn!" });
            }
            return Json(new { status = "error", message = "Email không tồn tại trong hệ thống." });
        }
        [HttpPost]
        public async Task<IActionResult> VerifyAndResetPassword(string email, string otp, string newPassword)
        {
            var result = await _loginServices.VerifyOtpAndResetPasswordAsync(email, otp, newPassword);
            if (result)
            {
                return Json(new { status = "success" });
            }
            return Json(new { status = "error", message = "Mã OTP không đúng hoặc đã hết hạn." });
        }
        [HttpGet]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("SignIn", "Login");
        }
        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }
        public IActionResult SignUp()
        {
            return View();
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        public IActionResult ResetPassword()
        {
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}