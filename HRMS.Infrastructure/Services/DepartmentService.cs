using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly HrmsDbContext _context;

        public DepartmentService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<Department>> GetDepartmentsAsync()
        {
            return await _context.Departments.ToListAsync();
        }

        public async Task<Department> AddDepartmentAsync(Department department, string? userName)
        {
            if (string.IsNullOrWhiteSpace(department.Code))
            {
                throw new InvalidOperationException("Department code is required.");
            }

            var normalizedCode = department.Code.Trim().ToUpperInvariant();
            var exists = await _context.Departments.AnyAsync(x => x.Code == normalizedCode);
            if (exists)
            {
                throw new InvalidOperationException($"Department code '{normalizedCode}' already exists.");
            }

            department.Code = normalizedCode;
            department.Name = department.Name?.Trim();
            department.CreatedDate = DateTime.UtcNow;

            await _context.Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key=N'AppUser', @value={0}", userName ?? "System");

            try
            {
                await _context.Departments.AddAsync(department);
                await _context.SaveChangesAsync();
            }
            finally
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key=N'AppUser', @value=NULL");
            }

            return department;
        }
    }
}
