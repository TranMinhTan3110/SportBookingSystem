using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Services
{
    public class SportProductService : ISportProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SportProductService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IEnumerable<Products>> GetAllProductsAsync(string? search, string? type, string? brand, string? stockStatus)
        {
            var query = _context.Products.AsQueryable();

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
                .Where(p => p.Status == true) // Chỉ lấy sản phẩm đang hoạt động
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || (p.Brand != null && p.Brand.Contains(search)));
            }

            // Categories (Multiple)
            if (categories != null && categories.Length > 0)
            {
                query = query.Where(p => categories.Contains(p.Category.CategoryName));
            }

            // Brands (Multiple)
            if (brands != null && brands.Length > 0)
            {
                query = query.Where(p => brands != null && brands.Contains(p.Brand));
            }

            // Price Range
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Sorting
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

                product.Status = true; // Mặc định khi tạo mới là hiện
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
                existing.Status = product.Status; // Cập nhật trạng thái

                if (imageFile != null && imageFile.Length > 0)
                {
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

                product.Status = !product.Status; // Đảo ngược trạng thái
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

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "asset", "img");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Lấy tên file gốc (tránh ký tự lạ nếu cần, nhưng IFormFile.FileName thường ổn)
            string fileName = imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, fileName);

            // Kiểm tra nếu file đã tồn tại thì sử dụng luôn, không lưu đè hoặc lưu mới
            if (System.IO.File.Exists(filePath))
            {
                return "/asset/img/" + fileName;
            }

            // Nếu chưa có thì mới thực hiện lưu
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/asset/img/" + fileName;
        }

        public async Task<IEnumerable<Categories>> GetCategoriesByTypeAsync(string type)
        {
            return await _context.Categories.Where(c => c.Type == type).ToListAsync();
        }
    }
}
