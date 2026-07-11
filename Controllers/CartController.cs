using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;
using SportBookingSystem.Services;
using System.Security.Claims;

namespace SportBookingSystem.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IPurchaseService _purchaseService;
        private readonly IProfileService _profileService;

        public CartController(ICartService cartService, IPurchaseService purchaseService, IProfileService profileService)
        {
            _cartService = cartService;
            _purchaseService = purchaseService;
            _profileService = profileService;
        }

        /// <summary>Trang giỏ hàng</summary>
        public async Task<IActionResult> Index()
        {
            var cart = _cartService.GetCart();
            var userId = GetUserId();
            decimal walletBalance = 0;

            if (userId.HasValue)
            {
                var user = await _profileService.GetUserByIdAsync(userId.Value);
                walletBalance = user?.WalletBalance ?? 0;
            }

            var viewModel = new CartViewModel
            {
                Items = cart,
                WalletBalance = walletBalance,
                IsLoggedIn = userId.HasValue,
                CartCreatedAt = _cartService.GetCartCreationTime(),
                RemainingSeconds = _cartService.GetRemainingSeconds(),
                CartTimeoutMinutes = _cartService.GetCartTimeoutMinutes()
            };

            return View(viewModel);
        }

        /// <summary>API: Thêm sản phẩm vào giỏ (AJAX)</summary>
        [HttpPost]
        public IActionResult Add([FromBody] CartItem item)
        {
            if (item == null || item.ProductId <= 0 || item.Quantity <= 0)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

            var cart = _cartService.GetCart();
            var existing = cart.FirstOrDefault(c => c.ProductId == item.ProductId);

            if (existing != null)
            {
                existing.Quantity += item.Quantity;
                _cartService.UpdateQuantity(existing.ProductId, existing.Quantity);
            }
            else
            {
                _cartService.AddToCart(item);
            }

            return Json(new { success = true, count = _cartService.GetCartCount(), message = "Đã thêm vào giỏ hàng!" });
        }

        /// <summary>API: Cập nhật số lượng (AJAX)</summary>
        [HttpPost]
        public IActionResult Update([FromBody] UpdateCartRequest request)
        {
            if (request == null || request.ProductId <= 0)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

            _cartService.UpdateQuantity(request.ProductId, request.Quantity);

            var cart = _cartService.GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == request.ProductId);
            decimal itemTotal = item?.Total ?? 0;
            decimal cartTotal = cart.Sum(c => c.Total);

            return Json(new
            {
                success = true,
                itemTotal = itemTotal,
                cartTotal = cartTotal,
                count = cart.Sum(c => c.Quantity),
                isEmpty = !cart.Any()
            });
        }

        /// <summary>API: Xóa sản phẩm khỏi giỏ (AJAX)</summary>
        [HttpPost]
        public IActionResult Remove([FromBody] RemoveCartRequest request)
        {
            if (request == null || request.ProductId <= 0)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

            _cartService.RemoveItem(request.ProductId);

            var cart = _cartService.GetCart();

            return Json(new
            {
                success = true,
                cartTotal = cart.Sum(c => c.Total),
                count = cart.Sum(c => c.Quantity),
                isEmpty = !cart.Any()
            });
        }

        /// <summary>API: Lấy số lượng giỏ hàng (badge)</summary>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Count()
        {
            var count = _cartService.GetCartCount();
            return Json(new { count });
        }

        /// <summary>API: Làm mới thời gian giỏ hàng</summary>
        [HttpPost]
        public IActionResult RefreshTimer()
        {
            _cartService.SetCartCreationTime(DateTime.Now);
            return Json(new { success = true, remainingSeconds = _cartService.GetRemainingSeconds() });
        }

        /// <summary>Thanh toán toàn bộ giỏ hàng</summary>
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Json(new { success = false, message = "Vui lòng đăng nhập để thanh toán!" });

            var cart = _cartService.GetCart();
            if (!cart.Any())
                return Json(new { success = false, message = "Giỏ hàng trống!" });

            var result = await _purchaseService.CheckoutCartAsync(userId.Value, cart);

            if (result.Success)
            {
                _cartService.ClearCart();
            }

            return Json(new { success = result.Success, message = result.Message, qrCode = result.QrCode });
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out var userId))
                return userId;
            return null;
        }
    }

    // Request models cho AJAX calls
    public class UpdateCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveCartRequest
    {
        public int ProductId { get; set; }
    }
}
