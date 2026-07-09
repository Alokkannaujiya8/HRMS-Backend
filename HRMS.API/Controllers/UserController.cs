using HRMS.API.Authorization;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Authorize(Roles = AppRoles.AdminOrHr)]
    public class UserController : ApiControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IAuthService _authService;

        public UserController(IUserManagementService userManagementService, IAuthService authService)
        {
            _userManagementService = userManagementService;
            _authService = authService;
        }

        [HttpGet]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await _userManagementService.GetUsersAsync());
        }

        [HttpPost]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> AddUser(RegisterRequest user)
        {
            var response = await _authService.RegisterAsync(user);

            if (response.Message == "Username already exists!" || response.Message == "Employee mapping is invalid.")
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
