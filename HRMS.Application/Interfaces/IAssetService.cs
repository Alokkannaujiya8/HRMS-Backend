using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces
{
    public interface IAssetService
    {
        Task<List<AssetDto>> GetAllAssetsAsync();
        Task<AssetDetailDto?> GetAssetByIdAsync(int id);
        Task<AssetDto> CreateAssetAsync(AssetCreateDto dto);
        Task<AssetDto?> UpdateAssetAsync(int id, AssetUpdateDto dto);
        Task<bool> DeleteAssetAsync(int id);
        Task<bool> AssignAssetAsync(int id, AssetAssignDto dto);
        Task<bool> ReturnAssetAsync(int id, AssetReturnDto dto);
        Task<List<AssetAssignmentDto>> GetEmployeeAssetsAsync(int employeeId);
    }
}
