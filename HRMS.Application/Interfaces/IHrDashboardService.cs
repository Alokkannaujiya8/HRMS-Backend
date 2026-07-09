using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces
{
    public interface IHrDashboardService
    {
        Task<HrDashboardSummaryResponse> GetSummaryAsync();
    }
}
