namespace HRMS.Domain.Entities
{
    /// <summary>
    /// Application User entity with security metadata, lockout, 2FA, and token tracking.
    /// </summary>
    public class AppUser
    {
        public int Id { get; set; }

        public string? Username { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? Role { get; set; }

        public int? EmployeeId { get; set; }

        public bool IsEmailVerified { get; set; }

        public string? EmailVerificationToken { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public string? TwoFactorSecret { get; set; }

        public int FailedLoginAttempts { get; set; }

        public DateTime? LockoutEnd { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpiry { get; set; }

        public DateTime? LastPasswordChangeDate { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        public ICollection<RefreshToken>? RefreshTokens { get; set; }

        public ICollection<PasswordHistory>? PasswordHistories { get; set; }
    }
}
