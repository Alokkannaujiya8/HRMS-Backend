using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class LeaveBalanceAutomationJobService : ILeaveBalanceAutomationJobService
    {
        private const int MonthlyCredit = 2;
        private const int MaxCarryForward = 30;

        private readonly HrmsDbContext _context;

        public LeaveBalanceAutomationJobService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task RecalculateMonthlyLeaveBalancesAsync()
        {
            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => e.Id)
                .ToListAsync();

            foreach (var employeeId in employees)
            {
                var balances = await _context.LeaveBalances
                    .Where(lb => lb.EmployeeId == employeeId)
                    .ToListAsync();

                if (balances.Count == 0)
                {
                    await _context.LeaveBalances.AddAsync(new LeaveBalance
                    {
                        EmployeeId = employeeId,
                        LeaveType = "Casual",
                        TotalLeaves = MonthlyCredit,
                        UsedLeaves = 0
                    });

                    continue;
                }

                foreach (var balance in balances)
                {
                    var remaining = Math.Max(0, balance.TotalLeaves - balance.UsedLeaves);
                    balance.TotalLeaves = Math.Min(MaxCarryForward, remaining + MonthlyCredit);
                    balance.UsedLeaves = 0;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
