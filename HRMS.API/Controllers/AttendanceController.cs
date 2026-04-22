using HRMS.API.Authorization;
using HRMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public AttendanceController(HrmsDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Employee")]
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn()
        {
            var employeeId = await ResolveEmployeeIdAsync();
            if (employeeId == null)
            {
                return BadRequest("Employee mapping not found for logged-in user.");
            }

            var today = DateTime.UtcNow.Date;
            var existing = await _context.Attendances
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId.Value && x.AttendanceDate == today);

            if (existing != null && existing.CheckInTime.HasValue)
            {
                return BadRequest("Already checked in for today.");
            }

            if (existing == null)
            {
                existing = new Domain.Entities.Attendance
                {
                    EmployeeId = employeeId.Value,
                    AttendanceDate = today,
                    CheckInTime = DateTime.UtcNow,
                    Status = "Present"
                };

                await _context.Attendances.AddAsync(existing);
            }
            else
            {
                existing.CheckInTime = DateTime.UtcNow;
                existing.Status = "Present";
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Check-in successful.", existing.Id, existing.CheckInTime });
        }

        [Authorize(Roles = "Employee")]
        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut()
        {
            var employeeId = await ResolveEmployeeIdAsync();
            if (employeeId == null)
            {
                return BadRequest("Employee mapping not found for logged-in user.");
            }

            var today = DateTime.UtcNow.Date;
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId.Value && x.AttendanceDate == today);

            if (attendance == null || !attendance.CheckInTime.HasValue)
            {
                return BadRequest("Check-in not found for today.");
            }

            if (attendance.CheckOutTime.HasValue)
            {
                return BadRequest("Already checked out for today.");
            }

            attendance.CheckOutTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var totalHours = (attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
            return Ok(new
            {
                Message = "Check-out successful.",
                attendance.Id,
                attendance.CheckOutTime,
                TotalHours = Math.Round(totalHours, 2)
            });
        }

        [Authorize(Roles = "Employee")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAttendance([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var employeeId = await ResolveEmployeeIdAsync();
            if (employeeId == null)
            {
                return BadRequest("Employee mapping not found for logged-in user.");
            }

            var query = _context.Attendances
                .Include(x => x.Employee)
                .Where(x => x.EmployeeId == employeeId.Value)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.AttendanceDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.AttendanceDate <= toDate.Value.Date);
            }

            var data = await query
                .OrderByDescending(x => x.AttendanceDate)
                .Select(x => new
                {
                    x.Id,
                    x.EmployeeId,
                    EmployeeName = x.Employee != null ? x.Employee.Name : null,
                    x.AttendanceDate,
                    x.CheckInTime,
                    x.CheckOutTime,
                    x.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        [Authorize(Roles = "Admin,HR")]
        [HasPermission("CanEditAttendance")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAttendance([FromQuery] int? employeeId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var query = _context.Attendances
                .Include(x => x.Employee)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(x => x.EmployeeId == employeeId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.AttendanceDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.AttendanceDate <= toDate.Value.Date);
            }

            var data = await query
                .OrderByDescending(x => x.AttendanceDate)
                .Select(x => new
                {
                    x.Id,
                    x.EmployeeId,
                    EmployeeName = x.Employee != null ? x.Employee.Name : null,
                    x.AttendanceDate,
                    x.CheckInTime,
                    x.CheckOutTime,
                    x.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        private async Task<int?> ResolveEmployeeIdAsync()
        {
            var employeeIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(employeeIdClaim, out var employeeId))
            {
                return employeeId;
            }

            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return await _context.Employees
                .Where(x => x.IsActive && x.Email == username)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }
    }
}
