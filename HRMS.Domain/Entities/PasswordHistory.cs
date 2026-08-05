namespace HRMS.Domain.Entities
{
    /// <summary>
    /// Entity tracking historical password hashes for users to prevent password reuse.
    /// </summary>
    public class PasswordHistory
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public AppUser? User { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
