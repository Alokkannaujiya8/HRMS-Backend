using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface IAuditReportService
    {
        Task<(int Total, List<AuditTrail> Data)> GetAuditTrailAsync(int page, int pageSize);

        Task<List<AuditTrail>> GetAuditRecordsAsync(DateTime? fromDate, DateTime? toDate);
    }
}
