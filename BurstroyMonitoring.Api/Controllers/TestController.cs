using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BurstroyMonitoring.Api.Models;
using System.Security.Claims;

namespace BurstroyMonitoring.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // Все методы этого контроллера требуют авторизации
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Публичный эндпоинт для проверки работы API (не требует авторизации)
        /// </summary>
        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult Public()
        {
            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "API is working",
                Data = "This is a public endpoint"
            });
        }

        /// <summary>
        /// Эндпоинт для проверки авторизации (требует валидный JWT токен)
        /// </summary>
        [HttpGet("secure")]
        public IActionResult Secure()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "You are authorized!",
                Data = new
                {
                    UserId = userId,
                    UserName = userName,
                    Role = userRole,
                    Message = "This is a protected endpoint"
                }
            });
        }

        /// <summary>
        /// Эндпоинт для администраторов
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult AdminOnly()
        {
            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "You are an admin!",
                Data = "This endpoint is only accessible by administrators"
            });
        }

        /// <summary>
        /// Эндпоинт с задержкой для тестирования
        /// </summary>
        [HttpGet("delay/{seconds:int}")]
        public async Task<IActionResult> Delay(int seconds)
        {
            if (seconds > 10)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Delay cannot exceed 10 seconds"
                });
            }

            await Task.Delay(seconds * 1000);
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Delayed for {seconds} seconds",
                Data = new { DelaySeconds = seconds, Timestamp = DateTime.UtcNow }
            });
        }
    }
}