namespace SportBookingSystem.Services
{
    public interface IQrService
    {
        string GenerateQrCode(string data);
        Task<Transactions?> GetTransactionByOrderIdAsync(int orderId);
    }
}
