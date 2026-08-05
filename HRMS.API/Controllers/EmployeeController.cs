using HRMS.Application.Features.Employees.Commands.CreateEmployee;
using HRMS.Application.Features.Employees.Commands.DeleteEmployee;
using HRMS.Application.Features.Employees.Commands.UpdateEmployee;
using HRMS.Application.Features.Employees.Queries.GetEmployeeById;
using HRMS.Application.Features.Employees.Queries.GetEmployees;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    /// <summary>
    /// API Controller managing Employee entity operations using CQRS pattern with MediatR.
    /// </summary>
    public class EmployeeController : ApiControllerBase
    {
        private readonly ISender _mediator;
        private readonly IEmailService _emailService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(
            ISender mediator,
            IEmailService emailService,
            IFileStorageService fileStorageService,
            ILogger<EmployeeController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves list of active employees via GetEmployeesQuery.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEmployees(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetEmployeesQuery(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Retrieves specific employee details by ID via GetEmployeeByIdQuery.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployee(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetEmployeeByIdQuery(id), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Creates a new employee record via CreateEmployeeCommand and dispatches welcome email.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddEmployee([FromBody] Employee emp, CancellationToken cancellationToken)
        {
            var command = new CreateEmployeeCommand(
                emp.Name,
                emp.Email,
                emp.Mobile,
                emp.Salary,
                emp.DepartmentId,
                emp.JoinDate,
                emp.Designation,
                emp.Address);

            var result = await _mediator.Send(command, cancellationToken);
            if (result.IsSuccess && !string.IsNullOrWhiteSpace(emp.Email))
            {
                var displayName = string.IsNullOrWhiteSpace(emp.Name) ? "Employee" : emp.Name;
                var htmlBody = $@"
                    <h2>Welcome to HRMS, {displayName}!</h2>
                    <p>Your employee profile has been created successfully.</p>
                    <p>We're excited to have you onboard.</p>
                    <br />
                    <p>Regards,<br />HR Team</p>";

                try
                {
                    await _emailService.SendEmailAsync(emp.Email, "Welcome to HRMS", htmlBody);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send welcome email to {Email}", emp.Email);
                }
            }

            return ToActionResult(result);
        }

        /// <summary>
        /// Updates an existing employee profile via UpdateEmployeeCommand.
        /// </summary>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEmployee([FromBody] Employee emp, CancellationToken cancellationToken)
        {
            var command = new UpdateEmployeeCommand(
                emp.Id,
                emp.Name,
                emp.Email,
                emp.Mobile,
                emp.Salary,
                emp.DepartmentId,
                emp.Designation,
                emp.Address);

            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Soft deletes an employee by ID via DeleteEmployeeCommand.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEmployee(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteEmployeeCommand(id), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Uploads an employee photo asset.
        /// </summary>
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadPhoto(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = "No file was uploaded."
                });
            }

            var photoUrl = await _fileStorageService.SaveFileAsync(file, "photos", cancellationToken);
            return Ok(new { photoUrl });
        }

        /// <summary>
        /// Uploads an employee resume document (PDF/DOCX).
        /// </summary>
        [HttpPost("upload-resume")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadResume(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = "No resume file was uploaded."
                });
            }

            var resumeUrl = await _fileStorageService.SaveFileAsync(file, "resumes", cancellationToken);
            return Ok(new { resumeUrl });
        }

        /// <summary>
        /// Uploads an employee identity or compliance document (PAN, Aadhaar, Passport).
        /// </summary>
        [HttpPost("upload-document")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadDocument([FromForm] string documentType, IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = "No document file was uploaded."
                });
            }

            var subFolder = string.IsNullOrWhiteSpace(documentType) ? "documents" : $"documents/{documentType.ToLowerInvariant()}";
            var documentUrl = await _fileStorageService.SaveFileAsync(file, subFolder, cancellationToken);
            return Ok(new { documentType, documentUrl });
        }
    }
}
