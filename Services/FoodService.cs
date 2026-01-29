using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.ViewModels;
using System.Globalization;

namespace SportBookingSystem.Services
{
    public class FoodService : IFoodService
    {
        private readonly ApplicationDbContext _context;

        public FoodService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// Lấy dữ liệu ban đầu cho trang Food 
        public async Task<FoodIndexViewModel> GetInitialDataAsync()
        {
            // Lấy danh sách Categories có Type = 'Product'
            var categories = await _context.Categories
                .Where(c => c.Type == "Product")
                .Select(c => new CategoryFilterViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ProductCount = c.Products.Count(p => p.StockQuantity > 0 && p.Status == true) 
                })
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            // Lấy sản phẩm
            var categoryIds = categories.Select(c => c.CategoryId).ToList();
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => categoryIds.Contains(p.CategoryId) && p.Status == true) 
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            var productViewModels = products.Select(p => MapToProductViewModel(p)).ToList();

            return new FoodIndexViewModel
            {
                Categories = categories,
                Products = productViewModels,
                TotalProducts = products.Count
            };
        }

        /// Lọc sản phẩm theo các tiêu chí
        public async Task<FilterProductResponse> GetFilteredProductsAsync(FilterProductRequest request)
        {
            try
            {
                var query = _context.Products
            .Include(p => p.Category)
            .Where(p => p.Category.Type == "Product" && p.Status == true) 
            .AsQueryable();

                if (request.CategoryIds != null && request.CategoryIds.Any())
                {
                    query = query.Where(p => request.CategoryIds.Contains(p.CategoryId));
                }

                if (request.MinPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= request.MinPrice.Value);
                }

                if (request.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= request.MaxPrice.Value);
                }

                query = request.SortBy switch
                {
                    "price-asc" => query.OrderBy(p => p.Price),
                    "price-desc" => query.OrderByDescending(p => p.Price),
                    "name-asc" => query.OrderBy(p => p.ProductName),
                    "name-desc" => query.OrderByDescending(p => p.ProductName),
                    _ => query.OrderBy(p => p.ProductName) // Mặc định sắp xếp theo tên
                };

                var products = await query.ToListAsync();

                var productViewModels = products.Select(p => MapToProductViewModel(p)).ToList();

                return new FilterProductResponse
                {
                    Products = productViewModels,
                    TotalCount = products.Count,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new FilterProductResponse
                {
                    Success = false,
                    Message = $"Lỗi khi lọc sản phẩm: {ex.Message}"
                };
            }
        }

        private ProductViewModelUser MapToProductViewModel(Models.Entities.Products product)
        {
            return new ProductViewModelUser
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                FormattedPrice = FormatCurrency(product.Price),
                ImageUrl = product.ImageUrl ?? "/asset/img/default-product.jpg",
                CategoryName = product.Category?.CategoryName ?? "Chưa phân loại",
                CategoryId = product.CategoryId,
                StockQuantity = product.StockQuantity,
                Size = product.Size,
                Brand = product.Brand,
                ProductType = product.ProductType
            };
        }

        private string FormatCurrency(decimal amount)
        {
            var culture = new CultureInfo("vi-VN");
            return amount.ToString("#,##0", culture) + "₫";
        }
    }
}