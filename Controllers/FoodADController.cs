using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Models.ViewModels;
using SportBookingSystem.Services;
using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Controllers
{
    public class FoodADController : Controller
    {
        private readonly IFoodADService _foodADService;

        public FoodADController(IFoodADService foodADService)
        {
            _foodADService = foodADService;
        }

        // GET: FoodAD/Index
        public async Task<IActionResult> Index(string searchTerm = null, int? categoryId = null, string stockFilter = null)
        {
            var products = await _foodADService.GetAllProductsAsync(searchTerm, categoryId, stockFilter);
            var categories = await _foodADService.GetProductCategoriesAsync();

            ViewBag.Categories = categories;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CategoryId = categoryId;
            ViewBag.StockFilter = stockFilter;

            return View(products);
        }

        // POST: FoodAD/Create (API)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = "error", message = "Dữ liệu không hợp lệ!" });
            }

            var result = await _foodADService.CreateProductAsync(model);

            if (result)
            {
                return Json(new { status = "success", message = "Thêm sản phẩm thành công!" });
            }

            return Json(new { status = "error", message = "Có lỗi xảy ra khi thêm sản phẩm!" });
        }

        // GET: FoodAD/GetProduct/{id} (API)
        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _foodADService.GetProductByIdAsync(id);

            if (product == null)
            {
                return Json(new { status = "error", message = "Không tìm thấy sản phẩm!" });
            }

            return Json(new
            {
                status = "success",
                data = new
                {
                    productId = product.ProductId,
                    productName = product.ProductName,
                    price = product.Price,
                    unit = product.Unit,
                    stockQuantity = product.StockQuantity,
                    imageUrl = product.ImageUrl,
                    categoryId = product.CategoryId
                }
            });
        }

        // POST: FoodAD/Update (API)
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] ProductCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = "error", message = "Dữ liệu không hợp lệ!" });
            }

            var result = await _foodADService.UpdateProductAsync(model.ProductId, model);

            if (result)
            {
                return Json(new { status = "success", message = "Cập nhật sản phẩm thành công!" });
            }

            return Json(new { status = "error", message = "Có lỗi xảy ra khi cập nhật sản phẩm!" });
        }

        // POST: FoodAD/Delete/{id} (API)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _foodADService.DeleteProductAsync(id);

            if (result)
            {
                return Json(new { status = "success", message = "Xóa sản phẩm thành công!" });
            }

            return Json(new { status = "error", message = "Có lỗi xảy ra khi xóa sản phẩm!" });
        }

        // POST: FoodAD/AddStock (API)
        [HttpPost]
        public async Task<IActionResult> AddStock([FromBody] AddStockViewModel model)
        {
            if (model.Amount <= 0)
            {
                return Json(new { status = "error", message = "Số lượng phải lớn hơn 0!" });
            }

            var result = await _foodADService.AddStockAsync(model.ProductId, model.Amount);

            if (result)
            {
                var product = await _foodADService.GetProductByIdAsync(model.ProductId);
                return Json(new
                {
                    status = "success",
                    message = $"Đã thêm {model.Amount} {product.Unit} vào kho!",
                    newStock = product.StockQuantity
                });
            }

            return Json(new { status = "error", message = "Có lỗi xảy ra khi cập nhật số lượng!" });
        }
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    return Json(new { success = false, message = "Không có file được chọn!" });
                }


                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(imageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return Json(new { success = false, message = "Chỉ chấp nhận file hình ảnh (jpg, jpeg, png, gif, webp)!" });
                }

                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "Kích thước file không được vượt quá 5MB!" });
                }

                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/products/{fileName}";
                return Json(new { success = true, url = imageUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi upload hình ảnh: " + ex.Message });
            }
        }
    }
}