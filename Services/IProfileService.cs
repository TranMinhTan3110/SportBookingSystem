using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Services
{
    public interface IProfileService
    {
        Task<Users?> GetUserByIdAsync(int userId);
        Task<Users?> GetAdminByIdAsync(int userId);
        Task<(bool Success, string Message)> ChangeEmailAsync(int userId, string newEmail);
        Task<(bool Success, string Message)> ChangePhoneAsync(int userId, string newPhone);
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<Bookings?> GetUpcomingBookingAsync(int userId);
        Task<Orders?> GetLatestPendingOrderAsync(int userId);
        Task<Orders?> GetOrderByIdAsync(int orderId, int userId);
        Task<List<Bookings>> GetActiveBookingsAsync(int userId);
    }
}
