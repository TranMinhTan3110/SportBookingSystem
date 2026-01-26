using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.DTO;
using SportBookingSystem.Services;
using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.DTOs;
using SportBookingSystem.Models.EF;

namespace SportBookingSystem.Controllers
{
    public class AdminPaymentController : Controller
    {
        private readonly ITransactionService _transactionService;

        public AdminPaymentController(ITransactionService ts) => _transactionService = ts;

        public async Task<IActionResult> Index(
              int page = 1,
              int pageSize = 10,
              string? search = null,
              string? type = null,
              string? status = null,
              DateTime? date = null)
        {
            SportBookingSystem.DTO.PaymentDashboardDTO model;

            bool hasFilters = !string.IsNullOrWhiteSpace(search) ||
                            (!string.IsNullOrWhiteSpace(type) && type != "all") ||
                            (!string.IsNullOrWhiteSpace(status) && status != "all") ||
                            date.HasValue;

            if (hasFilters)
            {
                model = await _transactionService.GetFilteredPaymentsAsync(
                    page, pageSize, search, type, status, date);
            }
            else
            {
                model = await _transactionService.GetPaymentDashboardAsync(page, pageSize);
            }

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentType = type;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentDate = date?.ToString("yyyy-MM-dd");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> FilterPayments(
            int page = 1,
            int pageSize = 10,
            string? search = null,
            string? type = null,
            string? status = null,
            string? date = null)
        {
            DateTime? parsedDate = null;
            if (!string.IsNullOrWhiteSpace(date))
            {
                if (DateTime.TryParse(date, out var d))
                {
                    parsedDate = d;
                }
            }

            var model = await _transactionService.GetFilteredPaymentsAsync(
                page, pageSize, search, type, status, parsedDate);

            return Json(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserByPhone(string phone)
        {
            var result = await _transactionService.GetUserByPhoneAsync(phone);
            if (result == null) return NotFound();
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _transactionService.GetUserByIdAsync(id);
            if (user == null) return Json(new { success = false });

            return Json(new { 
                success = true, 
                userId = user.UserId,
                fullName = user.FullName,
                phone = user.Phone,
                balance = user.WalletBalance
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeposit([FromBody] SportBookingSystem.DTO.CreateDepositDTO dto)
        {
            var result = await _transactionService.CreateDepositAsync(dto);
            return Json(new { success = result.Success, message = result.Message, transactionCode = result.TransactionCode });
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _transactionService.GetProductsAsync();
            return Json(products);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRewardSettings([FromBody] RewardSettingDTO dto)
        {
            if (dto == null) return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            try
            {
                await _transactionService.SaveRewardSettingsAsync(dto);
                return Json(new { success = true, message = "Lưu cấu hình thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRewardSetting()
        {
            var data = await _transactionService.GetCurrentRewardSettingsAsync();
            return Json(data);
        }

        [HttpPost]
        public IActionResult CreatePurchase([FromBody] CreatePurchaseDTO model)
        {
            try
            {
                if (model.Quantity <= 0)
                {
                    return Json(new { success = false, message = "Số lượng phải lớn hơn 0" });
                }

                // Temporary Mock response for purchase
                var orderCode = $"ORD-{DateTime.Now:yyyyMMddHHmmss}";
                
                return Json(new { 
                    success = true, 
                    message = "Tạo đơn hàng thành công (Mock)!", 
                    orderCode = orderCode 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderForFulfillment(int orderId)
        {
            var order = await _transactionService.GetOrderDetailsByIdAsync(orderId);
            if (order == null) return NotFound();

            // Check expiration (15 minutes) for Pending orders
            if (order.Status == "0" && DateTime.Now > order.OrderDate.AddMinutes(15))
            {
                 return Json(new { 
                     error = true, 
                     message = "Mã QR đã hết hạn (quá 15 phút). Vui lòng yêu cầu khách hàng đặt lại." 
                 });
            }

            return Json(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] FulfillmentRequestDTO request)
        {
            if (request == null) return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            var result = await _transactionService.UpdateOrderStatusAsync(request.OrderId, request.NewStatus);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}