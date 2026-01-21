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
            PaymentDashboardDTO model;

        
            bool hasFilters = !string.IsNullOrWhiteSpace(search) ||
                            !string.IsNullOrWhiteSpace(type) && type != "all" ||
                            !string.IsNullOrWhiteSpace(status) && status != "all" ||
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

        [HttpPost]
        public async Task<IActionResult> CreateDeposit([FromBody] CreateDepositDTO dto)
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
        //hàm lưu cấu hình điểm thưởng
        public IActionResult CreateDeposit([FromBody] CreateDepositDTO model)
        {
            try
            {
                if (model.Amount <= 0)
                {
                    return Json(new { success = false, message = "Số tiền phải lớn hơn 0" });
                }

                // TODO: Replace with actual database operations
                // 1. Create Transaction record
                // 2. Update User WalletBalance
                
                // Mock success response
                var transactionCode = $"DEP-{DateTime.Now:yyyyMMddHHmmss}";
                
                return Json(new { 
                    success = true, 
                    message = "Tạo giao dịch nạp tiền thành công!", 
                    transactionCode = transactionCode 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: Create Purchase Order
        [HttpPost]
        public async Task<IActionResult> SaveRewardSettings([FromBody] RewardSettingDTO dto)
        public IActionResult CreatePurchase([FromBody] CreatePurchaseDTO model)
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

        //hàm load lên modal cấu hình điểm thưởng 
        [HttpGet]
        public  async Task<IActionResult> GetRewardSetting()
        {
          var data =   await _transactionService.GetCurrentRewardSettingsAsync();
            return Json(data);
        } 
    }
}