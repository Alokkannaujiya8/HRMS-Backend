using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class SalaryManagementService : ISalaryManagementService
    {
        private readonly HrmsDbContext _context;

        public SalaryManagementService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<SalaryManagement> UpsertAsync(SalaryManagementRequest request)
        {
            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == request.EmployeeId && e.IsActive);
            if (!employeeExists)
            {
                throw new InvalidOperationException("Employee not found.");
            }

            var incrementAmount = Math.Round(request.BasicSalary * request.IncrementPercent / 100m, 2);
            var totalSalary = request.BasicSalary + incrementAmount + request.SpecialAllowance - request.SpecialDeduction;

            var existing = await _context.SalaryManagements.FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId);
            if (existing == null)
            {
                existing = new SalaryManagement
                {
                    EmployeeId = request.EmployeeId
                };
                await _context.SalaryManagements.AddAsync(existing);
            }

            existing.BasicSalary = request.BasicSalary;
            existing.IncrementPercent = request.IncrementPercent;
            existing.IncrementAmount = incrementAmount;
            existing.EffectiveDate = request.EffectiveDate;
            existing.SpecialAllowance = request.SpecialAllowance;
            existing.SpecialDeduction = request.SpecialDeduction;
            existing.TotalSalary = totalSalary;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<SalaryManagement?> GetByEmployeeAsync(int employeeId)
        {
            return await _context.SalaryManagements.FirstOrDefaultAsync(x => x.EmployeeId == employeeId);
        }
    }
}
