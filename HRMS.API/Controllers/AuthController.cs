using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);

            if (response.Message == "Username already exists!" || response.Message == "Employee mapping is invalid.")
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);

            if (response.Message == "Invalid Username or Password")
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request);

            if (response.Message is "Invalid refresh token." or "Refresh token expired.")
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            var response = await _authService.ChangePasswordAsync(username ?? string.Empty, request);

            if (response.Message is "User not found."
                or "New password must be at least 6 characters."
                or "New password and confirm password do not match."
                or "Current password is incorrect.")
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
