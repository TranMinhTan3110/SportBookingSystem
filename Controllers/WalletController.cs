using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Constants;
using SportBookingSystem.Services;
using System.Security.Claims;

namespace SportBookingSystem.Controllers
{
    [Authorize]
    public class WalletController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly IWalletService _walletService;

        public WalletController(IVnPayService vnPayService, IWalletService walletService)
        {
            _vnPayService = vnPayService;
            _walletService = walletService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateVNPayUrl([FromBody] RechargeRequest request)
        {
            try
            {
                if (request.Amount < 10000)
                {
                    return Json(new { success = false, message = "Số tiền nạp tối thiểu là 10.000₫" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                var userId = int.Parse(userIdClaim);

                // Tạo mã giao dịch
                var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
                var random = new Random().Next(1000, 9999);
                var txnRef = $"{TransactionPrefixes.Recharge}{timestamp}{random}";

                // Lưu giao dịch
                await _walletService.CreateRechargeTransactionAsync(userId, request.Amount, txnRef);

                // Tạo URL thanh toán VNPay
                var paymentUrl = _vnPayService.CreatePaymentUrl(request.Amount, txnRef, HttpContext);

                // ✅ THÊM DEBUG INFO VÀO RESPONSE
                return Json(new
                {
                    success = true,
                    paymentUrl,
                    debug = new
                    {
                        txnRef = txnRef,
                        txnRefLength = txnRef.Length,
                        amount = request.Amount,
                        userId = userId,
                        timestamp = timestamp
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> VnPayReturn()
        {
            try
            {
                var vnPayResponse = _vnPayService.ProcessVnPayReturn(Request.Query);
                var (success, message) = await _walletService.ExecuteRechargeAsync(vnPayResponse);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xử lý giao dịch: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public class RechargeRequest
        {
            public decimal Amount { get; set; }
        }
    }
}