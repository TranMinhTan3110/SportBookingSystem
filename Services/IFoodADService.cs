using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public interface IFoodADService
    {
        Task<List<ProductViewModel>> GetAllProductsAsync(string searchTerm = null, int? categoryId = null, string stockFilter = null);
        Task<ProductViewModel> GetProductByIdAsync(int id);
        Task<bool> CreateProductAsync(ProductCreateEditViewModel model);
        Task<bool> UpdateProductAsync(int id, ProductCreateEditViewModel model);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> AddStockAsync(int id, int amount);
        Task<List<Categories>> GetProductCategoriesAsync();
    }
}