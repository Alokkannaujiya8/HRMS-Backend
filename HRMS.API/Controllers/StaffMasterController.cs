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
    public class StaffMasterController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public StaffMasterController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpPost("onboard")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> Onboard([FromBody] StaffOnboardRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Name and Email are required.");
            }

            var normalizedEmail = request.Email.Trim();
            var duplicateEmail = await _context.Employees.AnyAsync(e => e.Email == normalizedEmail && e.IsActive);
            if (duplicateEmail)
            {
                return BadRequest("An active employee already exists with this email.");
            }

            var employee = new Employee
            {
                Name = request.Name.Trim(),
                Email = normalizedEmail,
                Mobile = request.Mobile?.Trim(),
                Salary = request.Salary,
                DepartmentId = request.DepartmentId,
                JoinDate = request.JoinDate.GetValueOrDefault(DateTime.UtcNow),
                Address = request.Address?.Trim(),
                Designation = request.Designation?.Trim(),
                Division = request.Division?.Trim(),
                Pan = request.Pan?.Trim(),
                Dob = request.Dob,
                Gender = request.Gender?.Trim(),
                EmploymentStatus = request.EmploymentStatus?.Trim(),
                EmploymentType = request.EmploymentType?.Trim(),
                IsSeventhPayCommission = request.IsSeventhPayCommission,
                BankAccountNumber = request.BankAccountNumber?.Trim(),
                IfscCode = request.IfscCode?.Trim()
            };

            employee.IsActive = true;

            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Employee onboarded successfully.", employee.Id });
        }
    }
}
