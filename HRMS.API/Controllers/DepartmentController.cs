using HRMS.API.Authorization;
using HRMS.Application.Features.Departments.Commands.CreateDepartment;
using HRMS.Application.Features.Departments.Queries.GetDepartments;
using HRMS.Application.Security;
using HRMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    /// <summary>
    /// API Controller managing Department entity operations via CQRS pattern.
    /// </summary>
    public class DepartmentController : ApiControllerBase
    {
        private readonly ISender _mediator;

        public DepartmentController(ISender mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Retrieves list of all departments.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDepartments(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetDepartmentsQuery(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Registers a new department.
        /// </summary>
        [Authorize(Roles = AppRoles.AdminOrHr)]
        [HasPermission("CanManageDepartments")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDepartment([FromBody] Department dept, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CreateDepartmentCommand(dept.Name ?? string.Empty, dept.Code), cancellationToken);
            return ToActionResult(result);
        }
    }
}
