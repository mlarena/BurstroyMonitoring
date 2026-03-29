using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Api.Services;
using BurstroyMonitoring.Api.Models;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace BurstroyMonitoring.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context, 
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<AuthResponse>
                    {
                        Success = false,
                        Message = "Invalid request",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);

                if (user == null || !_authService.VerifyPassword(request.Password, user.PasswordHash, user.Salt))
                {
                    return Unauthorized(new ApiResponse<AuthResponse>
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    });
                }

                var token = _authService.GenerateJwtToken(user);

                return Ok(new ApiResponse<AuthResponse>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = new AuthResponse
                    {
                        Token = token,
                        UserName = user.UserName,
                        Role = user.Role,
                        ExpiresAt = DateTime.UtcNow.AddDays(1)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {UserName}", request.UserName);
                return StatusCode(500, new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<AuthResponse>
                    {
                        Success = false,
                        Message = "Invalid request",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.UserName == request.UserName))
                {
                    return BadRequest(new ApiResponse<AuthResponse>
                    {
                        Success = false,
                        Message = "Username already exists"
                    });
                }

                // Validate role
                var validRoles = new[] { "User", "Admin", "Manager" };
                var role = string.IsNullOrEmpty(request.Role) ? "User" : request.Role;
                if (!validRoles.Contains(role))
                {
                    role = "User";
                }

                // Create new user
                var (hash, salt) = _authService.HashPassword(request.Password);
                var user = new User
                {
                    UserName = request.UserName,
                    PasswordHash = hash,
                    Salt = salt,
                    Role = role,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate token for the new user
                var token = _authService.GenerateJwtToken(user);

                return Ok(new ApiResponse<AuthResponse>
                {
                    Success = true,
                    Message = "Registration successful",
                    Data = new AuthResponse
                    {
                        Token = token,
                        UserName = user.UserName,
                        Role = user.Role,
                        ExpiresAt = DateTime.UtcNow.AddDays(1)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {UserName}", request.UserName);
                return StatusCode(500, new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public ActionResult<ApiResponse<object>> Logout()
        {
            // In JWT, logout is typically handled client-side by removing the token
            // But we can add additional logic like token blacklisting if needed
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Logout successful"
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var user = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Role, u.CreatedAt })
                    .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

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
                    Message = "User data retrieved",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public ActionResult<ApiResponse<bool>> ValidateToken([FromHeader(Name = "Authorization")] string authorization)
        {
            try
            {
                if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                {
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "No token provided",
                        Data = false
                    });
                }

                var token = authorization.Substring("Bearer ".Length).Trim();
                var principal = _authService.ValidateToken(token);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = principal != null ? "Token is valid" : "Token is invalid",
                    Data = principal != null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Error validating token",
                    Data = false
                });
            }
        }
    }
}