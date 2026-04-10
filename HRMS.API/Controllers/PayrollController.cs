using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayrollController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public PayrollController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayrolls()
        {
            var payrolls = await _context.Payrolls.ToListAsync();
            return Ok(payrolls);
        }

        [HttpPost]
        public async Task<IActionResult> AddPayroll(Payroll payroll)
        {
            await _context.Payrolls.AddAsync(payroll);
            await _context.SaveChangesAsync();
            return Ok(payroll);
        }
    }
}
