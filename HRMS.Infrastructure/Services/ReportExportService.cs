using ClosedXML.Excel;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HRMS.Infrastructure.Services
{
    public class ReportExportService : IReportExportService
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private readonly IAttendanceReportingService _attendanceReportingService;
        private readonly IAuditReportService _auditReportService;

        public ReportExportService(
            IAttendanceReportingService attendanceReportingService,
            IAuditReportService auditReportService)
        {
            _attendanceReportingService = attendanceReportingService;
            _auditReportService = auditReportService;
        }

        public async Task<ReportFileResponse> ExportAttendanceExcelAsync(DateTime from, DateTime to, int? employeeId, double overtimeAfterHours, TimeSpan lateCutoff, decimal standardMonthlyHours)
        {
            var records = await _attendanceReportingService.GetAttendanceRowsAsync(from, to, employeeId, overtimeAfterHours, lateCutoff, standardMonthlyHours);

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Attendance");
            var headers = new[]
            {
                "Employee ID", "Employee Name", "Date", "Check In", "Check Out", "Status", "Working Hours",
                "Late", "Weekend", "Holiday", "Regular OT Hours", "Weekend OT Hours", "Holiday Double OT Hours",
                "Total OT Hours", "Monthly Salary", "Hourly Salary", "OT Amount"
            };

            for (var i = 0; i < headers.Length; i++)
            {
                sheet.Cell(1, i + 1).Value = headers[i];
            }

            for (var i = 0; i < records.Count; i++)
            {
                var row = i + 2;
                var item = records[i];
                sheet.Cell(row, 1).Value = item.EmployeeId;
                sheet.Cell(row, 2).Value = item.EmployeeName ?? string.Empty;
                sheet.Cell(row, 3).Value = item.AttendanceDate.ToString("yyyy-MM-dd");
                sheet.Cell(row, 4).Value = item.CheckInTime?.ToString("HH:mm") ?? "-";
                sheet.Cell(row, 5).Value = item.CheckOutTime?.ToString("HH:mm") ?? "-";
                sheet.Cell(row, 6).Value = item.Status;
                sheet.Cell(row, 7).Value = item.WorkingHours;
                sheet.Cell(row, 8).Value = item.IsLate ? "Yes" : "No";
                sheet.Cell(row, 9).Value = item.IsWeekend ? "Yes" : "No";
                sheet.Cell(row, 10).Value = item.IsHoliday ? "Yes" : "No";
                sheet.Cell(row, 11).Value = item.RegularOvertimeHours;
                sheet.Cell(row, 12).Value = item.WeekendOvertimeHours;
                sheet.Cell(row, 13).Value = item.HolidayDoubleOvertimeHours;
                sheet.Cell(row, 14).Value = item.TotalOvertimeHours;
                sheet.Cell(row, 15).Value = item.MonthlySalary;
                sheet.Cell(row, 16).Value = item.HourlySalary;
                sheet.Cell(row, 17).Value = item.OvertimeAmount;
            }

            sheet.Columns().AdjustToContents();
            return CreateExcelResponse(workbook, $"attendance-report-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
        }

        public async Task<ReportFileResponse> ExportAttendancePdfAsync(DateTime from, DateTime to, int? employeeId, double overtimeAfterHours, TimeSpan lateCutoff, decimal standardMonthlyHours)
        {
            var records = await _attendanceReportingService.GetAttendanceRowsAsync(from, to, employeeId, overtimeAfterHours, lateCutoff, standardMonthlyHours);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4.Landscape());
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header().Text($"Attendance Report - {DateTime.UtcNow:dd-MMM-yyyy HH:mm}").SemiBold().FontSize(14);
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
                            columns.RelativeColumn(0.7f);
                            columns.RelativeColumn(0.7f);
                            columns.RelativeColumn(0.7f);
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            foreach (var text in new[] { "Emp ID", "Name", "Date", "Check In", "Check Out", "Status", "Hours", "Late", "Wknd", "Hol", "OT", "OT Amt" })
                            {
                                header.Cell().Element(CellStyle).Text(text);
                            }
                        });

                        foreach (var item in records)
                        {
                            table.Cell().Element(CellStyle).Text(item.EmployeeId.ToString());
                            table.Cell().Element(CellStyle).Text(item.EmployeeName ?? string.Empty);
                            table.Cell().Element(CellStyle).Text(item.AttendanceDate.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(item.CheckInTime?.ToString("HH:mm") ?? "-");
                            table.Cell().Element(CellStyle).Text(item.CheckOutTime?.ToString("HH:mm") ?? "-");
                            table.Cell().Element(CellStyle).Text(item.Status);
                            table.Cell().Element(CellStyle).Text(item.WorkingHours.ToString("0.##"));
                            table.Cell().Element(CellStyle).Text(item.IsLate ? "Y" : "N");
                            table.Cell().Element(CellStyle).Text(item.IsWeekend ? "Y" : "N");
                            table.Cell().Element(CellStyle).Text(item.IsHoliday ? "Y" : "N");
                            table.Cell().Element(CellStyle).Text(item.TotalOvertimeHours.ToString("0.##"));
                            table.Cell().Element(CellStyle).Text(item.OvertimeAmount.ToString("0.00"));
                        }
                    });
                });
            }).GeneratePdf();

            return new ReportFileResponse { Content = pdfBytes, ContentType = "application/pdf", FileName = $"attendance-report-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf" };
        }

        public async Task<ReportFileResponse> ExportAuditExcelAsync(DateTime? fromDate, DateTime? toDate)
        {
            var records = await _auditReportService.GetAuditRecordsAsync(fromDate, toDate);

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("AuditTrail");
            var headers = new[] { "Changed At (UTC)", "Changed By", "Table", "Action", "Record Id", "Old Values", "New Values" };
            for (var i = 0; i < headers.Length; i++)
            {
                sheet.Cell(1, i + 1).Value = headers[i];
            }

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
            return CreateExcelResponse(workbook, $"audit-report-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
        }

        public async Task<ReportFileResponse> ExportAuditPdfAsync(DateTime? fromDate, DateTime? toDate)
        {
            var records = await _auditReportService.GetAuditRecordsAsync(fromDate, toDate);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4.Landscape());
                    page.DefaultTextStyle(x => x.FontSize(9));
                    page.Header().Text($"Audit Trail Report - {DateTime.UtcNow:dd-MMM-yyyy HH:mm}").SemiBold().FontSize(14);
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
                            foreach (var text in new[] { "Changed At", "Changed By", "Table", "Action", "Record", "Old Values", "New Values" })
                            {
                                header.Cell().Element(CellStyle).Text(text);
                            }
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

            return new ReportFileResponse { Content = pdfBytes, ContentType = "application/pdf", FileName = $"audit-report-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf" };
        }

        private static ReportFileResponse CreateExcelResponse(XLWorkbook workbook, string fileName)
        {
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return new ReportFileResponse
            {
                Content = stream.ToArray(),
                ContentType = ExcelContentType,
                FileName = fileName
            };
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
