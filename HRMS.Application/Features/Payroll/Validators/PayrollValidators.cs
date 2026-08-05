using FluentValidation;
using HRMS.Application.Common.Results;
using MediatR;

namespace HRMS.Application.Features.Payroll.Validators
{
    /// <summary>
    /// Command request to generate monthly payroll processing.
    /// </summary>
    public record GeneratePayrollCommand(int Month, int Year, int? DepartmentId) : IRequest<Result>;

    /// <summary>
    /// Validator enforcing monthly payroll generation rules.
    /// </summary>
    public class GeneratePayrollCommandValidator : AbstractValidator<GeneratePayrollCommand>
    {
        public GeneratePayrollCommandValidator()
        {
            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");

            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100).WithMessage("Valid four-digit processing year is required.");
        }
    }
}
