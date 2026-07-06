using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SportBookingSystem.Services
{
    public class PitchService : IPitchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PitchService> _logger;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IDistributedCache _cache;

        public PitchService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<PitchService> logger,
            ICloudinaryService cloudinaryService)
            ILogger<PitchService> logger,
            IDistributedCache cache)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
            _cache = cache;
        }

        /// Lấy tất cả sân với điều kiện lọc
        public async Task<List<Pitches>> GetAllPitchesAsync(int? categoryId = null, string? status = null, string? searchTerm = null)
        {
            try
            {
                // Nếu có bộ lọc thì không dùng cache
                if (categoryId.HasValue || !string.IsNullOrEmpty(status) || !string.IsNullOrEmpty(searchTerm))
                {
                    return await GetFilteredPitchesFromDbAsync(categoryId, status, searchTerm);
                }

                string cacheKey = "ALL_PITCHES";
                var cachedData = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<List<Pitches>>(cachedData) ?? new List<Pitches>();
                }

                var pitchesFromDb = await GetFilteredPitchesFromDbAsync(null, null, null);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };
                
                // Tránh lỗi vòng lặp Reference khi serialize
                var jsonOptions = new JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(pitchesFromDb, jsonOptions), cacheOptions);

                return pitchesFromDb;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sân (Redis/DB)");
                return new List<Pitches>();
            }
        }

        private async Task<List<Pitches>> GetFilteredPitchesFromDbAsync(int? categoryId, string? status, string? searchTerm)
        {
            var query = _context.Pitches.Include(p => p.Category).AsNoTracking().AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(p => p.PitchName.Contains(searchTerm));

            return await query.OrderBy(p => p.PitchName).ToListAsync();
        }

        /// Lấy thông tin sân theo ID
        public async Task<Pitches?> GetPitchByIdAsync(int pitchId)
        {
            try
            {
                return await _context.Pitches
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.PitchId == pitchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy thông tin sân ID: {pitchId}");
                return null;
            }
        }

        /// Tạo sân mới
        public async Task<(bool Success, string Message, Pitches? Pitch)> CreatePitchAsync(PitchViewModel model)
        {
            try
            {
                // Kiểm tra tên sân đã tồn tại
                var existingPitch = await _context.Pitches
                    .FirstOrDefaultAsync(p => p.PitchName == model.PitchName);

                if (existingPitch != null)
                {
                    return (false, "Tên sân đã tồn tại", null);
                }

                // Kiểm tra Category có tồn tại
                var category = await _context.Categories.FindAsync(model.CategoryId);
                if (category == null)
                {
                    return (false, "Loại sân không hợp lệ", null);
                }

                // Tạo entity mới
                var pitch = new Pitches
                {
                    PitchName = model.PitchName,
                    PricePerHour = model.PricePerHour,
                    Capacity = model.Capacity,
                    Description = model.Description,
                    Status = model.Status,
                    CategoryId = model.CategoryId
                };

                // Xử lý upload ảnh
                if (model.ImageFile != null)
                {
                    var imageUrl = await SaveImageAsync(model.ImageFile);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        pitch.ImageUrl = imageUrl;
                    }
                }

                _context.Pitches.Add(pitch);
                await _context.SaveChangesAsync();
                
                // Xóa Cache
                await _cache.RemoveAsync("ALL_PITCHES");

                return (true, "Thêm sân thành công", pitch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo sân mới");
                return (false, "Đã xảy ra lỗi khi thêm sân", null);
            }
        }

        /// Cập nhật thông tin sân
        public async Task<(bool Success, string Message)> UpdatePitchAsync(PitchViewModel model)
        {
            try
            {
                var pitch = await _context.Pitches.FindAsync(model.PitchId);
                if (pitch == null)
                {
                    return (false, "Không tìm thấy sân");
                }

                // Kiểm tra trùng tên (trừ chính nó)
                var duplicateName = await _context.Pitches
                    .AnyAsync(p => p.PitchName == model.PitchName && p.PitchId != model.PitchId);

                if (duplicateName)
                {
                    return (false, "Tên sân đã tồn tại");
                }

                // Cập nhật thông tin
                pitch.PitchName = model.PitchName;
                pitch.PricePerHour = model.PricePerHour;
                pitch.Capacity = model.Capacity;
                pitch.Description = model.Description;
                pitch.Status = model.Status;
                pitch.CategoryId = model.CategoryId;

                // Xử lý upload ảnh mới
                if (model.ImageFile != null)
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(pitch.ImageUrl))
                    {
                        await DeleteImageAsync(pitch.ImageUrl);
                    }

                    var imageUrl = await SaveImageAsync(model.ImageFile);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        pitch.ImageUrl = imageUrl;
                    }
                }

                _context.Pitches.Update(pitch);
                await _context.SaveChangesAsync();
                
                // Xóa Cache
                await _cache.RemoveAsync("ALL_PITCHES");

                return (true, "Cập nhật sân thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật sân ID: {model.PitchId}");
                return (false, "Đã xảy ra lỗi khi cập nhật sân");
            }
        }

        /// Xóa sân
        public async Task<(bool Success, string Message)> DeletePitchAsync(int pitchId)
        {
            try
            {
                var pitch = await _context.Pitches
                    .Include(p => p.Bookings)
                    .FirstOrDefaultAsync(p => p.PitchId == pitchId);

                if (pitch == null)
                {
                    return (false, "Không tìm thấy sân");
                }

                // Kiểm tra xem sân có booking nào không
                if (pitch.Bookings != null && pitch.Bookings.Any())
                {
                    return (false, "Không thể xóa sân đã có lịch đặt");
                }

                // Xóa ảnh nếu có
                if (!string.IsNullOrEmpty(pitch.ImageUrl))
                {
                    await DeleteImageAsync(pitch.ImageUrl);
                }

                _context.Pitches.Remove(pitch);
                await _context.SaveChangesAsync();

                // Xóa Cache
                await _cache.RemoveAsync("ALL_PITCHES");

                return (true, "Xóa sân thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa sân ID: {pitchId}");
                return (false, "Đã xảy ra lỗi khi xóa sân");
            }
        }

        /// Lấy danh sách Categories loại Pitch
        public async Task<List<Categories>> GetPitchCategoriesAsync()
        {
            try
            {
                return await _context.Categories
                    .Where(c => c.Type == "Pitch")
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách loại sân");
                return new List<Categories>();
            }
        }

        public async Task<List<PitchPriceSetting>> GetPitchPricesAsync(int pitchId)
        {
            return await _context.PitchPriceSettings
                .Where(p => p.PitchId == pitchId)
                .OrderBy(p => p.StartTime)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> AddPitchPriceAsync(PitchPriceViewModel model)
        {
            try
            {
                if (model.EndTime <= model.StartTime)
                    return (false, "Giờ kết thúc phải lớn hơn giờ bắt đầu");

                var setting = new PitchPriceSetting
                {
                    PitchId = model.PitchId,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Price = model.Price,
                    Note = model.Note
                };

                _context.PitchPriceSettings.Add(setting);
                await _context.SaveChangesAsync();
                return (true, "Thêm khung giờ giá thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi thêm giá");
                return (false, "Lỗi hệ thống");
            }
        }

        public async Task<(bool Success, string Message)> DeletePitchPriceAsync(int id)
        {
            var item = await _context.PitchPriceSettings.FindAsync(id);
            if (item == null) return (false, "Không tìm thấy dữ liệu");

            _context.PitchPriceSettings.Remove(item);
            await _context.SaveChangesAsync();
            return (true, "Đã xóa");
        }

        #region Private Methods - Xử lý ảnh

        /// Lưu ảnh lên Cloudinary
        private async Task<string?> SaveImageAsync(IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                    return null;

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning($"Định dạng file không hợp lệ: {extension}");
                    return null;
                }

                // Kiểm tra kích thước file (tối đa 5MB)
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    _logger.LogWarning($"File quá lớn: {imageFile.Length} bytes");
                    return null;
                }

                // Upload lên Cloudinary trong thư mục 'pitches'
                return await _cloudinaryService.UploadImageAsync(imageFile, "pitches");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu ảnh lên Cloudinary");
                return null;
            }
        }

        /// Xóa ảnh khỏi Cloudinary
        private async Task DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return;

                var deleted = await _cloudinaryService.DeleteImageAsync(imageUrl);
                if (deleted)
                {
                    _logger.LogInformation($"Đã xóa ảnh trên Cloudinary: {imageUrl}");
                }
                else
                {
                    _logger.LogWarning($"Không xóa được hoặc không cần xóa ảnh Cloudinary: {imageUrl}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa ảnh Cloudinary: {imageUrl}");
            }
        }

        #endregion
    }
}