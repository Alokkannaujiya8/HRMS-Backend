using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class StaffOnboardingService : IStaffOnboardingService
    {
        private readonly HrmsDbContext _context;

        public StaffOnboardingService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<int> OnboardAsync(StaffOnboardRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            {
                throw new InvalidOperationException("Name and Email are required.");
            }

            var normalizedEmail = request.Email.Trim();
            var duplicateEmail = await _context.Employees.AnyAsync(e => e.Email == normalizedEmail && e.IsActive);
            if (duplicateEmail)
            {
                throw new InvalidOperationException("An active employee already exists with this email.");
            }

            var employee = new Employee
            {
                Name = request.Name.Trim(),
                Email = normalizedEmail,
                Mobile = request.Mobile?.Trim(),
                Salary = request.Salary,
                DepartmentId = request.DepartmentId,
                JoinDate = request.JoinDate.GetValueOrDefault(DateTime.UtcNow),
                Address = request.Address?.Trim(),
                Designation = request.Designation?.Trim(),
                Division = request.Division?.Trim(),
                Pan = request.Pan?.Trim(),
                Dob = request.Dob,
                Gender = request.Gender?.Trim(),
                EmploymentStatus = request.EmploymentStatus?.Trim(),
                EmploymentType = request.EmploymentType?.Trim(),
                IsSeventhPayCommission = request.IsSeventhPayCommission,
                BankAccountNumber = request.BankAccountNumber?.Trim(),
                IfscCode = request.IfscCode?.Trim(),
                IsActive = true
            };

            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            return employee.Id;
        }
    }
}
