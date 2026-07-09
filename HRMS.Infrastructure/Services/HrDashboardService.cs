using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Constants;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class HrDashboardService : IHrDashboardService
    {
        private readonly HrmsDbContext _context;

        public HrDashboardService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<HrDashboardSummaryResponse> GetSummaryAsync()
        {
            return new HrDashboardSummaryResponse
            {
                TotalEmployees = await _context.Employees.CountAsync(e => e.IsActive),
                TotalDepartments = await _context.Departments.CountAsync(),
                TotalPendingLeaves = await _context.LeaveRequests.CountAsync(l => l.Status == LeaveStatuses.Pending),
                TotalPayrollRecords = await _context.Payrolls.CountAsync()
            };
        }
    }
}
