using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Services
{
    public interface IPurchaseService
    {
        Task<(bool Success, string Message, string? QrCode)> PurchaseProductAsync(int userId, int productId, int quantity);
        Task<(bool Success, string Message, string? QrCode)> CheckoutCartAsync(int userId, List<CartItem> cartItems);
    }
}
