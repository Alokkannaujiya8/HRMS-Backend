using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<int?> ResolveEmployeeIdAsync(string? employeeIdClaim, string? username, CancellationToken cancellationToken = default);
        Task<AttendanceCheckInResponse> CheckInAsync(int employeeId, CancellationToken cancellationToken = default);
        Task<AttendanceCheckOutResponse> CheckOutAsync(int employeeId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<AttendanceListItemResponse>> GetEmployeeAttendanceAsync(int employeeId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<AttendanceListItemResponse>> GetAllAttendanceAsync(int? employeeId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    }
}
