using HRMS.API.Authorization;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Authorize(Roles = AppRoles.AdminOrHr)]
    public class SalaryManagementController : ApiControllerBase
    {
        private readonly ISalaryManagementService _salaryManagementService;

        public SalaryManagementController(ISalaryManagementService salaryManagementService)
        {
            _salaryManagementService = salaryManagementService;
        }

        [HttpPost]
        [HasPermission("CanViewSalary")]
        public async Task<IActionResult> Upsert([FromBody] SalaryManagementRequest request)
        {
            var salary = await _salaryManagementService.UpsertAsync(request);

            return Ok(new
            {
                Message = "Salary details saved successfully.",
                salary.BasicSalary,
                salary.IncrementPercent,
                salary.IncrementAmount,
                salary.EffectiveDate,
                salary.TotalSalary,
                salary.SpecialAllowance,
                salary.SpecialDeduction
            });
        }

        [HttpGet("{employeeId:int}")]
        [HasPermission("CanViewSalary")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var salary = await _salaryManagementService.GetByEmployeeAsync(employeeId);
            return salary == null ? Ok(new { Message = "No Record Found" }) : Ok(salary);
        }
    }
}
