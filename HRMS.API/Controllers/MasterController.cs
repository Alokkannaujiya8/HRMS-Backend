using HRMS.API.Authorization;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Authorize(Roles = AppRoles.AdminOrHr)]
    public class MasterController : ApiControllerBase
    {
        private readonly IMasterDataService _masterDataService;

        public MasterController(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
        }

        [HttpGet("staff-masters")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> GetStaffMasters()
        {
            return Ok(await _masterDataService.GetStaffMastersAsync());
        }
    }
}
