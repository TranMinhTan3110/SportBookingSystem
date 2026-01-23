using SportBookingSystem.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace SportBookingSystem.Services
{
    public interface ISportProductService
    {
        Task<IEnumerable<Products>> GetAllProductsAsync(string? search, string? type, string? brand, string? stockStatus);
        Task<IEnumerable<Products>> GetProductsForUserAsync(string? search, string[]? categories, string[]? brands, decimal? minPrice, decimal? maxPrice, string? sortBy);
        Task<Products?> GetProductByIdAsync(int id);
        Task<bool> CreateProductAsync(Products product, IFormFile? imageFile);
        Task<bool> UpdateProductAsync(Products product, IFormFile? imageFile);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> ToggleProductStatusAsync(int id);
        Task<bool> UpdateStockAsync(int id, int addedQuantity);
        Task<IEnumerable<Categories>> GetCategoriesByTypeAsync(string type);
    }
}
