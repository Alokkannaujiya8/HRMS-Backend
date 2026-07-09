using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces
{
    public interface IAttendanceReportingService
    {
        Task<List<AttendanceReportRowDto>> GetAttendanceRowsAsync(
            DateTime fromDate,
            DateTime toDate,
            int? employeeId,
            double overtimeAfterHours,
            TimeSpan lateCutoff,
            decimal standardMonthlyHours);

        Task<AttendanceSummaryResponse> GetTodaySummaryAsync(
            DateTime date,
            double overtimeAfterHours,
            TimeSpan lateCutoff,
            decimal standardMonthlyHours);

        Task<OvertimeReportResponse> GetOvertimeReportAsync(
            DateTime fromDate,
            DateTime toDate,
            int? employeeId,
            double overtimeAfterHours,
            decimal standardMonthlyHours);

        Task<MonthlyAttendanceReportResponse> GetMonthlyReportAsync(
            int year,
            int month,
            int? employeeId,
            double overtimeAfterHours,
            TimeSpan lateCutoff,
            decimal standardMonthlyHours);

        Task<PayrollOvertimeDashboardResponse> GetPayrollOvertimeDashboardAsync(
            int year,
            int month,
            double overtimeAfterHours,
            decimal standardMonthlyHours);
    }
}
