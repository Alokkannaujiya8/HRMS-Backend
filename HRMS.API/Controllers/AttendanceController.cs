using HRMS.API.Authorization;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMS.API.Controllers
{
    [Authorize]
    public class AttendanceController : ApiControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IAttendanceReportingService _attendanceReportingService;

        public AttendanceController(
            IAttendanceService attendanceService,
            IAttendanceReportingService attendanceReportingService)
        {
            _attendanceService = attendanceService;
            _attendanceReportingService = attendanceReportingService;
        }

        [Authorize(Roles = AppRoles.Employee)]
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn()
        {
            var employeeId = await ResolveEmployeeIdAsync();
            if (employeeId == null)
            {
                return BadRequest("Employee mapping not found for logged-in user.");
            }

            return Ok(await _attendanceService.CheckInAsync(employeeId.Value));
        }

        [Authorize(Roles = AppRoles.Employee)]
        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut()
        {
            var employeeId = await ResolveEmployeeIdAsync();
            if (employeeId == null)
            {
                return BadRequest("Employee mapping not found for logged-in user.");
            }

            return Ok(await _attendanceService.CheckOutAsync(employeeId.Value));
        }

        [Authorize(Roles = AppRoles.Employee)]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAttendance([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var employeeId = await ResolveEmployeeIdAsync();
            if (employeeId == null)
            {
                return BadRequest("Employee mapping not found for logged-in user.");
            }

            return Ok(await _attendanceService.GetEmployeeAttendanceAsync(employeeId.Value, fromDate, toDate));
        }

        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanEditAttendance")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAttendance([FromQuery] int? employeeId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            return Ok(await _attendanceService.GetAllAttendanceAsync(employeeId, fromDate, toDate));
        }

        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanEditAttendance")]
        [HttpGet("today-summary")]
        public async Task<IActionResult> GetTodaySummary(
            [FromQuery] DateTime? date,
            [FromQuery] double overtimeAfterHours = 8,
            [FromQuery] decimal standardMonthlyHours = 208,
            [FromQuery] string lateAfter = "09:15")
        {
            var reportDate = (date ?? DateTime.UtcNow).Date;
            var response = await _attendanceReportingService.GetTodaySummaryAsync(
                reportDate,
                overtimeAfterHours,
                ParseTime(lateAfter, new TimeSpan(9, 15, 0)),
                standardMonthlyHours);

            return Ok(response);
        }

        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanEditAttendance")]
        [HttpGet("late-employees")]
        public async Task<IActionResult> GetLateEmployees(
            [FromQuery] DateTime? date,
            [FromQuery] decimal standardMonthlyHours = 208,
            [FromQuery] string lateAfter = "09:15")
        {
            var reportDate = (date ?? DateTime.UtcNow).Date;
            var rows = await _attendanceReportingService.GetAttendanceRowsAsync(
                reportDate,
                reportDate,
                null,
                8,
                ParseTime(lateAfter, new TimeSpan(9, 15, 0)),
                standardMonthlyHours);

            return Ok(rows.Where(x => x.IsLate).ToList());
        }

        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanEditAttendance")]
        [HttpGet("overtime")]
        public async Task<IActionResult> GetOvertime(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? employeeId,
            [FromQuery] double overtimeAfterHours = 8,
            [FromQuery] decimal standardMonthlyHours = 208)
        {
            var to = (toDate ?? DateTime.UtcNow).Date;
            var from = (fromDate ?? to).Date;
            if (from > to)
            {
                return BadRequest("FromDate cannot be after ToDate.");
            }

            var response = await _attendanceReportingService.GetOvertimeReportAsync(
                from,
                to,
                employeeId,
                overtimeAfterHours,
                standardMonthlyHours);

            return Ok(response);
        }

        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanEditAttendance")]
        [HttpGet("monthly-report")]
        public async Task<IActionResult> GetMonthlyReport(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int? employeeId,
            [FromQuery] double overtimeAfterHours = 8,
            [FromQuery] decimal standardMonthlyHours = 208,
            [FromQuery] string lateAfter = "09:15")
        {
            if (year < 2000 || month is < 1 or > 12)
            {
                return BadRequest("Valid year and month are required.");
            }

            var response = await _attendanceReportingService.GetMonthlyReportAsync(
                year,
                month,
                employeeId,
                overtimeAfterHours,
                ParseTime(lateAfter, new TimeSpan(9, 15, 0)),
                standardMonthlyHours);

            return Ok(response);
        }

        private async Task<int?> ResolveEmployeeIdAsync()
        {
            return await _attendanceService.ResolveEmployeeIdAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                User.Identity?.Name);
        }

        private static TimeSpan ParseTime(string value, TimeSpan fallback)
        {
            return TimeSpan.TryParse(value, out var time) ? time : fallback;
        }
    }
}
