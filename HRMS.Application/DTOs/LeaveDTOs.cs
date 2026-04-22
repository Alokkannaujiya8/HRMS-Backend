namespace HRMS.Application.DTOs
{
    public class ApplyLeaveRequest
    {
        public string LeaveType { get; set; } = "Casual";

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string? Reason { get; set; }
    }

    public class LeaveActionRequest
    {
        public string? Comment { get; set; }
    }
}
