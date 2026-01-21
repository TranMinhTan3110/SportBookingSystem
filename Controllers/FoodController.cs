using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Models.ViewModels;
using SportBookingSystem.Services;

namespace SportBookingSystem.Controllers
{
    public class FoodController : Controller
    {
        private readonly IFoodService _foodService;

        public FoodController(IFoodService foodService)
        {
            _foodService = foodService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = await _foodService.GetInitialDataAsync();
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> GetFilteredProducts([FromBody] FilterProductRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var response = await _foodService.GetFilteredProductsAsync(request);
            return Json(response);
        }
    }
}