using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public class FoodADService : IFoodADService
    {
        private readonly ApplicationDbContext _context;

        public FoodADService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductViewModel>> GetAllProductsAsync(string searchTerm = null, int? categoryId = null, string stockFilter = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Category.Type == "Product")
                .AsQueryable();

            // Tìm kiếm theo tên
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(searchTerm));
            }

            // Lọc theo danh mục
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query.ToListAsync();

            // Chuyển đổi sang ViewModel và tính toán trạng thái tồn kho
            var productViewModels = products.Select(p => new ProductViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Price,
                Unit = p.Unit ?? "Phần",
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                ProductType = p.ProductType,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.CategoryName,
                StockStatus = GetStockStatus(p.StockQuantity)
            }).ToList();

            // Lọc theo tình trạng kho
            if (!string.IsNullOrWhiteSpace(stockFilter))
            {
                productViewModels = productViewModels
                    .Where(p => p.StockStatus.ToLower() == stockFilter.ToLower())
                    .ToList();
            }

            return productViewModels;
        }

        public async Task<ProductViewModel> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return null;

            return new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                Unit = product.Unit ?? "Phần",
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                ProductType = product.ProductType,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.CategoryName,
                StockStatus = GetStockStatus(product.StockQuantity)
            };
        }

        public async Task<bool> CreateProductAsync(ProductCreateEditViewModel model)
        {
            try
            {
                // Lấy thông tin Category để xác định ProductType
                var category = await _context.Categories.FindAsync(model.CategoryId);
                if (category == null || category.Type != "Product")
                    return false;

                var product = new Products
                {
                    ProductName = model.ProductName,
                    Price = model.Price,
                    Unit = model.Unit ?? "Phần",
                    StockQuantity = model.StockQuantity,
                    ImageUrl = model.ImageUrl,
                    CategoryId = model.CategoryId,
                    ProductType = category.CategoryName, // Gán ProductType từ CategoryName
                    Size = null,
                    Brand = null
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(int id, ProductCreateEditViewModel model)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return false;

                // Lấy thông tin Category để cập nhật ProductType
                var category = await _context.Categories.FindAsync(model.CategoryId);
                if (category == null || category.Type != "Product")
                    return false;

                product.ProductName = model.ProductName;
                product.Price = model.Price;
                product.Unit = model.Unit ?? "Phần";
                product.StockQuantity = model.StockQuantity;
                product.ImageUrl = model.ImageUrl;
                product.CategoryId = model.CategoryId;
                product.ProductType = category.CategoryName;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return false;

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> AddStockAsync(int id, int amount)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null || amount <= 0)
                    return false;

                product.StockQuantity += amount;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<Categories>> GetProductCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.Type == "Product")
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        // Helper method để xác định trạng thái tồn kho
        private string GetStockStatus(int stockQuantity)
        {
            if (stockQuantity == 0)
                return "out";
            else if (stockQuantity <= 20)
                return "low";
            else
                return "in";
        }
    }
}