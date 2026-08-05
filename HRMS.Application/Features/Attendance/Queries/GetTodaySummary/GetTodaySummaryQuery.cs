using HRMS.Application.Common.Results;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using MediatR;

namespace HRMS.Application.Features.Attendance.Queries.GetTodaySummary
{
    public record GetTodaySummaryQuery(
        DateTime? Date,
        double OvertimeAfterHours = 8,
        decimal StandardMonthlyHours = 208,
        string LateAfter = "09:15") : IRequest<Result<AttendanceSummaryResponse>>;

    public class GetTodaySummaryQueryHandler : IRequestHandler<GetTodaySummaryQuery, Result<AttendanceSummaryResponse>>
    {
        private readonly IAttendanceReportingService _attendanceReportingService;

        public GetTodaySummaryQueryHandler(IAttendanceReportingService attendanceReportingService)
        {
            _attendanceReportingService = attendanceReportingService ?? throw new ArgumentNullException(nameof(attendanceReportingService));
        }

        public async Task<Result<AttendanceSummaryResponse>> Handle(GetTodaySummaryQuery request, CancellationToken cancellationToken)
        {
            var reportDate = (request.Date ?? DateTime.UtcNow).Date;
            var lateCutoff = TimeSpan.TryParse(request.LateAfter, out var time) ? time : new TimeSpan(9, 15, 0);

            var response = await _attendanceReportingService.GetTodaySummaryAsync(
                reportDate,
                request.OvertimeAfterHours,
                lateCutoff,
                request.StandardMonthlyHours,
                cancellationToken);

            return Result.Success(response);
        }
    }
}
