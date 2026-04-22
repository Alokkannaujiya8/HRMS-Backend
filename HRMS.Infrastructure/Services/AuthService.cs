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
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
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
                Username = request.Username,
                Password = passwordHash,
                Role = string.IsNullOrWhiteSpace(request.Role) ? "Employee" : request.Role,
                EmployeeId = request.EmployeeId
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return new AuthResponse { Message = "User registered successfully!" };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
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

        private async Task<AuthResponse> CreateTokenResponseAsync(AppUser user, string message)
        {
            var role = user.Role ?? "Employee";
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
