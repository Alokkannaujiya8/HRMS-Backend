using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HRMS.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly HrmsDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(HrmsDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var trimmedUsername = (request.Username ?? string.Empty).Trim();
            if (await _context.Users.AnyAsync(u => u.Username != null && u.Username.ToLower() == trimmedUsername.ToLower()))
            {
                return new AuthResponse { Message = "Username already exists!" };
            }

            if (request.EmployeeId.HasValue)
            {
                var employeeExists = await _context.Employees.AnyAsync(e => e.Id == request.EmployeeId.Value && e.IsActive);
                if (!employeeExists)
                {
                    return new AuthResponse { Message = "Employee mapping is invalid." };
                }
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new AppUser
            {
                Username = trimmedUsername,
                Password = passwordHash,
                Role = string.IsNullOrWhiteSpace(request.Role) ? AppRoles.Employee : request.Role,
                EmployeeId = request.EmployeeId
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return new AuthResponse { Message = "User registered successfully!" };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var trimmedUsername = (request.Username ?? string.Empty).Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == trimmedUsername.ToLower());

            if (user == null || string.IsNullOrWhiteSpace(user.Password) || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return new AuthResponse { Message = "Invalid Username or Password" };
            }

            return await CreateTokenResponseAsync(user, "Login Successful");
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return new AuthResponse { Message = "Invalid refresh token." };
            }

            var incomingHash = ComputeHash(request.RefreshToken);

            var existingToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == incomingHash);

            if (existingToken == null || existingToken.User == null || existingToken.IsRevoked)
            {
                return new AuthResponse { Message = "Invalid refresh token." };
            }

            if (existingToken.ExpiresAt <= DateTime.UtcNow)
            {
                existingToken.IsRevoked = true;
                existingToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new AuthResponse { Message = "Refresh token expired." };
            }

            existingToken.IsRevoked = true;
            existingToken.RevokedAt = DateTime.UtcNow;

            return await CreateTokenResponseAsync(existingToken.User, "Token refreshed successfully.");
        }

        public async Task<AuthResponse> ChangePasswordAsync(string username, ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new AuthResponse { Message = "User not found." };
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            {
                return new AuthResponse { Message = "New password must be at least 6 characters." };
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return new AuthResponse { Message = "New password and confirm password do not match." };
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == username.Trim().ToLower());
            if (user == null || string.IsNullOrWhiteSpace(user.Password))
            {
                return new AuthResponse { Message = "User not found." };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
            {
                return new AuthResponse { Message = "Current password is incorrect." };
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            var activeRefreshTokens = await _context.RefreshTokens
                .Where(x => x.UserId == user.Id && !x.IsRevoked)
                .ToListAsync();

            foreach (var refreshToken in activeRefreshTokens)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new AuthResponse { Message = "Password changed successfully." };
        }

        private async Task<AuthResponse> CreateTokenResponseAsync(AppUser user, string message)
        {
            var role = user.Role ?? AppRoles.Employee;
            var permissions = RolePermissionStore.GetPermissionsForRole(role);

            var accessToken = _jwtService.GenerateToken(
                user.Username ?? string.Empty,
                role,
                user.EmployeeId,
                permissions);

            var refreshTokenPlain = GenerateSecureToken();
            var refreshTokenHash = ComputeHash(refreshTokenPlain);

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _context.RefreshTokens.AddAsync(refreshTokenEntity);

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshTokenPlain,
                Role = role,
                Permissions = permissions.ToList(),
                Message = message
            };
        }

        private static string GenerateSecureToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        private static string ComputeHash(string rawValue)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawValue));
            return Convert.ToHexString(bytes);
        }
    }
}
