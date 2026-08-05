using HRMS.Application.Common.Results;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using MediatR;

namespace HRMS.Application.Features.Attendance.Queries.GetOvertimeReport
{
    public record GetOvertimeReportQuery(
        DateTime? FromDate,
        DateTime? ToDate,
        int? EmployeeId,
        double OvertimeAfterHours = 8,
        decimal StandardMonthlyHours = 208) : IRequest<Result<OvertimeReportResponse>>;

    public class GetOvertimeReportQueryHandler : IRequestHandler<GetOvertimeReportQuery, Result<OvertimeReportResponse>>
    {
        private readonly IAttendanceReportingService _attendanceReportingService;

        public GetOvertimeReportQueryHandler(IAttendanceReportingService attendanceReportingService)
        {
            _attendanceReportingService = attendanceReportingService ?? throw new ArgumentNullException(nameof(attendanceReportingService));
        }

        public async Task<Result<OvertimeReportResponse>> Handle(GetOvertimeReportQuery request, CancellationToken cancellationToken)
        {
            var to = (request.ToDate ?? DateTime.UtcNow).Date;
            var from = (request.FromDate ?? to).Date;

            if (from > to)
            {
                return Result.Failure<OvertimeReportResponse>(Error.Validation("Overtime.InvalidDateRange", "FromDate cannot be after ToDate."));
            }

            var response = await _attendanceReportingService.GetOvertimeReportAsync(
                from,
                to,
                request.EmployeeId,
                request.OvertimeAfterHours,
                request.StandardMonthlyHours,
                cancellationToken);

            return Result.Success(response);
        }
    }
}
