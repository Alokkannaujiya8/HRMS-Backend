namespace HRMS.Application.Interfaces
{
    public interface ILeaveBalanceAutomationJobService
    {
        Task RecalculateMonthlyLeaveBalancesAsync();
    }
}
