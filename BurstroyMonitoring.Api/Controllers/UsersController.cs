using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using BurstroyMonitoring.Api.Models;
using BurstroyMonitoring.Api.Services;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace BurstroyMonitoring.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly IAuthService _authService;

        public UsersController(
            ApplicationDbContext context,
            ILogger<UsersController> logger,
            IAuthService authService)
        {
            _context = context;
            _logger = logger;
            _authService = authService;
        }

        /// <summary>
        /// Получить список всех пользователей (только для администраторов)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Role,
                        u.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving users"
                });
            }
        }

        /// <summary>
        /// Получить пользователя по ID (только для администраторов)
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<object>>> GetUserById(int id)
        {
            try
            {
                var user = await _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Role,
                        u.CreatedAt
                    })
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user"
                });
            }
        }

        /// <summary>
        /// Обновить роль пользователя (только для администраторов)
        /// </summary>
        [HttpPut("{id:int}/role")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateUserRole(int id, [FromBody] UpdateRoleRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                var validRoles = new[] { "User", "Admin", "Manager" };
                if (!validRoles.Contains(request.Role))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid role. Valid roles: User, Admin, Manager"
                    });
                }

                user.Role = request.Role;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "User role updated successfully",
                    Data = new { user.Id, user.UserName, user.Role }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating user role"
                });
            }
        }

        /// <summary>
        /// Удалить пользователя (только для администраторов)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                // Нельзя удалить самого себя
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim, out int currentUserId) && currentUserId == id)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "You cannot delete your own account"
                    });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "User deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting user"
                });
            }
        }

        /// <summary>
        /// Изменить пароль текущего пользователя
        /// </summary>
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                // Проверяем текущий пароль
                if (!_authService.VerifyPassword(request.CurrentPassword, user.PasswordHash, user.Salt))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Current password is incorrect"
                    });
                }

                // Хешируем новый пароль
                var (newHash, newSalt) = _authService.HashPassword(request.NewPassword);
                user.PasswordHash = newHash;
                user.Salt = newSalt;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Password changed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while changing password"
                });
            }
        }
    }

    public class UpdateRoleRequest
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}