using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Services;
using System.Security.Claims;

namespace SportBookingSystem.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/transaction")]
    public class TransactionAPIController : ControllerBase
    {
        private readonly ITransactionUserService _transactionUserService;

        public TransactionAPIController(ITransactionUserService transactionUserService)
        {
            _transactionUserService = transactionUserService;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetUserTransactions()
        {
            // Lấy userId từ Claims (user đang đăng nhập)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập" });
            }

            var transactions = await _transactionUserService.LoadUserTransactionAsync(userId);
            return Ok(transactions);
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetUserBookings()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập" });
            }

            var bookings = await _transactionUserService.LoadUserBookingsAsync(userId);
            return Ok(bookings);
        }
        // API kiểm tra người nhận
        [HttpGet("check-receiver")]
        public async Task<IActionResult> CheckReceiver(string phone)
        {
            var user = await _transactionUserService.GetUserByPhoneAsync(phone);

            if (user != null)
                return Ok(new { success = true, name = user.FullName });
            else
                return NotFound(new { success = false, message = "Không tìm thấy người dùng" });
        }

        // API chuyển tiền
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferDTO dto)
        {
            // Lấy userId từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
            }

            // Gọi service
            string result = await _transactionUserService.TransferMoneyAsync(userId, dto);

            if (result == "success")
                return Ok(new { success = true, message = "Chuyển tiền thành công" });
            else
                return BadRequest(new { success = false, message = result });
        }
    }
}
