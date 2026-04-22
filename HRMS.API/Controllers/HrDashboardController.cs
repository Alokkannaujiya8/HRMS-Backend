using HRMS.API.Authorization;
using HRMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,HR")]
    public class HrDashboardController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public HrDashboardController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> GetSummary()
        {
            var totalEmployees = await _context.Employees.CountAsync(e => e.IsActive);
            var totalDepartments = await _context.Departments.CountAsync();
            var totalPendingLeaves = await _context.LeaveRequests.CountAsync(l => l.Status == "Pending");
            var totalPayrollRecords = await _context.Payrolls.CountAsync();

            return Ok(new
            {
                TotalEmployees = totalEmployees,
                TotalDepartments = totalDepartments,
                TotalPendingLeaves = totalPendingLeaves,
                TotalPayrollRecords = totalPayrollRecords
            });
        }
    }
}
