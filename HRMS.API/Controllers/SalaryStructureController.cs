using HRMS.API.Authorization;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using HRMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Authorize(Roles = AppRoles.AdminOrHr)]
    public class SalaryStructureController : ApiControllerBase
    {
        private readonly ISalaryStructureService _salaryStructureService;

        public SalaryStructureController(ISalaryStructureService salaryStructureService)
        {
            _salaryStructureService = salaryStructureService;
        }

        [HttpPost]
        [HasPermission("CanViewSalary")]
        public async Task<IActionResult> AddOrUpdate(SalaryStructure request)
        {
            await _salaryStructureService.AddOrUpdateAsync(request);
            return Ok(new { Message = "Salary structure saved successfully." });
        }

        [HttpGet("{employeeId:int}")]
        [HasPermission("CanViewSalary")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var structure = await _salaryStructureService.GetByEmployeeAsync(employeeId);
            return structure == null ? NotFound("Salary structure not found.") : Ok(structure);
        }
    }
}
