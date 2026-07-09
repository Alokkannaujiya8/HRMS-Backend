using HRMS.API.Authorization;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Authorize(Roles = AppRoles.AdminOrHr)]
    public class HrDashboardController : ApiControllerBase
    {
        private readonly IHrDashboardService _dashboardService;

        public HrDashboardController(IHrDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> GetSummary()
        {
            return Ok(await _dashboardService.GetSummaryAsync());
        }
    }
}
