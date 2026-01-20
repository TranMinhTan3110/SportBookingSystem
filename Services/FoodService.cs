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

        /// <summary>
        /// Lấy dữ liệu ban đầu cho trang Food (Categories + Products)
        /// </summary>
        public async Task<FoodIndexViewModel> GetInitialDataAsync()
        {
            // 1. Lấy danh sách Categories có Type = 'Product'
            var categories = await _context.Categories
                .Where(c => c.Type == "Product")
                .Select(c => new CategoryFilterViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ProductCount = c.Products.Count(p => p.StockQuantity > 0) // Đếm sản phẩm còn hàng
                })
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            // 2. Lấy tất cả sản phẩm thuộc các danh mục Product
            var categoryIds = categories.Select(c => c.CategoryId).ToList();
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => categoryIds.Contains(p.CategoryId))
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            // 3. Map sang ProductViewModel
            var productViewModels = products.Select(p => MapToProductViewModel(p)).ToList();

            return new FoodIndexViewModel
            {
                Categories = categories,
                Products = productViewModels,
                TotalProducts = products.Count
            };
        }

        /// <summary>
        /// Lọc sản phẩm theo các tiêu chí
        /// </summary>
        public async Task<FilterProductResponse> GetFilteredProductsAsync(FilterProductRequest request)
        {
            try
            {
                // Bắt đầu query từ Products
                var query = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Category.Type == "Product")
                    .AsQueryable();

                // 1. Lọc theo danh mục (nếu có)
                if (request.CategoryIds != null && request.CategoryIds.Any())
                {
                    query = query.Where(p => request.CategoryIds.Contains(p.CategoryId));
                }

                // 2. Lọc theo giá tối thiểu
                if (request.MinPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= request.MinPrice.Value);
                }

                // 3. Lọc theo giá tối đa
                if (request.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= request.MaxPrice.Value);
                }

                // 4. Sắp xếp theo yêu cầu
                query = request.SortBy switch
                {
                    "price-asc" => query.OrderBy(p => p.Price),
                    "price-desc" => query.OrderByDescending(p => p.Price),
                    "name-asc" => query.OrderBy(p => p.ProductName),
                    "name-desc" => query.OrderByDescending(p => p.ProductName),
                    _ => query.OrderBy(p => p.ProductName) // Mặc định sắp xếp theo tên
                };

                // 5. Thực thi query
                var products = await query.ToListAsync();

                // 6. Map sang ViewModel
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

        /// <summary>
        /// Map từ Product entity sang ProductViewModel
        /// </summary>
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

        /// <summary>
        /// Format giá tiền sang định dạng VND
        /// </summary>
        private string FormatCurrency(decimal amount)
        {
            var culture = new CultureInfo("vi-VN");
            return amount.ToString("#,##0", culture) + "₫";
        }
    }
}