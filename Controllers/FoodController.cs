using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Models.ViewModels;
using SportBookingSystem.Services;

namespace SportBookingSystem.Controllers
{
    public class FoodController : Controller
    {
        private readonly IFoodService _foodService;
        private readonly IPurchaseService _purchaseService;

        public FoodController(IFoodService foodService, IPurchaseService purchaseService)
        {
            _foodService = foodService;
            _purchaseService = purchaseService;
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

        [HttpPost]
        public async Task<IActionResult> Purchase(int productId, int quantity)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để đặt đồ ăn!" });
                }

                var result = await _purchaseService.PurchaseProductAsync(userId, productId, quantity);

                return Json(new { 
                    success = result.Success, 
                    message = result.Message, 
                    qrCode = result.QrCode,
                    remainingSeconds = 900 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
}