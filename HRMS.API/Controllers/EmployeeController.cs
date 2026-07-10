using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace HRMS.API.Controllers
{
    public class EmployeeController : ApiControllerBase
    {
        private readonly IEmployeeService _service;
        private readonly IEmailService _emailService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(
            IEmployeeService service,
            IEmailService emailService,
            ILogger<EmployeeController> logger)
        {
            _service = service;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            return Ok(await _service.GetEmployees());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var emp = await _service.GetEmployee(id);
            if (emp == null)
            {
                return NotFound(new { Message = "Employee not found." });
            }
            return Ok(emp);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(Employee emp)
        {
            await _service.AddEmployee(emp);

            if (!string.IsNullOrWhiteSpace(emp.Email))
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
                    await _emailService.SendEmailAsync(
                        emp.Email,
                        "Welcome to HRMS",
                        htmlBody);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send welcome email to {Email}", emp.Email);
                }
            }

            return Ok("Employee Added");
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEmployee(Employee emp)
        {
            await _service.UpdateEmployee(emp);
            return Ok("Employee Updated");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            await _service.DeleteEmployee(id);
            return Ok("Employee Deleted");
        }


        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No File uploaded");


            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);


            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Ok(new { photoUrl = $"/uploads/{uniqueFileName}" });
        }
    } 
}

