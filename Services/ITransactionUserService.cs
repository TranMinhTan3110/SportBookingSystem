using SportBookingSystem.DTO;
using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Services
{
    public interface ITransactionUserService
    {
        Task<string> TransferMoneyAsync(int senderId, TransferDTO dto);
        Task<Users> GetUserByPhoneAsync(string phoneNumber);
        Task<List<UserTransactionDTO>> LoadUserTransactionAsync(int userId);
        Task<List<UserBookingDTO>> LoadUserBookingsAsync(int userId);
    }
}