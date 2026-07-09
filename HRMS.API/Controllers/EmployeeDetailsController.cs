using HRMS.API.Authorization;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/employees/{employeeId:int}")]
    [Authorize(Roles = AppRoles.AdminOrHr)]
    public class EmployeeDetailsController : ControllerBase
    {
        private readonly IEmployeeDetailsService _employeeDetailsService;

        public EmployeeDetailsController(IEmployeeDetailsService employeeDetailsService)
        {
            _employeeDetailsService = employeeDetailsService;
        }

        [HttpGet("profile")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> GetFullProfile(int employeeId)
        {
            return Ok(await _employeeDetailsService.GetFullProfileAsync(employeeId));
        }

        [HttpPost("skills")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> AddSkill(int employeeId, [FromBody] AddSkillRequest request)
        {
            await _employeeDetailsService.AddSkillAsync(employeeId, request);
            return Ok(new { Message = "Skill added." });
        }

        [HttpPost("remarks")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> AddRemark(int employeeId, [FromBody] AddRemarkRequest request)
        {
            await _employeeDetailsService.AddRemarkAsync(employeeId, request, User.Identity?.Name ?? "System");
            return Ok(new { Message = "Remark added." });
        }

        [HttpPost("documents")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> UploadDocument(int employeeId, [FromForm] string documentType, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            await using var stream = file.OpenReadStream();
            await _employeeDetailsService.UploadDocumentAsync(employeeId, documentType, file.FileName, stream);

            return Ok(new { Message = "Document uploaded." });
        }

        [HttpPost("verify-aadhaar")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> VerifyAadhaar(int employeeId, [FromBody] VerifyAadhaarRequest request)
        {
            await _employeeDetailsService.VerifyAadhaarAsync(employeeId, request);
            return Ok(new { Message = "Aadhaar verified successfully." });
        }

        [HttpGet("appointment-letter")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> GenerateAppointmentLetter(int employeeId)
        {
            var file = await _employeeDetailsService.GenerateAppointmentLetterAsync(employeeId);
            return File(file.Content, file.ContentType, file.FileName);
        }

        [HttpGet("appraisal-letter")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> GenerateAppraisalLetter(int employeeId)
        {
            var file = await _employeeDetailsService.GenerateAppraisalLetterAsync(employeeId);
            return File(file.Content, file.ContentType, file.FileName);
        }
    }
}
