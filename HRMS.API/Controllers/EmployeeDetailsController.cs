using HRMS.API.Authorization;
using HRMS.Application.DTOs;
using HRMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/employees/{employeeId:int}")]
    [Authorize(Roles = "Admin,HR")]
    public class EmployeeDetailsController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public EmployeeDetailsController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> GetFullProfile(int employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Skills)
                .Include(e => e.Remarks)
                .Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive);

            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            var salary = await _context.SalaryManagements
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId);

            var response = new EmployeeDetailsResponse
            {
                EmployeeId = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Mobile = employee.Mobile,
                Address = employee.Address,
                JoinDate = employee.JoinDate,
                Designation = employee.Designation,
                Division = employee.Division,
                DepartmentName = employee.Department?.Name,
                EmploymentStatus = employee.EmploymentStatus,
                EmploymentType = employee.EmploymentType,
                Pan = employee.Pan,
                Dob = employee.Dob,
                Gender = employee.Gender,
                IsSeventhPayCommission = employee.IsSeventhPayCommission,
                BankAccountNumber = employee.BankAccountNumber,
                IfscCode = employee.IfscCode,
                IsAadhaarVerified = employee.IsAadhaarVerified,
                Skills = employee.Skills?
                    .OrderByDescending(x => x.AddedAt)
                    .Select(x => new EmployeeSkillResponse
                    {
                        Id = x.Id,
                        SkillName = x.SkillName,
                        Proficiency = x.Proficiency,
                        AddedAt = x.AddedAt
                    })
                    .ToList() ?? [],
                Remarks = employee.Remarks?
                    .OrderByDescending(x => x.AddedAt)
                    .Select(x => new EmployeeRemarkResponse
                    {
                        Id = x.Id,
                        Remark = x.Remark,
                        AddedBy = x.AddedBy,
                        AddedAt = x.AddedAt
                    })
                    .ToList() ?? [],
                Documents = employee.Documents?
                    .OrderByDescending(x => x.UploadedAt)
                    .Select(x => new EmployeeDocumentResponse
                    {
                        Id = x.Id,
                        DocumentType = x.DocumentType,
                        FilePath = x.FilePath,
                        UploadedAt = x.UploadedAt
                    })
                    .ToList() ?? [],
                SalaryManagement = salary == null
                    ? null
                    : new SalarySummaryResponse
                    {
                        BasicSalary = salary.BasicSalary,
                        IncrementPercent = salary.IncrementPercent,
                        IncrementAmount = salary.IncrementAmount,
                        EffectiveDate = salary.EffectiveDate,
                        SpecialAllowance = salary.SpecialAllowance,
                        SpecialDeduction = salary.SpecialDeduction,
                        TotalSalary = salary.TotalSalary
                    }
            };

            return Ok(response);
        }

        [HttpPost("skills")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> AddSkill(int employeeId, [FromBody] AddSkillRequest request)
        {
            var exists = await _context.Employees.AnyAsync(e => e.Id == employeeId && e.IsActive);
            if (!exists) return NotFound("Employee not found.");

            await _context.EmployeeSkills.AddAsync(new Domain.Entities.EmployeeSkill
            {
                EmployeeId = employeeId,
                SkillName = request.SkillName,
                Proficiency = request.Proficiency,
                AddedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Skill added." });
        }

        [HttpPost("remarks")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> AddRemark(int employeeId, [FromBody] AddRemarkRequest request)
        {
            var exists = await _context.Employees.AnyAsync(e => e.Id == employeeId && e.IsActive);
            if (!exists) return NotFound("Employee not found.");

            await _context.EmployeeRemarks.AddAsync(new Domain.Entities.EmployeeRemark
            {
                EmployeeId = employeeId,
                Remark = request.Remark,
                AddedBy = User.Identity?.Name ?? "System",
                AddedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Remark added." });
        }

        [HttpPost("documents")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> UploadDocument(int employeeId, [FromForm] string documentType, [FromForm] IFormFile file)
        {
            var exists = await _context.Employees.AnyAsync(e => e.Id == employeeId && e.IsActive);
            if (!exists) return NotFound("Employee not found.");

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "employee-documents");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var fileName = $"{employeeId}_{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(folder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            await _context.EmployeeDocuments.AddAsync(new Domain.Entities.EmployeeDocument
            {
                EmployeeId = employeeId,
                DocumentType = documentType,
                FilePath = $"/employee-documents/{fileName}",
                UploadedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Document uploaded." });
        }

        [HttpPost("verify-aadhaar")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> VerifyAadhaar(int employeeId, [FromBody] VerifyAadhaarRequest request)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            employee.AadhaarNumber = request.AadhaarNumber;
            employee.IsAadhaarVerified = true;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Aadhaar verified successfully." });
        }

        [HttpGet("appointment-letter")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> GenerateAppointmentLetter(int employeeId)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            var pdf = GenerateLetter(employee, "Appointment Letter", "We are pleased to appoint you to the organization.");
            return File(pdf, "application/pdf", $"appointment-letter-{employeeId}.pdf");
        }

        [HttpGet("appraisal-letter")]
        [HasPermission("CanManageUsers")]
        public async Task<IActionResult> GenerateAppraisalLetter(int employeeId)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            var pdf = GenerateLetter(employee, "Appraisal Letter", "We appreciate your performance and contributions.");
            return File(pdf, "application/pdf", $"appraisal-letter-{employeeId}.pdf");
        }

        private static byte[] GenerateLetter(Domain.Entities.Employee employee, string title, string body)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Text(title).SemiBold().FontSize(20).FontColor("#0F172A");
                    page.Content().Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Text($"Date: {DateTime.UtcNow:dd-MMM-yyyy}");
                        col.Item().Text($"Employee: {employee.Name}");
                        col.Item().Text($"Designation: {employee.Designation ?? "N/A"}");
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().Text(body);
                        col.Item().Text("Regards,");
                        col.Item().Text("HR Department").SemiBold();
                    });
                });
            }).GeneratePdf();
        }
    }
}
