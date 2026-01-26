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

                var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
                Random rnd = new Random();
                string txnRef = $"NAP-{rnd.Next(100000, 999999)}";

                await _walletService.CreateRechargeTransactionAsync(userId, request.Amount, txnRef);

                var paymentUrl = _vnPayService.CreatePaymentUrl(request.Amount, txnRef, HttpContext);

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
                var response = _vnPayService.ProcessVnPayReturn(Request.Query);

                var (success, message) = await _walletService.ExecuteRechargeAsync(response);

                if (response.Success)
                {
                    TempData["PaymentStatus"] = "success";
                    TempData["PaymentMessage"] = $"Nạp tiền thành công {response.Amount:N0}₫ vào ví. Mã GD: {response.TransactionCode}";
                }
                else
                {
                    TempData["PaymentStatus"] = "error";
                    TempData["PaymentMessage"] = "Giao dịch thất bại hoặc chữ ký không hợp lệ";
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["PaymentStatus"] = "error";
                TempData["PaymentMessage"] = $"Lỗi hệ thống: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public class RechargeRequest
        {
            public decimal Amount { get; set; }
        }
    }
}