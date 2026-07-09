using HRMS.Application.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface ISalaryManagementService
    {
        Task<SalaryManagement> UpsertAsync(SalaryManagementRequest request);

        Task<SalaryManagement?> GetByEmployeeAsync(int employeeId);
    }
}
