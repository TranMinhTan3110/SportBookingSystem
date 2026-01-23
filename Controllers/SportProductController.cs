using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Services;

namespace SportBookingSystem.Controllers
{
    public class SportProductController : Controller
    {
        private readonly ISportProductService _productService;

        public SportProductController(ISportProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string? search, string? type, string? brand, string? stockStatus)
        {
            var products = await _productService.GetAllProductsAsync(search, type, brand, stockStatus);
            return Json(new { success = true, data = products });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _productService.GetCategoriesByTypeAsync("Service");
            return Json(new { success = true, data = categories });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
            return Json(new { success = true, data = product });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Products product, IFormFile? imageFile)
        {
            try
            {
                if (product == null)
                {
                    return Json(new { success = false, message = "Dữ liệu gửi lên rỗng (null)!" });
                }

                if (ModelState.IsValid)
                {
                    var result = await _productService.CreateProductAsync(product, imageFile);
                    if (result)
                    {
                        return Json(new { success = true, message = "Thêm sản phẩm thành công!" });
                    }
                    return Json(new { success = false, message = "Có lỗi xảy ra khi lưu sản phẩm vào CSDL!" });
                }

                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!", errors = errors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromForm] Products product, IFormFile? imageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _productService.UpdateProductAsync(product, imageFile);
                    if (result) return Json(new { success = true, message = "Cập nhật sản phẩm thành công!" });
                    return Json(new { success = false, message = "Cập nhật thất bại!" });
                }
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (result) return Json(new { success = true, message = "Đã xóa sản phẩm!" });
            return Json(new { success = false, message = "Xóa thất bại!" });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _productService.ToggleProductStatusAsync(id);
            if (result) return Json(new { success = true, message = "Đã cập nhật trạng thái hiển thị!" });
            return Json(new { success = false, message = "Cập nhật thất bại!" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(int id, int quantity)
        {
            var result = await _productService.UpdateStockAsync(id, quantity);
            if (result) return Json(new { success = true, message = "Đã cập nhật số lượng tồn kho!" });
            return Json(new { success = false, message = "Cập nhật thất bại!" });
        }
    }
}
