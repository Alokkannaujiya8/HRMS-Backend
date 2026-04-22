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
    public class UserController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public UserController(HrmsDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> AddUser(AppUser user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }
    }
}
