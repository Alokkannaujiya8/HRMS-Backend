using System.Security.Claims;
using HRMS.Application.Common.Results;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using MediatR;

namespace HRMS.Application.Features.Attendance.Commands.CheckIn
{
    public record CheckInCommand(ClaimsPrincipal User) : IRequest<Result<AttendanceCheckInResponse>>;

    public class CheckInCommandHandler : IRequestHandler<CheckInCommand, Result<AttendanceCheckInResponse>>
    {
        private readonly IAttendanceService _attendanceService;

        public CheckInCommandHandler(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService ?? throw new ArgumentNullException(nameof(attendanceService));
        }

        public async Task<Result<AttendanceCheckInResponse>> Handle(CheckInCommand request, CancellationToken cancellationToken)
        {
            var userId = request.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = request.User.Identity?.Name;

            var employeeId = await _attendanceService.ResolveEmployeeIdAsync(userId, userName, cancellationToken);
            if (!employeeId.HasValue)
            {
                return Result.Failure<AttendanceCheckInResponse>(Error.Validation("Attendance.EmployeeNotFound", "Employee mapping not found for logged-in user."));
            }

            var response = await _attendanceService.CheckInAsync(employeeId.Value, cancellationToken);
            return Result.Success(response);
        }
    }
}
