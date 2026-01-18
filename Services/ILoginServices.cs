using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public interface ILoginServices
    {
        // Xác thực người dùng bằng số điện thoại và mật khẩu
        Task<Users> CheckLoginAsync(string phone, string password);

        //sign up
        Task<bool> IsPhoneExistsAsync(string phone);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> RegisterUserAsync(SignUpViewModel model);
        //forgot password
        Task<bool> SendOtpEmailAsync(string email);
        Task<bool> VerifyOtpAndResetPasswordAsync(string email, string otp, string newPassword);
    }
}