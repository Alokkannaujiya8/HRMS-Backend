using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    /// <summary>
    /// Specialized repository contract for Employee entity management.
    /// </summary>
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        /// <summary>
        /// Retrieves active employees along with department details.
        /// </summary>
        Task<List<Employee>> GetAllActiveWithDepartmentsAsync(CancellationToken cancellationToken = default);
    }
}