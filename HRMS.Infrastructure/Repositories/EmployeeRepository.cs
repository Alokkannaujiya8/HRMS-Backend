using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories
{
    /// <summary>
    /// Employee-specific repository implementing specialized queries over EF Core context.
    /// </summary>
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(HrmsDbContext context) : base(context)
        {
        }

        public async Task<List<Employee>> GetAllActiveWithDepartmentsAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet
                .AsNoTracking()
                .Include(x => x.Department)
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);
        }
    }
}
