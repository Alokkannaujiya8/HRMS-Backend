using System.Security.Claims;
using HRMS.Application.Common.Results;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using MediatR;

namespace HRMS.Application.Features.Attendance.Commands.CheckOut
{
    public record CheckOutCommand(ClaimsPrincipal User) : IRequest<Result<AttendanceCheckOutResponse>>;

    public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, Result<AttendanceCheckOutResponse>>
    {
        private readonly IAttendanceService _attendanceService;

        public CheckOutCommandHandler(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService ?? throw new ArgumentNullException(nameof(attendanceService));
        }

        public async Task<Result<AttendanceCheckOutResponse>> Handle(CheckOutCommand request, CancellationToken cancellationToken)
        {
            var userId = request.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = request.User.Identity?.Name;

            var employeeId = await _attendanceService.ResolveEmployeeIdAsync(userId, userName, cancellationToken);
            if (!employeeId.HasValue)
            {
                return Result.Failure<AttendanceCheckOutResponse>(Error.Validation("Attendance.EmployeeNotFound", "Employee mapping not found for logged-in user."));
            }

            var response = await _attendanceService.CheckOutAsync(employeeId.Value, cancellationToken);
            return Result.Success(response);
        }
    }
}
