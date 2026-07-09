using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces
{
    public interface IMasterDataService
    {
        Task<MasterDataResponse> GetStaffMastersAsync();
    }
}
