using FluentValidation;
using HRMS.Application.Features.Attendance.Queries.GetMonthlyAttendanceReport;
using HRMS.Application.Features.Attendance.Queries.GetOvertimeReport;

namespace HRMS.Application.Features.Attendance.Validators
{
    /// <summary>
    /// FluentValidation validator for GetOvertimeReportQuery executing via MediatR ValidationBehavior.
    /// </summary>
    public class GetOvertimeReportQueryValidator : AbstractValidator<GetOvertimeReportQuery>
    {
        public GetOvertimeReportQueryValidator()
        {
            RuleFor(x => x.FromDate)
                .LessThanOrEqualTo(x => x.ToDate.HasValue ? x.ToDate.Value : DateTime.UtcNow)
                .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
                .WithMessage("FromDate cannot be after ToDate.");

            RuleFor(x => x.OvertimeAfterHours)
                .GreaterThan(0).WithMessage("OvertimeAfterHours must be greater than zero.");
        }
    }

    /// <summary>
    /// FluentValidation validator for GetMonthlyAttendanceReportQuery executing via MediatR ValidationBehavior.
    /// </summary>
    public class GetMonthlyAttendanceReportQueryValidator : AbstractValidator<GetMonthlyAttendanceReportQuery>
    {
        public GetMonthlyAttendanceReportQueryValidator()
        {
            RuleFor(x => x.Year)
                .GreaterThanOrEqualTo(2000).WithMessage("Valid year (2000 or later) is required.");

            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12).WithMessage("Valid month (1 to 12) is required.");

            RuleFor(x => x.OvertimeAfterHours)
                .GreaterThan(0).WithMessage("OvertimeAfterHours must be greater than zero.");
        }
    }
}
