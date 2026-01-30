using SportBookingSystem.DTO;
using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Services
{
    public interface ITransactionService
    {
        Task<PaymentDashboardDTO> GetFilteredPaymentsAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            string? type = null,
            string? status = null,
            DateTime? date = null
        );
        Task<RewardSettingDTO> GetCurrentRewardSettingsAsync();
        Task SaveRewardSettingsAsync(RewardSettingDTO dto);
        Task<PaymentDashboardDTO> GetPaymentDashboardAsync(int page, int pageSize);
        Task<UserDepositInfoDTO?> GetUserByPhoneAsync(string phone);
        Task<UserDepositInfoDTO?> GetUserByIdAsync(int id);
        Task<(bool Success, string Message, string? TransactionCode)> CreateDepositAsync(CreateDepositDTO dto);
        Task<List<ProductListDTO>> GetProductsAsync();
        Task<OrderFulfillmentDTO?> GetOrderDetailsByIdAsync(int orderId);
        Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, string newStatus);
        Task CancelExpiredOrdersAsync();
        Task AutoCancelExpiredBookingsAsync();
        Task<TransactionDetailDTO?> GetTransactionDetailsAsync(string code);
    }
}