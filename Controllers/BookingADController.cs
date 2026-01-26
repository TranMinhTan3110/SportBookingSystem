using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Services;

namespace SportBookingSystem.Controllers
{
    public class BookingADController : Controller
    {
        private readonly IPitchService _pitchService;

        public BookingADController(IPitchService pitchService)
        {
            _pitchService = pitchService;
        }

        public async Task<IActionResult> Index(int? categoryId, string? status, string? searchTerm)
        {
            // Lấy danh sách sân
            var pitches = await _pitchService.GetAllPitchesAsync(categoryId, status, searchTerm);

            // Lấy danh sách categories cho dropdown
            var categories = await _pitchService.GetPitchCategoriesAsync();

            // Truyền dữ liệu vào ViewBag
            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedStatus = status;
            ViewBag.SearchTerm = searchTerm;

            return View(pitches);
        }
    }
}