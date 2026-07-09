using HRMS.API.Authorization;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using HRMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Authorize]
    public class PayrollController : ApiControllerBase
    {
        private readonly IPayrollManagementService _payrollManagementService;
        private readonly IAttendanceReportingService _attendanceReportingService;

        public PayrollController(
            IPayrollManagementService payrollManagementService,
            IAttendanceReportingService attendanceReportingService)
        {
            _payrollManagementService = payrollManagementService;
            _attendanceReportingService = attendanceReportingService;
        }

        [HttpGet]
        [HasPermission("CanViewSalary")]
        public async Task<IActionResult> GetPayrolls()
        {
            return Ok(await _payrollManagementService.GetPayrollsAsync());
        }

        [HttpGet("overtime-dashboard")]
        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanViewSalary")]
        public async Task<IActionResult> GetOvertimePayrollDashboard(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] double overtimeAfterHours = 8,
            [FromQuery] decimal standardMonthlyHours = 208)
        {
            if (year < 2000 || month is < 1 or > 12)
            {
                return BadRequest("Valid year and month are required.");
            }

            var response = await _attendanceReportingService.GetPayrollOvertimeDashboardAsync(
                year,
                month,
                overtimeAfterHours,
                standardMonthlyHours);

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanViewSalary")]
        public async Task<IActionResult> AddPayroll(Payroll payroll)
        {
            return Ok(await _payrollManagementService.AddPayrollAsync(payroll));
        }
    }
}
