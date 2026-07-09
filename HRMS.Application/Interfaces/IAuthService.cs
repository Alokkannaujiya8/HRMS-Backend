using HRMS.Application.DTOs;


namespace HRMS.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<AuthResponse> ChangePasswordAsync(string username, ChangePasswordRequest request);
    }
}
