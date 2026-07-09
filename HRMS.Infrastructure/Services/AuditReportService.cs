using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class AuditReportService : IAuditReportService
    {
        private readonly HrmsDbContext _context;

        public AuditReportService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<(int Total, List<AuditTrail> Data)> GetAuditTrailAsync(int page, int pageSize)
        {
            var query = _context.AuditTrails.AsQueryable();
            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(x => x.ChangedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (total, data);
        }

        public async Task<List<AuditTrail>> GetAuditRecordsAsync(DateTime? fromDate, DateTime? toDate)
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
    }
}
