using SportBookingSystem.DTO;

namespace SportBookingSystem.Services
{
    public interface ITransactionUserService
    {
        Task<List<UserTransactionDTO>> LoadUserTransactionAsync(int userId);
        Task<List<UserBookingDTO>> LoadUserBookingsAsync(int userId);
    }
}