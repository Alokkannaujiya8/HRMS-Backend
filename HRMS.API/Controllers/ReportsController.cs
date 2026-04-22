using ClosedXML.Excel;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,HR")]
    public class ReportsController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public ReportsController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet("attendance/excel")]
        public async Task<IActionResult> ExportAttendanceExcel([FromQuery] int? employeeId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var records = await GetAttendanceRecords(employeeId, fromDate, toDate);

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Attendance");

            sheet.Cell(1, 1).Value = "Employee ID";
            sheet.Cell(1, 2).Value = "Employee Name";
            sheet.Cell(1, 3).Value = "Date";
            sheet.Cell(1, 4).Value = "Check In";
            sheet.Cell(1, 5).Value = "Check Out";
            sheet.Cell(1, 6).Value = "Status";
            sheet.Cell(1, 7).Value = "Working Hours";

            for (var i = 0; i < records.Count; i++)
            {
                var row = i + 2;
                var item = records[i];
                var hours = item.CheckInTime.HasValue && item.CheckOutTime.HasValue
                    ? Math.Round((item.CheckOutTime.Value - item.CheckInTime.Value).TotalHours, 2)
                    : 0;

                sheet.Cell(row, 1).Value = item.EmployeeId;
                sheet.Cell(row, 2).Value = item.Employee?.Name ?? string.Empty;
                sheet.Cell(row, 3).Value = item.AttendanceDate.ToString("yyyy-MM-dd");
                sheet.Cell(row, 4).Value = item.CheckInTime?.ToString("HH:mm") ?? "-";
                sheet.Cell(row, 5).Value = item.CheckOutTime?.ToString("HH:mm") ?? "-";
                sheet.Cell(row, 6).Value = item.Status;
                sheet.Cell(row, 7).Value = hours;
            }

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"attendance-report-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
        }

        [HttpGet("attendance/pdf")]
        public async Task<IActionResult> ExportAttendancePdf([FromQuery] int? employeeId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var records = await GetAttendanceRecords(employeeId, fromDate, toDate);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4.Landscape());
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text($"Attendance Report - {DateTime.UtcNow:dd-MMM-yyyy HH:mm}")
                        .SemiBold()
                        .FontSize(14);

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Emp ID");
                            header.Cell().Element(CellStyle).Text("Name");
                            header.Cell().Element(CellStyle).Text("Date");
                            header.Cell().Element(CellStyle).Text("Check In");
                            header.Cell().Element(CellStyle).Text("Check Out");
                            header.Cell().Element(CellStyle).Text("Status");
                            header.Cell().Element(CellStyle).Text("Hours");
                        });

                        foreach (var item in records)
                        {
                            var hours = item.CheckInTime.HasValue && item.CheckOutTime.HasValue
                                ? Math.Round((item.CheckOutTime.Value - item.CheckInTime.Value).TotalHours, 2)
                                : 0;

                            table.Cell().Element(CellStyle).Text(item.EmployeeId.ToString());
                            table.Cell().Element(CellStyle).Text(item.Employee?.Name ?? string.Empty);
                            table.Cell().Element(CellStyle).Text(item.AttendanceDate.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(item.CheckInTime?.ToString("HH:mm") ?? "-");
                            table.Cell().Element(CellStyle).Text(item.CheckOutTime?.ToString("HH:mm") ?? "-");
                            table.Cell().Element(CellStyle).Text(item.Status);
                            table.Cell().Element(CellStyle).Text(hours.ToString("0.##"));
                        }
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"attendance-report-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }

        [HttpGet("audit")]
        public async Task<IActionResult> GetAuditTrail([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 500 ? 50 : pageSize;

            var query = _context.AuditTrails.AsQueryable();
            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.ChangedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { page, pageSize, total, data });
        }

        [HttpGet("audit/excel")]
        public async Task<IActionResult> ExportAuditExcel([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var records = await GetAuditRecords(fromDate, toDate);

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("AuditTrail");

            sheet.Cell(1, 1).Value = "Changed At (UTC)";
            sheet.Cell(1, 2).Value = "Changed By";
            sheet.Cell(1, 3).Value = "Table";
            sheet.Cell(1, 4).Value = "Action";
            sheet.Cell(1, 5).Value = "Record Id";
            sheet.Cell(1, 6).Value = "Old Values";
            sheet.Cell(1, 7).Value = "New Values";

            for (var i = 0; i < records.Count; i++)
            {
                var row = i + 2;
                var item = records[i];
                sheet.Cell(row, 1).Value = item.ChangedAt.ToString("yyyy-MM-dd HH:mm:ss");
                sheet.Cell(row, 2).Value = item.ChangedBy ?? "System";
                sheet.Cell(row, 3).Value = item.TableName;
                sheet.Cell(row, 4).Value = item.ActionType;
                sheet.Cell(row, 5).Value = item.RecordId ?? "-";
                sheet.Cell(row, 6).Value = item.OldValues ?? "-";
                sheet.Cell(row, 7).Value = item.NewValues ?? "-";
            }

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"audit-report-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
        }

        [HttpGet("audit/pdf")]
        public async Task<IActionResult> ExportAuditPdf([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var records = await GetAuditRecords(fromDate, toDate);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4.Landscape());
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header()
                        .Text($"Audit Trail Report - {DateTime.UtcNow:dd-MMM-yyyy HH:mm}")
                        .SemiBold()
                        .FontSize(14);

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Changed At");
                            header.Cell().Element(CellStyle).Text("Changed By");
                            header.Cell().Element(CellStyle).Text("Table");
                            header.Cell().Element(CellStyle).Text("Action");
                            header.Cell().Element(CellStyle).Text("Record");
                            header.Cell().Element(CellStyle).Text("Old Values");
                            header.Cell().Element(CellStyle).Text("New Values");
                        });

                        foreach (var item in records)
                        {
                            table.Cell().Element(CellStyle).Text(item.ChangedAt.ToString("yyyy-MM-dd HH:mm"));
                            table.Cell().Element(CellStyle).Text(item.ChangedBy ?? "System");
                            table.Cell().Element(CellStyle).Text(item.TableName);
                            table.Cell().Element(CellStyle).Text(item.ActionType);
                            table.Cell().Element(CellStyle).Text(item.RecordId ?? "-");
                            table.Cell().Element(CellStyle).Text(item.OldValues ?? "-");
                            table.Cell().Element(CellStyle).Text(item.NewValues ?? "-");
                        }
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"audit-report-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }

        private async Task<List<Attendance>> GetAttendanceRecords(int? employeeId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Attendances
                .Include(x => x.Employee)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(x => x.EmployeeId == employeeId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.AttendanceDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.AttendanceDate <= toDate.Value.Date);
            }

            return await query
                .OrderBy(x => x.AttendanceDate)
                .ThenBy(x => x.EmployeeId)
                .ToListAsync();
        }

        private async Task<List<AuditTrail>> GetAuditRecords(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.AuditTrails.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.ChangedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.ChangedAt <= toDate.Value);
            }

            return await query
                .OrderByDescending(x => x.ChangedAt)
                .ToListAsync();
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(4);
        }
    }
}
