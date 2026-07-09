using HRMS.API.Authorization;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Authorize(Roles = AppRoles.AdminOrHr)]
    public class StaffMasterController : ApiControllerBase
    {
        private readonly IStaffOnboardingService _staffOnboardingService;

        public StaffMasterController(IStaffOnboardingService staffOnboardingService)
        {
            _staffOnboardingService = staffOnboardingService;
        }

        [HttpPost("onboard")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> Onboard([FromBody] StaffOnboardRequest request)
        {
            var employeeId = await _staffOnboardingService.OnboardAsync(request);
            return Ok(new { Message = "Employee onboarded successfully.", Id = employeeId });
        }
    }
}
