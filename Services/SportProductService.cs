using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Services
{
    public class SportProductService : ISportProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICloudinaryService _cloudinaryService;

        public SportProductService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<IEnumerable<Products>> GetAllProductsAsync(string? search, string? type, string? brand, string? stockStatus)
        {
            var query = _context.Products.Include(p => p.Category)
                .Where(p => p.Category.Type == "Service") 
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || (p.Brand != null && p.Brand.Contains(search)));
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(p => p.ProductType == type);
            }

            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Brand == brand);
            }

            if (!string.IsNullOrEmpty(stockStatus))
            {
                switch (stockStatus)
                {
                    case "in":
                        query = query.Where(p => p.StockQuantity > 10);
                        break;
                    case "low":
                        query = query.Where(p => p.StockQuantity > 0 && p.StockQuantity <= 10);
                        break;
                    case "out":
                        query = query.Where(p => p.StockQuantity == 0);
                        break;
                }
            }

            return await query.OrderByDescending(p => p.ProductId).ToListAsync();
        }

        public async Task<IEnumerable<Products>> GetProductsForUserAsync(string? search, string[]? categories, string[]? brands, decimal? minPrice, decimal? maxPrice, string? sortBy)
        {
            var query = _context.Products.Include(p => p.Category)
                .Where(p => p.Status == true && p.Category.Type == "Service") // Chỉ lấy sản phẩm Service đang hoạt động
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || (p.Brand != null && p.Brand.Contains(search)));
            }


            if (categories != null && categories.Length > 0)
            {
                query = query.Where(p => categories.Contains(p.Category.CategoryName));
            }


            if (brands != null && brands.Length > 0)
            {
                query = query.Where(p => brands != null && brands.Contains(p.Brand));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc" => query.OrderBy(p => p.ProductName),
                "newest" => query.OrderByDescending(p => p.ProductId),
                _ => query.OrderByDescending(p => p.ProductId)
            };

            return await query.ToListAsync();
        }

        public async Task<Products?> GetProductByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<bool> CreateProductAsync(Products product, IFormFile? imageFile)
        {
            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    product.ImageUrl = await SaveImageAsync(imageFile);
                }

                product.Status = true; 
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateProductAsync: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(Products product, IFormFile? imageFile)
        {
            try
            {
                var existing = await _context.Products.FindAsync(product.ProductId);
                if (existing == null) return false;

                existing.ProductName = product.ProductName;
                existing.Price = product.Price;
                existing.Unit = product.Unit;
                existing.StockQuantity = product.StockQuantity;
                existing.Size = product.Size;
                existing.Brand = product.Brand;
                existing.ProductType = product.ProductType;
                existing.CategoryId = product.CategoryId;

                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                    {
                        await _cloudinaryService.DeleteImageAsync(existing.ImageUrl);
                    }
                    existing.ImageUrl = await SaveImageAsync(imageFile);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdateProductAsync: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return false;

                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    await _cloudinaryService.DeleteImageAsync(product.ImageUrl);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in DeleteProductAsync: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> ToggleProductStatusAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return false;

                product.Status = !product.Status; 
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in ToggleProductStatusAsync: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateStockAsync(int id, int addedQuantity)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return false;

                product.StockQuantity += addedQuantity;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdateStockAsync: " + ex.Message);
                return false;
            }
        }

        private async Task<string?> SaveImageAsync(IFormFile imageFile)
        {
            return await _cloudinaryService.UploadImageAsync(imageFile, "products");
        }

        public async Task<IEnumerable<Categories>> GetCategoriesByTypeAsync(string type)
        {
            return await _context.Categories.Where(c => c.Type == type).ToListAsync();
        }
    }
}
