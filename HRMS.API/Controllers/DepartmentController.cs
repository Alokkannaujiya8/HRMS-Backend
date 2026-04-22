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
    public class DepartmentController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public DepartmentController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments.ToListAsync();
            return Ok(departments);
        }

        [Authorize(Roles = "Admin,HR")]
        [HasPermission("CanManageDepartments")]
        [HttpPost]
        public async Task<IActionResult> AddDepartment(Department dept)
        {
            if (string.IsNullOrWhiteSpace(dept.Code))
            {
                return BadRequest("Department code is required.");
            }

            var normalizedCode = dept.Code.Trim().ToUpperInvariant();

            var exists = await _context.Departments.AnyAsync(x => x.Code == normalizedCode);
            if (exists)
            {
                return BadRequest($"Department code '{normalizedCode}' already exists.");
            }

            dept.Code = normalizedCode;
            dept.Name = dept.Name?.Trim();
            dept.CreatedDate = DateTime.UtcNow;

            var userName = User?.Identity?.Name ?? "System";
            await _context.Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key=N'AppUser', @value={0}", userName);

            try
            {
                await _context.Departments.AddAsync(dept);
                await _context.SaveChangesAsync();
            }
            finally
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key=N'AppUser', @value=NULL");
            }

            return Ok(dept);
        }
    }
}
