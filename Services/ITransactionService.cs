using SportBookingSystem.DTO;

namespace SportBookingSystem.Services
{
    public interface ITransactionService
    {
        Task<PaymentDashboardDTO> GetPaymentDashboardAsync(int page, int pageSize);
        Task<UserDepositInfoDTO?> GetUserByPhoneAsync(string phone);
        Task<(bool Success, string Message, string? TransactionCode)> CreateDepositAsync(CreateDepositDTO dto);
        Task<List<ProductListDTO>> GetProductsAsync();
    }
}