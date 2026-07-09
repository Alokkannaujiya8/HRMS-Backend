using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface ISalaryStructureService
    {
        Task AddOrUpdateAsync(SalaryStructure request);

        Task<SalaryStructure?> GetByEmployeeAsync(int employeeId);
    }
}
