using SportBookingSystem.Models.ViewModels;
namespace SportBookingSystem.Services
{
    public interface IFoodService
    {
        Task<FoodIndexViewModel> GetInitialDataAsync();
        Task<FilterProductResponse> GetFilteredProductsAsync(FilterProductRequest request);
    }
}