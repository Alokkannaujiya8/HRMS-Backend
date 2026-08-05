using FluentValidation;
using HRMS.Application.Common.Results;
using MediatR;

namespace HRMS.Application.Features.Leaves.Validators
{
    /// <summary>
    /// Command request to submit a leave request.
    /// </summary>
    public record SubmitLeaveRequestCommand(
        int EmployeeId,
        string LeaveType,
        DateTime StartDate,
        DateTime EndDate,
        string Reason) : IRequest<Result>;

    /// <summary>
    /// Validator enforcing leave application rules.
    /// </summary>
    public class SubmitLeaveRequestCommandValidator : AbstractValidator<SubmitLeaveRequestCommand>
    {
        public SubmitLeaveRequestCommandValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("Valid Employee ID is required.");

            RuleFor(x => x.LeaveType)
                .NotEmpty().WithMessage("Leave type is required.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Leave end date must be on or after start date.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason for leave is required.")
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
        }
    }
}
