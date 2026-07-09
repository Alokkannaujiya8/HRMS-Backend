using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface IPayrollManagementService
    {
        Task<List<Payroll>> GetPayrollsAsync();

        Task<Payroll> AddPayrollAsync(Payroll payroll);
    }
}
