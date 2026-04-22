namespace HRMS.Domain.Entities
{
    public class AuditTrail
    {
        public int Id { get; set; }

        public string TableName { get; set; } = string.Empty;

        public string ActionType { get; set; } = string.Empty;

        public string? RecordId { get; set; }

        public string? ChangedBy { get; set; }

        public DateTime ChangedAt { get; set; }

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }
    }
}
