using HRMS.Application.Common.Results;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using MediatR;

namespace HRMS.Application.Features.Attendance.Queries.GetAllAttendance
{
    public record GetAllAttendanceQuery(
        int? EmployeeId,
        DateTime? FromDate,
        DateTime? ToDate) : IRequest<Result<IReadOnlyCollection<AttendanceListItemResponse>>>;

    public class GetAllAttendanceQueryHandler : IRequestHandler<GetAllAttendanceQuery, Result<IReadOnlyCollection<AttendanceListItemResponse>>>
    {
        private readonly IAttendanceService _attendanceService;

        public GetAllAttendanceQueryHandler(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService ?? throw new ArgumentNullException(nameof(attendanceService));
        }

        public async Task<Result<IReadOnlyCollection<AttendanceListItemResponse>>> Handle(GetAllAttendanceQuery request, CancellationToken cancellationToken)
        {
            var records = await _attendanceService.GetAllAttendanceAsync(request.EmployeeId, request.FromDate, request.ToDate, cancellationToken);
            return Result.Success(records);
        }
    }
}
