using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HRMS.Infrastructure.Services
{
    public class EmployeeDetailsService : IEmployeeDetailsService
    {
        private readonly HrmsDbContext _context;

        public EmployeeDetailsService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeDetailsResponse> GetFullProfileAsync(int employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Skills)
                .Include(e => e.Remarks)
                .Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive);

            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            var salary = await _context.SalaryManagements
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId);

            return new EmployeeDetailsResponse
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
        }

        public async Task AddSkillAsync(int employeeId, AddSkillRequest request)
        {
            await EnsureActiveEmployeeAsync(employeeId);

            await _context.EmployeeSkills.AddAsync(new EmployeeSkill
            {
                EmployeeId = employeeId,
                SkillName = request.SkillName,
                Proficiency = request.Proficiency,
                AddedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task AddRemarkAsync(int employeeId, AddRemarkRequest request, string addedBy)
        {
            await EnsureActiveEmployeeAsync(employeeId);

            await _context.EmployeeRemarks.AddAsync(new EmployeeRemark
            {
                EmployeeId = employeeId,
                Remark = request.Remark,
                AddedBy = addedBy,
                AddedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task UploadDocumentAsync(int employeeId, string documentType, string fileName, Stream fileStream)
        {
            await EnsureActiveEmployeeAsync(employeeId);

            if (fileStream.Length == 0)
            {
                throw new InvalidOperationException("No file uploaded.");
            }

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "employee-documents");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var safeFileName = $"{employeeId}_{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var filePath = Path.Combine(folder, safeFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            await _context.EmployeeDocuments.AddAsync(new EmployeeDocument
            {
                EmployeeId = employeeId,
                DocumentType = documentType,
                FilePath = $"/employee-documents/{safeFileName}",
                UploadedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task VerifyAadhaarAsync(int employeeId, VerifyAadhaarRequest request)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive);
            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            employee.AadhaarNumber = request.AadhaarNumber;
            employee.IsAadhaarVerified = true;

            await _context.SaveChangesAsync();
        }

        public Task<ReportFileResponse> GenerateAppointmentLetterAsync(int employeeId)
        {
            return GenerateLetterAsync(employeeId, "Appointment Letter", "We are pleased to appoint you to the organization.", $"appointment-letter-{employeeId}.pdf");
        }

        public Task<ReportFileResponse> GenerateAppraisalLetterAsync(int employeeId)
        {
            return GenerateLetterAsync(employeeId, "Appraisal Letter", "We appreciate your performance and contributions.", $"appraisal-letter-{employeeId}.pdf");
        }

        private async Task<ReportFileResponse> GenerateLetterAsync(int employeeId, string title, string body, string fileName)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive);
            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            var pdf = Document.Create(container =>
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

            return new ReportFileResponse
            {
                Content = pdf,
                ContentType = "application/pdf",
                FileName = fileName
            };
        }

        private async Task EnsureActiveEmployeeAsync(int employeeId)
        {
            var exists = await _context.Employees.AnyAsync(e => e.Id == employeeId && e.IsActive);
            if (!exists)
            {
                throw new KeyNotFoundException("Employee not found.");
            }
        }
    }
}
