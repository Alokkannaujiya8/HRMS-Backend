using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Constants;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class AttendanceReportingService : IAttendanceReportingService
    {
        private readonly HrmsDbContext _context;

        public AttendanceReportingService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<AttendanceReportRowDto>> GetAttendanceRowsAsync(
            DateTime fromDate,
            DateTime toDate,
            int? employeeId,
            double overtimeAfterHours,
            TimeSpan lateCutoff,
            decimal standardMonthlyHours)
        {
            var attendances = await _context.Attendances
                .Include(x => x.Employee)
                .Where(x => x.AttendanceDate >= fromDate.Date && x.AttendanceDate <= toDate.Date)
                .Where(x => !employeeId.HasValue || x.EmployeeId == employeeId.Value)
                .OrderBy(x => x.AttendanceDate)
                .ThenBy(x => x.Employee != null ? x.Employee.Name : string.Empty)
                .ToListAsync();

            var employeeIds = attendances.Select(x => x.EmployeeId).Distinct().ToList();
            var managedSalaries = await _context.SalaryManagements
                .Where(x => employeeIds.Contains(x.EmployeeId))
                .Select(x => new { x.EmployeeId, x.TotalSalary })
                .ToDictionaryAsync(x => x.EmployeeId, x => x.TotalSalary);

            var holidaySet = await GetHolidaySetAsync(fromDate.Date, toDate.Date);

            return attendances
                .Select(x =>
                {
                    var monthlySalary = managedSalaries.TryGetValue(x.EmployeeId, out var totalSalary)
                        ? totalSalary
                        : x.Employee?.Salary ?? 0m;

                    return CreateAttendanceReportRow(x, holidaySet, overtimeAfterHours, lateCutoff, monthlySalary, standardMonthlyHours);
                })
                .ToList();
        }

        public async Task<AttendanceSummaryResponse> GetTodaySummaryAsync(
            DateTime date,
            double overtimeAfterHours,
            TimeSpan lateCutoff,
            decimal standardMonthlyHours)
        {
            var reportDate = date.Date;
            var rows = await GetAttendanceRowsAsync(reportDate, reportDate, null, overtimeAfterHours, lateCutoff, standardMonthlyHours);

            return new AttendanceSummaryResponse
            {
                Date = reportDate,
                TotalEmployees = await _context.Employees.CountAsync(x => x.IsActive),
                PresentEmployees = rows.Count(x => string.Equals(x.Status, AttendanceStatuses.Present, StringComparison.OrdinalIgnoreCase)),
                AbsentEmployees = rows.Count(x => string.Equals(x.Status, AttendanceStatuses.Absent, StringComparison.OrdinalIgnoreCase)),
                LateEmployees = rows.Count(x => x.IsLate),
                PendingLeaveRequests = await _context.LeaveRequests.CountAsync(x => x.Status == LeaveStatuses.Pending),
                TotalWorkingHours = Math.Round(rows.Sum(x => x.WorkingHours), 2),
                TotalOtHours = Math.Round(rows.Sum(x => x.TotalOvertimeHours), 2),
                RegularOtHours = Math.Round(rows.Sum(x => x.RegularOvertimeHours), 2),
                WeekendOtHours = Math.Round(rows.Sum(x => x.WeekendOvertimeHours), 2),
                HolidayDoubleOtHours = Math.Round(rows.Sum(x => x.HolidayDoubleOvertimeHours), 2),
                TotalOtAmount = Math.Round(rows.Sum(x => x.OvertimeAmount), 2),
                Employees = rows
            };
        }

        public async Task<OvertimeReportResponse> GetOvertimeReportAsync(
            DateTime fromDate,
            DateTime toDate,
            int? employeeId,
            double overtimeAfterHours,
            decimal standardMonthlyHours)
        {
            var rows = await GetAttendanceRowsAsync(
                fromDate.Date,
                toDate.Date,
                employeeId,
                overtimeAfterHours,
                new TimeSpan(9, 15, 0),
                standardMonthlyHours);

            return new OvertimeReportResponse
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                OvertimeAfterHours = overtimeAfterHours,
                TotalOtHours = Math.Round(rows.Sum(x => x.TotalOvertimeHours), 2),
                RegularOtHours = Math.Round(rows.Sum(x => x.RegularOvertimeHours), 2),
                WeekendOtHours = Math.Round(rows.Sum(x => x.WeekendOvertimeHours), 2),
                HolidayDoubleOtHours = Math.Round(rows.Sum(x => x.HolidayDoubleOvertimeHours), 2),
                TotalOtAmount = Math.Round(rows.Sum(x => x.OvertimeAmount), 2),
                Employees = rows.Where(x => x.TotalOvertimeHours > 0).ToList()
            };
        }

        public async Task<MonthlyAttendanceReportResponse> GetMonthlyReportAsync(
            int year,
            int month,
            int? employeeId,
            double overtimeAfterHours,
            TimeSpan lateCutoff,
            decimal standardMonthlyHours)
        {
            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1).AddDays(-1);
            var rows = await GetAttendanceRowsAsync(from, to, employeeId, overtimeAfterHours, lateCutoff, standardMonthlyHours);

            var leaves = await _context.LeaveRequests
                .Include(x => x.Employee)
                .Where(x => x.FromDate.Date <= to && x.ToDate.Date >= from)
                .Where(x => !employeeId.HasValue || x.EmployeeId == employeeId.Value)
                .ToListAsync();

            var employeeGroups = rows
                .GroupBy(x => new { x.EmployeeId, x.EmployeeName })
                .Select(group =>
                {
                    var employeeLeaves = leaves.Where(x => x.EmployeeId == group.Key.EmployeeId).ToList();

                    return new MonthlyEmployeeAttendanceSummaryDto
                    {
                        EmployeeId = group.Key.EmployeeId,
                        EmployeeName = group.Key.EmployeeName,
                        PresentDays = group.Count(x => string.Equals(x.Status, AttendanceStatuses.Present, StringComparison.OrdinalIgnoreCase)),
                        AbsentDays = group.Count(x => string.Equals(x.Status, AttendanceStatuses.Absent, StringComparison.OrdinalIgnoreCase)),
                        LateDays = group.Count(x => x.IsLate),
                        WorkingHours = Math.Round(group.Sum(x => x.WorkingHours), 2),
                        MonthlySalary = group.Max(x => x.MonthlySalary),
                        HourlySalary = group.Max(x => x.HourlySalary),
                        TotalOtHours = Math.Round(group.Sum(x => x.TotalOvertimeHours), 2),
                        RegularOtHours = Math.Round(group.Sum(x => x.RegularOvertimeHours), 2),
                        WeekendOtHours = Math.Round(group.Sum(x => x.WeekendOvertimeHours), 2),
                        HolidayDoubleOtHours = Math.Round(group.Sum(x => x.HolidayDoubleOvertimeHours), 2),
                        OvertimeAmount = Math.Round(group.Sum(x => x.OvertimeAmount), 2),
                        TotalSalaryIncludingOvertime = Math.Round(group.Max(x => x.MonthlySalary) + group.Sum(x => x.OvertimeAmount), 2),
                        ApprovedLeaveDays = employeeLeaves
                            .Where(x => string.Equals(x.Status, LeaveStatuses.Approved, StringComparison.OrdinalIgnoreCase))
                            .Sum(x => CountOverlappingDays(x.FromDate.Date, x.ToDate.Date, from, to)),
                        PendingLeaveRequests = employeeLeaves.Count(x => string.Equals(x.Status, LeaveStatuses.Pending, StringComparison.OrdinalIgnoreCase))
                    };
                })
                .OrderBy(x => x.EmployeeName)
                .ToList();

            return new MonthlyAttendanceReportResponse
            {
                Year = year,
                Month = month,
                FromDate = from,
                ToDate = to,
                OvertimeAfterHours = overtimeAfterHours,
                StandardMonthlyHours = standardMonthlyHours,
                LateAfter = lateCutoff.ToString(@"hh\:mm"),
                TotalWorkingHours = Math.Round(rows.Sum(x => x.WorkingHours), 2),
                TotalOtHours = Math.Round(rows.Sum(x => x.TotalOvertimeHours), 2),
                TotalOtAmount = Math.Round(rows.Sum(x => x.OvertimeAmount), 2),
                TotalSalaryIncludingOvertime = Math.Round(employeeGroups.Sum(x => x.TotalSalaryIncludingOvertime), 2),
                TotalLateDays = rows.Count(x => x.IsLate),
                TotalPendingLeaveRequests = leaves.Count(x => string.Equals(x.Status, LeaveStatuses.Pending, StringComparison.OrdinalIgnoreCase)),
                Employees = employeeGroups,
                DailyRecords = rows
            };
        }

        public async Task<PayrollOvertimeDashboardResponse> GetPayrollOvertimeDashboardAsync(
            int year,
            int month,
            double overtimeAfterHours,
            decimal standardMonthlyHours)
        {
            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1).AddDays(-1);
            var rows = await GetAttendanceRowsAsync(from, to, null, overtimeAfterHours, new TimeSpan(9, 15, 0), standardMonthlyHours);

            var employees = await _context.Employees
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            var activeEmployeeIds = employees.Select(x => x.Id).ToList();
            var salaries = await _context.SalaryManagements
                .Where(x => activeEmployeeIds.Contains(x.EmployeeId))
                .Select(x => new { x.EmployeeId, x.TotalSalary })
                .ToDictionaryAsync(x => x.EmployeeId, x => x.TotalSalary);

            var rowGroups = rows
                .GroupBy(x => x.EmployeeId)
                .ToDictionary(x => x.Key, x => x.ToList());

            var dashboardRows = employees.Select(employee =>
            {
                var monthlySalary = salaries.TryGetValue(employee.Id, out var managedSalary)
                    ? managedSalary
                    : employee.Salary;
                var hourlySalary = standardMonthlyHours > 0 ? monthlySalary / standardMonthlyHours : 0m;
                var employeeRows = rowGroups.TryGetValue(employee.Id, out var values)
                    ? values
                    : new List<AttendanceReportRowDto>();
                var overtimeAmount = employeeRows.Sum(x => x.OvertimeAmount);

                return new PayrollOvertimeEmployeeDto
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.Name,
                    MonthlySalary = Math.Round(monthlySalary, 2),
                    StandardMonthlyHours = standardMonthlyHours,
                    HourlySalary = Math.Round(hourlySalary, 2),
                    PresentDays = employeeRows.Count(x => string.Equals(x.Status, AttendanceStatuses.Present, StringComparison.OrdinalIgnoreCase)),
                    WorkingHours = Math.Round(employeeRows.Sum(x => x.WorkingHours), 2),
                    RegularOtHours = Math.Round(employeeRows.Sum(x => x.RegularOvertimeHours), 2),
                    WeekendOtHours = Math.Round(employeeRows.Sum(x => x.WeekendOvertimeHours), 2),
                    HolidayDoubleOtHours = Math.Round(employeeRows.Sum(x => x.HolidayDoubleOvertimeHours), 2),
                    TotalOtHours = Math.Round(employeeRows.Sum(x => x.TotalOvertimeHours), 2),
                    OvertimeAmount = Math.Round(overtimeAmount, 2),
                    TotalSalaryIncludingOvertime = Math.Round(monthlySalary + overtimeAmount, 2)
                };
            }).ToList();

            return new PayrollOvertimeDashboardResponse
            {
                Year = year,
                Month = month,
                FromDate = from,
                ToDate = to,
                OvertimeAfterHours = overtimeAfterHours,
                StandardMonthlyHours = standardMonthlyHours,
                TotalMonthlySalary = Math.Round(dashboardRows.Sum(x => x.MonthlySalary), 2),
                TotalOtHours = Math.Round(dashboardRows.Sum(x => x.TotalOtHours), 2),
                TotalOtAmount = Math.Round(dashboardRows.Sum(x => x.OvertimeAmount), 2),
                TotalSalaryIncludingOvertime = Math.Round(dashboardRows.Sum(x => x.TotalSalaryIncludingOvertime), 2),
                Employees = dashboardRows
            };
        }

        private async Task<HashSet<DateTime>> GetHolidaySetAsync(DateTime fromDate, DateTime toDate)
        {
            var holidays = await _context.Holidays
                .Where(x => x.IsActive && x.HolidayDate >= fromDate && x.HolidayDate <= toDate)
                .Select(x => x.HolidayDate.Date)
                .ToListAsync();

            return holidays.ToHashSet();
        }

        private static AttendanceReportRowDto CreateAttendanceReportRow(
            Attendance attendance,
            HashSet<DateTime> holidayDates,
            double overtimeAfterHours,
            TimeSpan lateCutoff,
            decimal monthlySalary,
            decimal standardMonthlyHours)
        {
            var workingHours = GetWorkingHours(attendance);
            var attendanceDate = attendance.AttendanceDate.Date;
            var isWeekend = attendanceDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var isHoliday = holidayDates.Contains(attendanceDate);
            var isLate = attendance.CheckInTime.HasValue && attendance.CheckInTime.Value.TimeOfDay > lateCutoff;

            var regularOtHours = !isWeekend && !isHoliday
                ? Math.Max(0, workingHours - overtimeAfterHours)
                : 0;
            var weekendOtHours = isWeekend && !isHoliday ? workingHours : 0;
            var holidayDoubleOtHours = isHoliday ? workingHours * 2 : 0;
            var totalOtHours = regularOtHours + weekendOtHours + holidayDoubleOtHours;
            var hourlySalary = standardMonthlyHours > 0 ? monthlySalary / standardMonthlyHours : 0m;
            var overtimeAmount = hourlySalary * (decimal)totalOtHours;

            return new AttendanceReportRowDto
            {
                Id = attendance.Id,
                EmployeeId = attendance.EmployeeId,
                EmployeeName = attendance.Employee?.Name,
                AttendanceDate = attendanceDate,
                CheckInTime = attendance.CheckInTime,
                CheckOutTime = attendance.CheckOutTime,
                Status = attendance.Status,
                WorkingHours = Math.Round(workingHours, 2),
                IsLate = isLate,
                LateAfter = lateCutoff.ToString(@"hh\:mm"),
                IsWeekend = isWeekend,
                IsHoliday = isHoliday,
                MonthlySalary = Math.Round(monthlySalary, 2),
                StandardMonthlyHours = standardMonthlyHours,
                HourlySalary = Math.Round(hourlySalary, 2),
                RegularOvertimeHours = Math.Round(regularOtHours, 2),
                WeekendOvertimeHours = Math.Round(weekendOtHours, 2),
                HolidayDoubleOvertimeHours = Math.Round(holidayDoubleOtHours, 2),
                TotalOvertimeHours = Math.Round(totalOtHours, 2),
                OvertimeAmount = Math.Round(overtimeAmount, 2)
            };
        }

        private static double GetWorkingHours(Attendance attendance)
        {
            return attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue
                ? Math.Max(0, (attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours)
                : 0;
        }

        private static int CountOverlappingDays(DateTime start, DateTime end, DateTime rangeStart, DateTime rangeEnd)
        {
            var overlapStart = start > rangeStart ? start : rangeStart;
            var overlapEnd = end < rangeEnd ? end : rangeEnd;

            return overlapStart > overlapEnd ? 0 : (overlapEnd - overlapStart).Days + 1;
        }
    }
}
