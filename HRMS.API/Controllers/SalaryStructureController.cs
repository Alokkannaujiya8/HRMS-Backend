using HRMS.API.Authorization;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,HR")]
    public class SalaryStructureController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public SalaryStructureController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [HasPermission("CanViewSalary")]
        public async Task<IActionResult> AddOrUpdate(SalaryStructure request)
        {
            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == request.EmployeeId && e.IsActive);
            if (!employeeExists)
            {
                return BadRequest("Employee not found.");
            }

            var existing = await _context.SalaryStructures.FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId);
            if (existing == null)
            {
                request.CreatedAt = DateTime.UtcNow;
                await _context.SalaryStructures.AddAsync(request);
            }
            else
            {
                existing.Base = request.Base;
                existing.HRA = request.HRA;
                existing.DA = request.DA;
                existing.PFDeductions = request.PFDeductions;
                existing.Tax = request.Tax;
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Salary structure saved successfully." });
        }

        [HttpGet("{employeeId:int}")]
        [HasPermission("CanViewSalary")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var structure = await _context.SalaryStructures.FirstOrDefaultAsync(x => x.EmployeeId == employeeId);
            if (structure == null)
            {
                return NotFound("Salary structure not found.");
            }

            return Ok(structure);
        }
    }
}
