using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetDepartmentsAsync();

        Task<Department> AddDepartmentAsync(Department department, string? userName);
    }
}
