using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces
{
    public interface IStaffOnboardingService
    {
        Task<int> OnboardAsync(StaffOnboardRequest request);
    }
}
