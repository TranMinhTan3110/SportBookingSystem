using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Services;

namespace SportBookingSystem.Controllers
{
    public class SuppliesController : Controller
    {
        private readonly ISportProductService _productService;

        public SuppliesController(ISportProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetFilterData()
        {
            var categories = await _productService.GetCategoriesByTypeAsync("Service");
            var products = await _productService.GetAllProductsAsync(null, null, null, null);
            var brands = products.Where(p => !string.IsNullOrEmpty(p.Brand)).Select(p => p.Brand).Distinct().ToList();

            return Json(new { success = true, categories, brands });
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredProducts(string? search, string? categories, string? brands, decimal? minPrice, decimal? maxPrice, string? sortBy)
        {
            var catArray = string.IsNullOrEmpty(categories) ? null : categories.Split(',');
            var brandArray = string.IsNullOrEmpty(brands) ? null : brands.Split(',');

            var products = await _productService.GetProductsForUserAsync(search, catArray, brandArray, minPrice, maxPrice, sortBy);
            return Json(new { success = true, data = products });
        }
    }
}
