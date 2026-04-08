using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new AppUser
            {
                Username = request.Username,
                Password = passwordHash,
                Role = string.IsNullOrWhiteSpace(request.Role) ? "Employee" : request.Role
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

            var accessToken = _jwtService.GenerateToken(user.Username, user.Role);

            var refreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Role = user.Role,
                Message = "Login Successful"
            };
        }
    }
}