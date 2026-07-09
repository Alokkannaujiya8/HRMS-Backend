using HRMS.Application.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface IStaffService
    {
        Task<StaffListResponse> GetStaffListAsync(StaffListQueryRequest request);
        Task<Employee?> GetProfileAsync(int id);
        Task UpdateProfileAsync(int id, EditStaffProfileRequest request);
    }
}
