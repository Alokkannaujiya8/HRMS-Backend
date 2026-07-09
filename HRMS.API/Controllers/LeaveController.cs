using HRMS.API.Authorization;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMS.API.Controllers
{
    [Authorize]
    public class LeaveController : ApiControllerBase
    {
        private readonly ILeaveService _leaveService;

        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [Authorize(Roles = AppRoles.Employee)]
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLeave([FromBody] ApplyLeaveRequest request)
        {
            var employeeId = await ResolveEmployeeIdAsync();
            if (employeeId == null)
            {
                return BadRequest("Employee mapping not found for logged-in user.");
            }

            return Ok(await _leaveService.ApplyLeaveAsync(employeeId.Value, request));
        }

        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanManageLeaves")]
        [HttpPost("{leaveRequestId:int}/approve")]
        public async Task<IActionResult> ApproveLeave(int leaveRequestId, [FromBody] LeaveActionRequest? request)
        {
            await _leaveService.ApproveLeaveAsync(
                leaveRequestId,
                request?.Comment,
                User?.Identity?.Name ?? AppRoles.Admin);

            return Ok(new { Message = "Leave approved successfully." });
        }

        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanManageLeaves")]
        [HttpPost("{leaveRequestId:int}/reject")]
        public async Task<IActionResult> RejectLeave(int leaveRequestId, [FromBody] LeaveActionRequest? request)
        {
            await _leaveService.RejectLeaveAsync(
                leaveRequestId,
                request?.Comment,
                User?.Identity?.Name ?? AppRoles.Admin);

            return Ok(new { Message = "Leave rejected successfully." });
        }

        [Authorize(Roles = AppRoles.Employee)]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyLeaves()
        {
            var employeeId = await ResolveEmployeeIdAsync();
            if (employeeId == null)
            {
                return BadRequest("Employee mapping not found for logged-in user.");
            }

            return Ok(await _leaveService.GetEmployeeLeavesAsync(employeeId.Value));
        }

        [Authorize(Roles = AppRoles.Employee)]
        [HttpGet("my/balance")]
        public async Task<IActionResult> GetMyLeaveBalance()
        {
            var employeeId = await ResolveEmployeeIdAsync();
            if (employeeId == null)
            {
                return BadRequest("Employee mapping not found for logged-in user.");
            }

            return Ok(await _leaveService.GetEmployeeLeaveBalancesAsync(employeeId.Value));
        }

        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanManageLeaves")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllLeaves([FromQuery] string? status = null)
        {
            return Ok(await _leaveService.GetAllLeavesAsync(status));
        }

        private async Task<int?> ResolveEmployeeIdAsync()
        {
            return await _leaveService.ResolveEmployeeIdAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                User.Identity?.Name);
        }
    }
}
