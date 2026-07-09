using HRMS.Domain.Constants;

namespace HRMS.Domain.Entities
{
    public class Attendance
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public DateTime AttendanceDate { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public string Status { get; set; } = AttendanceStatuses.Present;
    }
}
