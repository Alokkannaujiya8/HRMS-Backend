using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly HrmsDbContext _context;

        public EmployeeRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context
                .Employees.Include(x => x.Department)
                .Where(x => x.IsActive == true) // Soft Delete filter
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task AddAsync(Employee employee)
        {
            var departmentExists = await _context.Departments.AnyAsync(d =>
                d.Id == employee.DepartmentId
            );

            if (!departmentExists)
            {
                throw new ArgumentException(
                    $"Department with ID {employee.DepartmentId} does not exist."
                );
            }

            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            var existingEmployee = await _context.Employees.FindAsync(employee.Id);
            if (existingEmployee != null)
            {
                _context.Entry(existingEmployee).CurrentValues.SetValues(employee);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"Employee with ID {employee.Id} not found.");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var emp = await _context.Employees.FindAsync(id);

            if (emp != null)
            {
                emp.IsActive = false;
                //_context.Employees.Remove(emp);
                await _context.SaveChangesAsync();
            }
        }
    }
}
