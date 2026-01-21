using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Models.ViewModels;
using SportBookingSystem.Services;
namespace SportBookingSystem.Controllers
{
    public class PitchesController : Controller
    {
        private readonly IPitchService _pitchService;
        private readonly ILogger<PitchesController> _logger;

        public PitchesController(IPitchService pitchService, ILogger<PitchesController> logger)
        {
            _pitchService = pitchService;
            _logger = logger;
        }

        /// Trang danh sách sân (Admin)
        [HttpGet]
        public async Task<IActionResult> Index(int? categoryId, string? status, string? searchTerm)
        {
            try
            {
                // Lấy danh sách sân với các điều kiện lọc
                var pitches = await _pitchService.GetAllPitchesAsync(categoryId, status, searchTerm);

                // Lấy danh sách categories để hiển thị trong dropdown filter
                var categories = await _pitchService.GetPitchCategoriesAsync();

                ViewBag.Categories = categories;
                ViewBag.SelectedCategoryId = categoryId;
                ViewBag.SelectedStatus = status;
                ViewBag.SearchTerm = searchTerm;

                return View(pitches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang quản lý sân");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải dữ liệu";
                return View(new List<SportBookingSystem.Models.Entities.Pitches>());
            }
        }

        /// Tạo sân mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] PitchViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = errors
                    });
                }

                var result = await _pitchService.CreatePitchAsync(model);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    data = result.Pitch != null ? new
                    {
                        pitchId = result.Pitch.PitchId,
                        pitchName = result.Pitch.PitchName,
                        imageUrl = result.Pitch.ImageUrl
                    } : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo sân mới");
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi thêm sân"
                });
            }
        }

        /// Lấy thông tin sân để Edit
        [HttpGet]
        public async Task<IActionResult> GetPitch(int id)
        {
            try
            {
                var pitch = await _pitchService.GetPitchByIdAsync(id);

                if (pitch == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy sân"
                    });
                }

                var viewModel = new PitchViewModel
                {
                    PitchId = pitch.PitchId,
                    PitchName = pitch.PitchName,
                    PricePerHour = pitch.PricePerHour,
                    Capacity = pitch.Capacity,
                    Description = pitch.Description,
                    Status = pitch.Status,
                    CategoryId = pitch.CategoryId,
                    ImageUrl = pitch.ImageUrl
                };

                return Json(new
                {
                    success = true,
                    data = viewModel
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy thông tin sân ID: {id}");
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi"
                });
            }
        }


        /// Cập nhật thông tin sân
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] PitchViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = errors
                    });
                }

                var result = await _pitchService.UpdatePitchAsync(model);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật sân ID: {model.PitchId}");
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi cập nhật sân"
                });
            }
        }

        /// Xóa sân
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _pitchService.DeletePitchAsync(id);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa sân ID: {id}");
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi xóa sân"
                });
            }
        }

        /// Lấy danh sách Categories cho dropdown
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _pitchService.GetPitchCategoriesAsync();

                return Json(new
                {
                    success = true,
                    data = categories.Select(c => new {
                        categoryId = c.CategoryId,
                        categoryName = c.CategoryName
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách loại sân");
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi"
                });
            }
        }
    }
}