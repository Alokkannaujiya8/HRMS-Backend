using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly HrmsDbContext _context;
        private readonly JwtService _jwt;

        public AuthController(HrmsDbContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        [HttpPost("login")]
        public IActionResult Login(string username, string password)
        {

            if (username == "admin" && password == "1234")
            {
                var token = _jwt.GenerateToken(username, "Admin");

                return Ok(new { token });
            }

            return Unauthorized();
        }
        
    }
}
