namespace HRMS.Application.DTOs
{
    public class LeaveApplyResponse
    {
        public string Message { get; set; } = string.Empty;
        public int LeaveRequestId { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class LeaveBalanceResponse
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public int TotalLeaves { get; set; }
        public int UsedLeaves { get; set; }
        public int RemainingLeaves { get; set; }
    }

    public class LeaveListItemResponse
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedOn { get; set; }
        public string? ActionBy { get; set; }
        public DateTime? ActionOn { get; set; }
        public string? RejectionReason { get; set; }
    }
}
