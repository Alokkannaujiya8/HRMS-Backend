using HRMS.Application.Common.Results;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    /// <summary>
    /// High-level service contract for Employee management.
    /// </summary>
    public interface IEmployeeService
    {
        /// <summary>
        /// Retrieves all active employees.
        /// </summary>
        Task<Result<IReadOnlyList<Employee>>> GetEmployeesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an employee by unique identifier.
        /// </summary>
        Task<Result<Employee>> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Registers a new employee profile.
        /// </summary>
        Task<Result<Employee>> AddEmployeeAsync(Employee employee, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing employee profile.
        /// </summary>
        Task<Result> UpdateEmployeeAsync(Employee employee, CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft deletes an employee profile by ID.
        /// </summary>
        Task<Result> DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default);
    }
}