using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class PayrollManagementService : IPayrollManagementService
    {
        private readonly HrmsDbContext _context;

        public PayrollManagementService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<Payroll>> GetPayrollsAsync()
        {
            return await _context.Payrolls.ToListAsync();
        }

        public async Task<Payroll> AddPayrollAsync(Payroll payroll)
        {
            await _context.Payrolls.AddAsync(payroll);
            await _context.SaveChangesAsync();
            return payroll;
        }
    }
}
