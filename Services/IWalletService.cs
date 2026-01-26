using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public interface IWalletService
    {
        Task<Transactions> CreateRechargeTransactionAsync(int userId, decimal amount, string txnRef);
        Task<(bool success, string message)> ExecuteRechargeAsync(VnPayResponseModel vnPayResponse);
    }
}
