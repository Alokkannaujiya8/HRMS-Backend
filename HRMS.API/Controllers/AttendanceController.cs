using HRMS.API.Authorization;
using HRMS.Application.Features.Attendance.Commands.CheckIn;
using HRMS.Application.Features.Attendance.Commands.CheckOut;
using HRMS.Application.Features.Attendance.Queries.GetAllAttendance;
using HRMS.Application.Features.Attendance.Queries.GetEmployeeAttendance;
using HRMS.Application.Features.Attendance.Queries.GetMonthlyAttendanceReport;
using HRMS.Application.Features.Attendance.Queries.GetOvertimeReport;
using HRMS.Application.Features.Attendance.Queries.GetTodaySummary;
using HRMS.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    /// <summary>
    /// API Controller managing Attendance operations using CQRS pattern with MediatR.
    /// Strictly contains zero business logic; delegates execution to Command and Query handlers.
    /// </summary>
    [Authorize]
    public class AttendanceController : ApiControllerBase
    {
        private readonly ISender _mediator;

        public AttendanceController(ISender mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Records check-in for the logged-in employee.
        /// </summary>
        [Authorize(Roles = AppRoles.Employee)]
        [HttpPost("check-in")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckIn(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CheckInCommand(User), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Records check-out for the logged-in employee.
        /// </summary>
        [Authorize(Roles = AppRoles.Employee)]
        [HttpPost("check-out")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckOut(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CheckOutCommand(User), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Retrieves attendance history for the logged-in employee.
        /// </summary>
        [Authorize(Roles = AppRoles.Employee)]
        [HttpGet("my")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMyAttendance(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetEmployeeAttendanceQuery(User, fromDate, toDate), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Retrieves all employee attendance records with optional filtering.
        /// </summary>
        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanEditAttendance")]
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAttendance(
            [FromQuery] int? employeeId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllAttendanceQuery(employeeId, fromDate, toDate), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Retrieves today's attendance summary report.
        /// </summary>
        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanEditAttendance")]
        [HttpGet("today-summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTodaySummary(
            [FromQuery] DateTime? date,
            [FromQuery] double overtimeAfterHours = 8,
            [FromQuery] decimal standardMonthlyHours = 208,
            [FromQuery] string lateAfter = "09:15",
            CancellationToken cancellationToken = default)
        {
            var query = new GetTodaySummaryQuery(date, overtimeAfterHours, standardMonthlyHours, lateAfter);
            var result = await _mediator.Send(query, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Retrieves overtime report across date range.
        /// </summary>
        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanEditAttendance")]
        [HttpGet("overtime")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetOvertime(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? employeeId,
            [FromQuery] double overtimeAfterHours = 8,
            [FromQuery] decimal standardMonthlyHours = 208,
            CancellationToken cancellationToken = default)
        {
            var query = new GetOvertimeReportQuery(fromDate, toDate, employeeId, overtimeAfterHours, standardMonthlyHours);
            var result = await _mediator.Send(query, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Retrieves monthly attendance breakdown report.
        /// </summary>
        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanEditAttendance")]
        [HttpGet("monthly-report")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMonthlyReport(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int? employeeId,
            [FromQuery] double overtimeAfterHours = 8,
            [FromQuery] decimal standardMonthlyHours = 208,
            [FromQuery] string lateAfter = "09:15",
            CancellationToken cancellationToken = default)
        {
            var query = new GetMonthlyAttendanceReportQuery(year, month, employeeId, overtimeAfterHours, standardMonthlyHours, lateAfter);
            var result = await _mediator.Send(query, cancellationToken);
            return ToActionResult(result);
        }
    }
}
