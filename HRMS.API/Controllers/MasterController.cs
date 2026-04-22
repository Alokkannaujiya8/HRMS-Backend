using HRMS.API.Authorization;
using HRMS.Application.DTOs;
using HRMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,HR")]
    public class MasterController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public MasterController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet("staff-masters")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> GetStaffMasters()
        {
            var divisions = await _context.Employees
                .Where(e => e.IsActive && e.Division != null && e.Division != "")
                .Select(e => e.Division!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var designations = await _context.Employees
                .Where(e => e.IsActive && e.Designation != null && e.Designation != "")
                .Select(e => e.Designation!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var departments = await _context.Departments
                .AsNoTracking()
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentMasterItem
                {
                    Id = d.Id,
                    Name = d.Name ?? string.Empty,
                    Code = d.Code ?? string.Empty
                })
                .ToListAsync();

            return Ok(new MasterDataResponse
            {
                Divisions = divisions,
                Designations = designations,
                Departments = departments
            });
        }
    }
}
