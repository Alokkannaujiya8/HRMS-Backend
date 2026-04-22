

namespace HRMS.Domain.Entities
{
    public class AppUser
    {
        public int Id { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? Role { get; set; }

        public int? EmployeeId { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        public ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}
