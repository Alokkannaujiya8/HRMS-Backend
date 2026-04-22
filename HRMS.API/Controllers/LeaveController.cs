using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.API.Authorization;
using HRMS.Domain.Entities;
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
    public class LeaveController : ControllerBase
    {
        private readonly HrmsDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<LeaveController> _logger;

        public LeaveController(HrmsDbContext context, IEmailService emailService, ILogger<LeaveController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [Authorize(Roles = "Employee")]
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLeave([FromBody] ApplyLeaveRequest request)
        {
            var employeeId = await ResolveEmployeeIdAsync();
            if (employeeId == null)
            {
                return BadRequest("Employee mapping not found for logged-in user.");
            }

            if (request.FromDate.Date > request.ToDate.Date)
            {
                return BadRequest("FromDate cannot be after ToDate.");
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId.Value && e.IsActive);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            var leaveDays = (request.ToDate.Date - request.FromDate.Date).Days + 1;
            if (leaveDays <= 0)
            {
                return BadRequest("Invalid leave duration.");
            }

            var leaveType = string.IsNullOrWhiteSpace(request.LeaveType) ? "Casual" : request.LeaveType.Trim();

            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId.Value && x.LeaveType == leaveType);

            if (balance == null)
            {
                balance = new LeaveBalance
                {
                    EmployeeId = employeeId.Value,
                    LeaveType = leaveType,
                    TotalLeaves = 20,
                    UsedLeaves = 0
                };
                await _context.LeaveBalances.AddAsync(balance);
            }

            if (balance.RemainingLeaves < leaveDays)
            {
                return BadRequest($"Insufficient leave balance. Remaining: {balance.RemainingLeaves}.");
            }

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = employeeId.Value,
                LeaveType = leaveType,
                FromDate = request.FromDate.Date,
                ToDate = request.ToDate.Date,
                Reason = request.Reason,
                Status = "Pending",
                AppliedOn = DateTime.UtcNow
            };

            await _context.LeaveRequests.AddAsync(leaveRequest);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Leave applied successfully.",
                LeaveRequestId = leaveRequest.Id,
                Status = leaveRequest.Status
            });
        }

        [Authorize(Roles = "Admin,HR")]
        [HasPermission("CanManageLeaves")]
        [HttpPost("{leaveRequestId:int}/approve")]
        public async Task<IActionResult> ApproveLeave(int leaveRequestId, [FromBody] LeaveActionRequest? request)
        {
            var leaveRequest = await _context.LeaveRequests
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Id == leaveRequestId);

            if (leaveRequest == null)
            {
                return NotFound("Leave request not found.");
            }

            if (!string.Equals(leaveRequest.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only pending leave requests can be approved.");
            }

            var leaveDays = (leaveRequest.ToDate.Date - leaveRequest.FromDate.Date).Days + 1;
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(x => x.EmployeeId == leaveRequest.EmployeeId && x.LeaveType == leaveRequest.LeaveType);

            if (balance == null)
            {
                return BadRequest("Leave balance is not configured for this employee and leave type.");
            }

            if (balance.RemainingLeaves < leaveDays)
            {
                return BadRequest($"Insufficient leave balance. Remaining: {balance.RemainingLeaves}.");
            }

            balance.UsedLeaves += leaveDays;
            leaveRequest.Status = "Approved";
            leaveRequest.ActionBy = User?.Identity?.Name ?? "Admin";
            leaveRequest.ActionOn = DateTime.UtcNow;
            leaveRequest.RejectionReason = request?.Comment;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(leaveRequest.Employee?.Email))
            {
                try
                {
                    var body = $@"
                        <h3>Leave Request Approved</h3>
                        <p>Hello {leaveRequest.Employee.Name},</p>
                        <p>Your leave request from <b>{leaveRequest.FromDate:dd-MMM-yyyy}</b> to <b>{leaveRequest.ToDate:dd-MMM-yyyy}</b> has been approved.</p>
                        <p>Leave Type: <b>{leaveRequest.LeaveType}</b></p>
                        <p>Regards,<br />HR Team</p>";

                    await _emailService.SendEmailAsync(
                        leaveRequest.Employee.Email,
                        "Leave Approval Notification",
                        body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send leave approval email for LeaveRequestId {LeaveRequestId}", leaveRequest.Id);
                }
            }

            return Ok(new { Message = "Leave approved successfully." });
        }

        [Authorize(Roles = "Admin,HR")]
        [HasPermission("CanManageLeaves")]
        [HttpPost("{leaveRequestId:int}/reject")]
        public async Task<IActionResult> RejectLeave(int leaveRequestId, [FromBody] LeaveActionRequest? request)
        {
            var leaveRequest = await _context.LeaveRequests
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Id == leaveRequestId);

            if (leaveRequest == null)
            {
                return NotFound("Leave request not found.");
            }

            if (!string.Equals(leaveRequest.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only pending leave requests can be rejected.");
            }

            leaveRequest.Status = "Rejected";
            leaveRequest.ActionBy = User?.Identity?.Name ?? "Admin";
            leaveRequest.ActionOn = DateTime.UtcNow;
            leaveRequest.RejectionReason = request?.Comment;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(leaveRequest.Employee?.Email))
            {
                try
                {
                    var body = $@"
                        <h3>Leave Request Rejected</h3>
                        <p>Hello {leaveRequest.Employee.Name},</p>
                        <p>Your leave request from <b>{leaveRequest.FromDate:dd-MMM-yyyy}</b> to <b>{leaveRequest.ToDate:dd-MMM-yyyy}</b> has been rejected.</p>
                        <p>Comments: <b>{request?.Comment ?? "N/A"}</b></p>
                        <p>Regards,<br />HR Team</p>";

                    await _emailService.SendEmailAsync(
                        leaveRequest.Employee.Email,
                        "Leave Rejection Notification",
                        body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send leave rejection email for LeaveRequestId {LeaveRequestId}", leaveRequest.Id);
                }
            }

            return Ok(new { Message = "Leave rejected successfully." });
        }

        [Authorize(Roles = "Employee")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyLeaves()
        {
            var employeeId = await ResolveEmployeeIdAsync();
            if (employeeId == null)
            {
                return BadRequest("Employee mapping not found for logged-in user.");
            }

            var leaves = await _context.LeaveRequests
                .Where(x => x.EmployeeId == employeeId.Value)
                .OrderByDescending(x => x.AppliedOn)
                .ToListAsync();

            return Ok(leaves);
        }

        [Authorize(Roles = "Employee")]
        [HttpGet("my/balance")]
        public async Task<IActionResult> GetMyLeaveBalance()
        {
            var employeeId = await ResolveEmployeeIdAsync();
            if (employeeId == null)
            {
                return BadRequest("Employee mapping not found for logged-in user.");
            }

            var balances = await _context.LeaveBalances
                .Where(x => x.EmployeeId == employeeId.Value)
                .Select(x => new
                {
                    x.Id,
                    x.EmployeeId,
                    x.LeaveType,
                    x.TotalLeaves,
                    x.UsedLeaves,
                    RemainingLeaves = x.TotalLeaves - x.UsedLeaves
                })
                .ToListAsync();

            return Ok(balances);
        }

        [Authorize(Roles = "Admin,HR")]
        [HasPermission("CanManageLeaves")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllLeaves([FromQuery] string? status = null)
        {
            var query = _context.LeaveRequests
                .Include(x => x.Employee)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status);
            }

            var leaves = await query
                .OrderByDescending(x => x.AppliedOn)
                .Select(x => new
                {
                    x.Id,
                    x.EmployeeId,
                    EmployeeName = x.Employee != null ? x.Employee.Name : null,
                    x.LeaveType,
                    x.FromDate,
                    x.ToDate,
                    x.Reason,
                    x.Status,
                    x.AppliedOn,
                    x.ActionBy,
                    x.ActionOn,
                    x.RejectionReason
                })
                .ToListAsync();

            return Ok(leaves);
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
