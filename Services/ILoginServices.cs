using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public interface ILoginServices
    {
        
        Task<Users?> CheckLoginAsync(string phone, string password);

        Task<bool> IsPhoneExistsAsync(string phone);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> RegisterUserAsync(SignUpViewModel model);

        Task<bool> SendOtpEmailAsync(string email);
        Task<bool> VerifyOtpAndResetPasswordAsync(string email, string otp, string newPassword);
        Task<Users?> GetUserByIdAsync(int userId);
    }
}