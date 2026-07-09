namespace HRMS.Application.DTOs
{
    public class AttendanceListItemResponse
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime AttendanceDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class AttendanceCheckInResponse
    {
        public string Message { get; set; } = string.Empty;
        public int Id { get; set; }
        public DateTime? CheckInTime { get; set; }
    }

    public class AttendanceCheckOutResponse
    {
        public string Message { get; set; } = string.Empty;
        public int Id { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public double TotalHours { get; set; }
    }
}
