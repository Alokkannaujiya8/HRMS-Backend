using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Results;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Services
{
    /// <summary>
    /// Employee service implementation utilizing IUnitOfWork and Result pattern.
    /// </summary>
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(IUnitOfWork unitOfWork, ILogger<EmployeeService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<IReadOnlyList<Employee>>> GetEmployeesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving active employee list.");
            var employees = await _unitOfWork.Employees.FindAsync(x => x.IsActive, cancellationToken);
            return Result.Success(employees);
        }

        public async Task<Result<Employee>> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving employee profile for ID {EmployeeId}", id);
            var employee = await _unitOfWork.Employees.GetByIdAsync(id, cancellationToken);
            if (employee == null || !employee.IsActive)
            {
                return Result.Failure<Employee>(Error.NotFound("Employee.NotFound", $"Employee with ID {id} was not found."));
            }

            return Result.Success(employee);
        }

        public async Task<Result<Employee>> AddEmployeeAsync(Employee employee, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new employee: {EmployeeName}", employee.Name);
            if (employee.DepartmentId > 0)
            {
                var deptExists = await _unitOfWork.Departments.ExistsAsync(d => d.Id == employee.DepartmentId, cancellationToken);
                if (!deptExists)
                {
                    return Result.Failure<Employee>(Error.Validation("Department.NotFound", $"Department ID {employee.DepartmentId} does not exist."));
                }
            }

            await _unitOfWork.Employees.AddAsync(employee, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(employee);
        }

        public async Task<Result> UpdateEmployeeAsync(Employee employee, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating employee profile for ID {EmployeeId}", employee.Id);
            var existing = await _unitOfWork.Employees.GetByIdAsync(employee.Id, cancellationToken);
            if (existing == null)
            {
                return Result.Failure(Error.NotFound("Employee.NotFound", $"Employee with ID {employee.Id} was not found."));
            }

            _unitOfWork.Employees.Update(employee);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result> DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Soft deleting employee with ID {EmployeeId}", id);
            var existing = await _unitOfWork.Employees.GetByIdAsync(id, cancellationToken);
            if (existing == null)
            {
                return Result.Failure(Error.NotFound("Employee.NotFound", $"Employee with ID {id} was not found."));
            }

            existing.IsActive = false;
            _unitOfWork.Employees.Update(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}