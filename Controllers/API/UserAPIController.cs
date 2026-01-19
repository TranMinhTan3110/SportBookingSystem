using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Services;

namespace SportBookingSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user")]  
    public class UserADApiController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserADApiController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("getCount")]
        public async Task<IActionResult> GetCountUser()
        {
            var data = await _userService.countUserAsync();
            return Ok(data);
        }

        [HttpGet("allUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var data = await _userService.getAllUserAsync();
            return Ok(data);
        }
        [HttpPost("toggleStatus/{userId}")]
        public async Task<IActionResult> ToggleStatus(string userId)
        {
            var result = await _userService.ToggleUserStatusAsync(userId);
            if (result)
            {
                return Ok(new { message = "Cập nhật trạng thái thành công " });
            }
            return BadRequest(new { message = "Không tìm thấy người dùng" });
        }
    }
}