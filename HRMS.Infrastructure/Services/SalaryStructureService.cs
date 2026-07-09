using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class SalaryStructureService : ISalaryStructureService
    {
        private readonly HrmsDbContext _context;

        public SalaryStructureService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task AddOrUpdateAsync(SalaryStructure request)
        {
            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == request.EmployeeId && e.IsActive);
            if (!employeeExists)
            {
                throw new InvalidOperationException("Employee not found.");
            }

            var existing = await _context.SalaryStructures.FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId);
            if (existing == null)
            {
                request.CreatedAt = DateTime.UtcNow;
                await _context.SalaryStructures.AddAsync(request);
            }
            else
            {
                existing.Base = request.Base;
                existing.HRA = request.HRA;
                existing.DA = request.DA;
                existing.PFDeductions = request.PFDeductions;
                existing.Tax = request.Tax;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<SalaryStructure?> GetByEmployeeAsync(int employeeId)
        {
            return await _context.SalaryStructures.FirstOrDefaultAsync(x => x.EmployeeId == employeeId);
        }
    }
}
