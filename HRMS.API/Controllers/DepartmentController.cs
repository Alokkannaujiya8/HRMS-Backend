using HRMS.API.Authorization;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using HRMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    public class DepartmentController : ApiControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            return Ok(await _departmentService.GetDepartmentsAsync());
        }

        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanManageDepartments")]
        [HttpPost]
        public async Task<IActionResult> AddDepartment(Department dept)
        {
            return Ok(await _departmentService.AddDepartmentAsync(dept, User?.Identity?.Name));
        }
    }
}
