using System.Threading.Tasks;

namespace SportBookingSystem.Services
{
    public interface IPurchaseService
    {
        Task<(bool Success, string Message, string? QrCode)> PurchaseProductAsync(int userId, int productId, int quantity);
    }
}
