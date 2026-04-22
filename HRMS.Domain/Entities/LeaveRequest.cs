namespace HRMS.Domain.Entities
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public string LeaveType { get; set; } = "Casual";

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string? Reason { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;

        public string? ActionBy { get; set; }

        public DateTime? ActionOn { get; set; }

        public string? RejectionReason { get; set; }
    }
}
