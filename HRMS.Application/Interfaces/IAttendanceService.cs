using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<int?> ResolveEmployeeIdAsync(string? employeeIdClaim, string? username);
        Task<AttendanceCheckInResponse> CheckInAsync(int employeeId);
        Task<AttendanceCheckOutResponse> CheckOutAsync(int employeeId);
        Task<IReadOnlyCollection<AttendanceListItemResponse>> GetEmployeeAttendanceAsync(int employeeId, DateTime? fromDate, DateTime? toDate);
        Task<IReadOnlyCollection<AttendanceListItemResponse>> GetAllAttendanceAsync(int? employeeId, DateTime? fromDate, DateTime? toDate);
    }
}
