using SportBookingSystem.DTO;
using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Services
{
    public interface ITransactionUserService
    {
        Task<string> TransferMoneyAsync(int senderId, TransferDTO dto);
        Task<Users> GetUserByPhoneAsync(string phoneNumber);
        Task<(List<UserTransactionDTO> Data, int TotalRecords)> LoadUserTransactionAsync(int userId, int page, int pageSize);
        Task<(List<UserBookingDTO> Data, int TotalRecords)> LoadUserBookingsAsync(int userId, int page, int pageSize);
        Task<(List<UserTransferDTO> Data, int TotalRecords)> LoadUserTransfersAsync(int userId, int page, int pageSize);
    }
}