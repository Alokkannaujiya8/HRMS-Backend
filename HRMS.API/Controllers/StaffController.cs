using HRMS.API.Authorization;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Authorize(Roles = AppRoles.AdminOrHr)]
    public class StaffController : ApiControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> GetStaffList([FromQuery] StaffListQueryRequest request)
        {
            return Ok(await _staffService.GetStaffListAsync(request));
        }

        [HttpGet("{id:int}")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> ViewProfile(int id)
        {
            var employee = await _staffService.GetProfileAsync(id);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            return Ok(employee);
        }

        [HttpPut("{id:int}")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> EditProfile(int id, [FromBody] EditStaffProfileRequest request)
        {
            await _staffService.UpdateProfileAsync(id, request);
            return Ok(new { Message = "Employee updated successfully." });
        }
    }
}
