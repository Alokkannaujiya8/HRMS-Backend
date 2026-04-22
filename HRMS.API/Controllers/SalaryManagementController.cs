using HRMS.API.Authorization;
using HRMS.Application.DTOs;
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
    public class SalaryManagementController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public SalaryManagementController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [HasPermission("CanViewSalary")]
        public async Task<IActionResult> Upsert([FromBody] SalaryManagementRequest request)
        {
            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == request.EmployeeId && e.IsActive);
            if (!employeeExists)
            {
                return BadRequest("Employee not found.");
            }

            var incrementAmount = Math.Round(request.BasicSalary * request.IncrementPercent / 100m, 2);
            var totalSalary = request.BasicSalary + incrementAmount + request.SpecialAllowance - request.SpecialDeduction;

            var existing = await _context.SalaryManagements.FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId);
            if (existing == null)
            {
                existing = new SalaryManagement
                {
                    EmployeeId = request.EmployeeId
                };
                await _context.SalaryManagements.AddAsync(existing);
            }

            existing.BasicSalary = request.BasicSalary;
            existing.IncrementPercent = request.IncrementPercent;
            existing.IncrementAmount = incrementAmount;
            existing.EffectiveDate = request.EffectiveDate;
            existing.SpecialAllowance = request.SpecialAllowance;
            existing.SpecialDeduction = request.SpecialDeduction;
            existing.TotalSalary = totalSalary;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Salary details saved successfully.",
                existing.BasicSalary,
                existing.IncrementPercent,
                existing.IncrementAmount,
                existing.EffectiveDate,
                existing.TotalSalary,
                existing.SpecialAllowance,
                existing.SpecialDeduction
            });
        }

        [HttpGet("{employeeId:int}")]
        [HasPermission("CanViewSalary")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var salary = await _context.SalaryManagements.FirstOrDefaultAsync(x => x.EmployeeId == employeeId);
            if (salary == null)
            {
                return Ok(new { Message = "No Record Found" });
            }

            return Ok(salary);
        }
    }
}
