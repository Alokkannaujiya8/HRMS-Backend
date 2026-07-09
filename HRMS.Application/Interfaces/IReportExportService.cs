using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces
{
    public interface IReportExportService
    {
        Task<ReportFileResponse> ExportAttendanceExcelAsync(DateTime from, DateTime to, int? employeeId, double overtimeAfterHours, TimeSpan lateCutoff, decimal standardMonthlyHours);
        Task<ReportFileResponse> ExportAttendancePdfAsync(DateTime from, DateTime to, int? employeeId, double overtimeAfterHours, TimeSpan lateCutoff, decimal standardMonthlyHours);
        Task<ReportFileResponse> ExportAuditExcelAsync(DateTime? fromDate, DateTime? toDate);
        Task<ReportFileResponse> ExportAuditPdfAsync(DateTime? fromDate, DateTime? toDate);
    }
}
