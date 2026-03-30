using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BurstroyMonitoring.Api.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
