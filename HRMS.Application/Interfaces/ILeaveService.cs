using HRMS.Application.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface ILeaveService
    {
        Task<int?> ResolveEmployeeIdAsync(string? employeeIdClaim, string? username);
        Task<LeaveApplyResponse> ApplyLeaveAsync(int employeeId, ApplyLeaveRequest request);
        Task ApproveLeaveAsync(int leaveRequestId, string? comment, string actionBy);
        Task RejectLeaveAsync(int leaveRequestId, string? comment, string actionBy);
        Task<IReadOnlyCollection<LeaveRequest>> GetEmployeeLeavesAsync(int employeeId);
        Task<IReadOnlyCollection<LeaveBalanceResponse>> GetEmployeeLeaveBalancesAsync(int employeeId);
        Task<IReadOnlyCollection<LeaveListItemResponse>> GetAllLeavesAsync(string? status);
    }
}
