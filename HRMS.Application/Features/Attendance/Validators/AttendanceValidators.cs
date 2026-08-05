using FluentValidation;
using HRMS.Application.Common.Results;
using MediatR;

namespace HRMS.Application.Features.Attendance.Validators
{
    /// <summary>
    /// Command request to clock-in employee attendance.
    /// </summary>
    public record ClockInCommand(int EmployeeId, DateTime ClockInTime, string? Notes) : IRequest<Result>;

    /// <summary>
    /// Command request to clock-out employee attendance.
    /// </summary>
    public record ClockOutCommand(int EmployeeId, DateTime ClockOutTime, string? Notes) : IRequest<Result>;

    /// <summary>
    /// Validator for ClockInCommand.
    /// </summary>
    public class ClockInCommandValidator : AbstractValidator<ClockInCommand>
    {
        public ClockInCommandValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("Valid Employee ID is required.");

            RuleFor(x => x.ClockInTime)
                .NotEmpty().WithMessage("Clock-in timestamp is required.")
                .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5)).WithMessage("Clock-in timestamp cannot be in the future.");
        }
    }

    /// <summary>
    /// Validator for ClockOutCommand.
    /// </summary>
    public class ClockOutCommandValidator : AbstractValidator<ClockOutCommand>
    {
        public ClockOutCommandValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("Valid Employee ID is required.");

            RuleFor(x => x.ClockOutTime)
                .NotEmpty().WithMessage("Clock-out timestamp is required.");
        }
    }
}
