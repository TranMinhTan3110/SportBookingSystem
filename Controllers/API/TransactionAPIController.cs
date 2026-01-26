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
        public async Task<IActionResult> GetUserTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập" });
            }

            var (data, totalRecords) = await _transactionUserService.LoadUserTransactionAsync(userId, page, pageSize);

            return Ok(new
            {
                data = data,
                currentPage = page,
                pageSize = pageSize,
                totalRecords = totalRecords,
                totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            });
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetUserBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập" });
            }

            var (data, totalRecords) = await _transactionUserService.LoadUserBookingsAsync(userId, page, pageSize);

            return Ok(new
            {
                data = data,
                currentPage = page,
                pageSize = pageSize,
                totalRecords = totalRecords,
                totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            });
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
        // API lịch sử chuyển tiền
        [HttpGet("transfers")]
        public async Task<IActionResult> GetUserTransfers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập" });
            }

            var (data, totalRecords) = await _transactionUserService.LoadUserTransfersAsync(userId, page, pageSize);

            return Ok(new
            {
                data = data,
                currentPage = page,
                pageSize = pageSize,
                totalRecords = totalRecords,
                totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            });
        }
    }
}
