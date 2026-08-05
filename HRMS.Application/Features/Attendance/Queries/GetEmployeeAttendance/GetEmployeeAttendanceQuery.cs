using System.Security.Claims;
using HRMS.Application.Common.Results;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using MediatR;

namespace HRMS.Application.Features.Attendance.Queries.GetEmployeeAttendance
{
    public record GetEmployeeAttendanceQuery(
        ClaimsPrincipal User,
        DateTime? FromDate,
        DateTime? ToDate) : IRequest<Result<IReadOnlyCollection<AttendanceListItemResponse>>>;

    public class GetEmployeeAttendanceQueryHandler : IRequestHandler<GetEmployeeAttendanceQuery, Result<IReadOnlyCollection<AttendanceListItemResponse>>>
    {
        private readonly IAttendanceService _attendanceService;

        public GetEmployeeAttendanceQueryHandler(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService ?? throw new ArgumentNullException(nameof(attendanceService));
        }

        public async Task<Result<IReadOnlyCollection<AttendanceListItemResponse>>> Handle(GetEmployeeAttendanceQuery request, CancellationToken cancellationToken)
        {
            var userId = request.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = request.User.Identity?.Name;

            var employeeId = await _attendanceService.ResolveEmployeeIdAsync(userId, userName, cancellationToken);
            if (!employeeId.HasValue)
            {
                return Result.Failure<IReadOnlyCollection<AttendanceListItemResponse>>(Error.Validation("Attendance.EmployeeNotFound", "Employee mapping not found for logged-in user."));
            }

            var records = await _attendanceService.GetEmployeeAttendanceAsync(employeeId.Value, request.FromDate, request.ToDate, cancellationToken);
            return Result.Success(records);
        }
    }
}
