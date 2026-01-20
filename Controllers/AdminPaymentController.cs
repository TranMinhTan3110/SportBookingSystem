using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.DTO;
using SportBookingSystem.Services;

namespace SportBookingSystem.Controllers
{
    public class AdminPaymentController : Controller
    {
        private readonly ITransactionService _transactionService;

        public AdminPaymentController(ITransactionService ts) => _transactionService = ts;

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var model = await _transactionService.GetPaymentDashboardAsync(page, pageSize);
            return View(model);
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
    }
}