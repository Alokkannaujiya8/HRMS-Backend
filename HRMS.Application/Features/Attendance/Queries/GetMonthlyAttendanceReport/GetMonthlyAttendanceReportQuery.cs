using HRMS.Application.Common.Results;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Features.Attendance.Queries.GetMonthlyAttendanceReport
{
    public record GetMonthlyAttendanceReportQuery(
        int Year,
        int Month,
        int? EmployeeId,
        double OvertimeAfterHours = 8,
        decimal StandardMonthlyHours = 208,
        string LateAfter = "09:15") : IRequest<Result<MonthlyAttendanceReportResponse>>;

    public class GetMonthlyAttendanceReportQueryHandler : IRequestHandler<GetMonthlyAttendanceReportQuery, Result<MonthlyAttendanceReportResponse>>
    {
        private readonly IAttendanceReportingService _attendanceReportingService;
        private readonly ILogger<GetMonthlyAttendanceReportQueryHandler> _logger;

        public GetMonthlyAttendanceReportQueryHandler(
            IAttendanceReportingService attendanceReportingService,
            ILogger<GetMonthlyAttendanceReportQueryHandler> logger)
        {
            _attendanceReportingService = attendanceReportingService ?? throw new ArgumentNullException(nameof(attendanceReportingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<MonthlyAttendanceReportResponse>> Handle(GetMonthlyAttendanceReportQuery request, CancellationToken cancellationToken)
        {
            if (request.Year < 2000 || request.Month is < 1 or > 12)
            {
                return Result.Failure<MonthlyAttendanceReportResponse>(Error.Validation("MonthlyReport.InvalidYearMonth", "Valid year and month are required."));
            }

            var lateCutoff = TimeSpan.TryParse(request.LateAfter, out var time) ? time : new TimeSpan(9, 15, 0);

            _logger.LogInformation("Generating monthly report for {Year}/{Month}", request.Year, request.Month);
            var response = await _attendanceReportingService.GetMonthlyReportAsync(
                request.Year,
                request.Month,
                request.EmployeeId,
                request.OvertimeAfterHours,
                lateCutoff,
                request.StandardMonthlyHours);

            return Result.Success(response);
        }
    }
}
