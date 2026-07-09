namespace HRMS.Application.DTOs
{
    public class AttendanceReportRowDto
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public string? EmployeeName { get; set; }

        public DateTime AttendanceDate { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public string Status { get; set; } = string.Empty;

        public double WorkingHours { get; set; }

        public bool IsLate { get; set; }

        public string LateAfter { get; set; } = string.Empty;

        public bool IsWeekend { get; set; }

        public bool IsHoliday { get; set; }

        public decimal MonthlySalary { get; set; }

        public decimal StandardMonthlyHours { get; set; }

        public decimal HourlySalary { get; set; }

        public double RegularOvertimeHours { get; set; }

        public double WeekendOvertimeHours { get; set; }

        public double HolidayDoubleOvertimeHours { get; set; }

        public double TotalOvertimeHours { get; set; }

        public decimal OvertimeAmount { get; set; }
    }

    public class AttendanceSummaryResponse
    {
        public DateTime Date { get; set; }

        public int TotalEmployees { get; set; }

        public int PresentEmployees { get; set; }

        public int AbsentEmployees { get; set; }

        public int LateEmployees { get; set; }

        public int PendingLeaveRequests { get; set; }

        public double TotalWorkingHours { get; set; }

        public double TotalOtHours { get; set; }

        public double RegularOtHours { get; set; }

        public double WeekendOtHours { get; set; }

        public double HolidayDoubleOtHours { get; set; }

        public decimal TotalOtAmount { get; set; }

        public List<AttendanceReportRowDto> Employees { get; set; } = new();
    }

    public class OvertimeReportResponse
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public double OvertimeAfterHours { get; set; }

        public double TotalOtHours { get; set; }

        public double RegularOtHours { get; set; }

        public double WeekendOtHours { get; set; }

        public double HolidayDoubleOtHours { get; set; }

        public decimal TotalOtAmount { get; set; }

        public List<AttendanceReportRowDto> Employees { get; set; } = new();
    }

    public class MonthlyEmployeeAttendanceSummaryDto
    {
        public int EmployeeId { get; set; }

        public string? EmployeeName { get; set; }

        public int PresentDays { get; set; }

        public int AbsentDays { get; set; }

        public int LateDays { get; set; }

        public double WorkingHours { get; set; }

        public decimal MonthlySalary { get; set; }

        public decimal HourlySalary { get; set; }

        public double TotalOtHours { get; set; }

        public double RegularOtHours { get; set; }

        public double WeekendOtHours { get; set; }

        public double HolidayDoubleOtHours { get; set; }

        public decimal OvertimeAmount { get; set; }

        public decimal TotalSalaryIncludingOvertime { get; set; }

        public int ApprovedLeaveDays { get; set; }

        public int PendingLeaveRequests { get; set; }
    }

    public class MonthlyAttendanceReportResponse
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public double OvertimeAfterHours { get; set; }

        public decimal StandardMonthlyHours { get; set; }

        public string LateAfter { get; set; } = string.Empty;

        public double TotalWorkingHours { get; set; }

        public double TotalOtHours { get; set; }

        public decimal TotalOtAmount { get; set; }

        public decimal TotalSalaryIncludingOvertime { get; set; }

        public int TotalLateDays { get; set; }

        public int TotalPendingLeaveRequests { get; set; }

        public List<MonthlyEmployeeAttendanceSummaryDto> Employees { get; set; } = new();

        public List<AttendanceReportRowDto> DailyRecords { get; set; } = new();
    }

    public class PayrollOvertimeEmployeeDto
    {
        public int EmployeeId { get; set; }

        public string? EmployeeName { get; set; }

        public decimal MonthlySalary { get; set; }

        public decimal StandardMonthlyHours { get; set; }

        public decimal HourlySalary { get; set; }

        public int PresentDays { get; set; }

        public double WorkingHours { get; set; }

        public double RegularOtHours { get; set; }

        public double WeekendOtHours { get; set; }

        public double HolidayDoubleOtHours { get; set; }

        public double TotalOtHours { get; set; }

        public decimal OvertimeAmount { get; set; }

        public decimal TotalSalaryIncludingOvertime { get; set; }
    }

    public class PayrollOvertimeDashboardResponse
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public double OvertimeAfterHours { get; set; }

        public decimal StandardMonthlyHours { get; set; }

        public decimal TotalMonthlySalary { get; set; }

        public double TotalOtHours { get; set; }

        public decimal TotalOtAmount { get; set; }

        public decimal TotalSalaryIncludingOvertime { get; set; }

        public List<PayrollOvertimeEmployeeDto> Employees { get; set; } = new();
    }
}
