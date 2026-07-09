using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Authorize(Roles = AppRoles.AdminOrHr)]
    public class ReportsController : ApiControllerBase
    {
        private readonly IAuditReportService _auditReportService;
        private readonly IReportExportService _reportExportService;

        public ReportsController(
            IAuditReportService auditReportService,
            IReportExportService reportExportService)
        {
            _auditReportService = auditReportService;
            _reportExportService = reportExportService;
        }

        [HttpGet("attendance/excel")]
        public async Task<IActionResult> ExportAttendanceExcel(
            [FromQuery] int? employeeId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] double overtimeAfterHours = 8,
            [FromQuery] decimal standardMonthlyHours = 208,
            [FromQuery] string lateAfter = "09:15")
        {
            var (from, to) = ResolveDateRange(fromDate, toDate);
            var file = await _reportExportService.ExportAttendanceExcelAsync(
                from,
                to,
                employeeId,
                overtimeAfterHours,
                ParseTime(lateAfter, new TimeSpan(9, 15, 0)),
                standardMonthlyHours);

            return File(file.Content, file.ContentType, file.FileName);
        }

        [HttpGet("attendance/pdf")]
        public async Task<IActionResult> ExportAttendancePdf(
            [FromQuery] int? employeeId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] double overtimeAfterHours = 8,
            [FromQuery] decimal standardMonthlyHours = 208,
            [FromQuery] string lateAfter = "09:15")
        {
            var (from, to) = ResolveDateRange(fromDate, toDate);
            var file = await _reportExportService.ExportAttendancePdfAsync(
                from,
                to,
                employeeId,
                overtimeAfterHours,
                ParseTime(lateAfter, new TimeSpan(9, 15, 0)),
                standardMonthlyHours);

            return File(file.Content, file.ContentType, file.FileName);
        }

        [HttpGet("audit")]
        public async Task<IActionResult> GetAuditTrail([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 500 ? 50 : pageSize;

            var (total, data) = await _auditReportService.GetAuditTrailAsync(page, pageSize);
            return Ok(new { page, pageSize, total, data });
        }

        [HttpGet("audit/excel")]
        public async Task<IActionResult> ExportAuditExcel([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var file = await _reportExportService.ExportAuditExcelAsync(fromDate, toDate);
            return File(file.Content, file.ContentType, file.FileName);
        }

        [HttpGet("audit/pdf")]
        public async Task<IActionResult> ExportAuditPdf([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var file = await _reportExportService.ExportAuditPdfAsync(fromDate, toDate);
            return File(file.Content, file.ContentType, file.FileName);
        }

        private static (DateTime From, DateTime To) ResolveDateRange(DateTime? fromDate, DateTime? toDate)
        {
            var to = (toDate ?? DateTime.UtcNow).Date;
            var from = (fromDate ?? to).Date;
            if (from > to)
            {
                throw new InvalidOperationException("FromDate cannot be after ToDate.");
            }

            return (from, to);
        }

        private static TimeSpan ParseTime(string value, TimeSpan fallback)
        {
            return TimeSpan.TryParse(value, out var time) ? time : fallback;
        }
    }
}
