using SportBookingSystem.DTO;

namespace SportBookingSystem.Services
{
    public interface  IUserService 
    {
        Task<UserStatisticsDto> countUserAsync();
        Task<List<UserInfo>> getAllUserAsync();
        Task<bool> ToggleUserStatusAsync(string userId);
    }
}
