using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public class PitchService : IPitchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PitchService> _logger;

        public PitchService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<PitchService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        /// Lấy tất cả sân với điều kiện lọc
        public async Task<List<Pitches>> GetAllPitchesAsync(int? categoryId = null, string? status = null, string? searchTerm = null)
        {
            try
            {
                var query = _context.Pitches.Include(p => p.Category).AsQueryable();

                // Lọc theo Category
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                // Lọc theo Status
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(p => p.Status == status);
                }

                // Tìm kiếm theo tên
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p => p.PitchName.Contains(searchTerm));
                }

                return await query.OrderBy(p => p.PitchName).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sân");
                return new List<Pitches>();
            }
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
                        DeleteImage(pitch.ImageUrl);
                    }

                    var imageUrl = await SaveImageAsync(model.ImageFile);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        pitch.ImageUrl = imageUrl;
                    }
                }

                _context.Pitches.Update(pitch);
                await _context.SaveChangesAsync();

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
                    DeleteImage(pitch.ImageUrl);
                }

                _context.Pitches.Remove(pitch);
                await _context.SaveChangesAsync();

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

        #region Private Methods - Xử lý ảnh

        /// Lưu ảnh vào thư mục wwwroot/asset/img/
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

                // Tạo tên file unique
                var fileName = $"pitch_{Guid.NewGuid()}{extension}";
                var uploadPath = Path.Combine(_environment.WebRootPath, "asset", "img");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                return $"/asset/img/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu ảnh");
                return null;
            }
        }

        /// Xóa ảnh khỏi thư mục
        private void DeleteImage(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return;

                var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"Đã xóa ảnh: {imageUrl}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa ảnh: {imageUrl}");
            }
        }

        #endregion
    }
}