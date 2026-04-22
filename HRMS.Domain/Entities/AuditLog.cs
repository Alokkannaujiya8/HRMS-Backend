namespace HRMS.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int DepartmentId { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string PerformedBy { get; set; } = string.Empty;

        public DateTime PerformedAt { get; set; }
    }
}
