using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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

        
        [HttpPost]
        public async Task<IActionResult> AddDepartment(Department dept)
        {
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();
            return Ok(dept); 
        }
    }
}