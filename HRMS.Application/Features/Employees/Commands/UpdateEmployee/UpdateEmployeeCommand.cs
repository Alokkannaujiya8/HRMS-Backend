using AutoMapper;
using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Results;
using HRMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Features.Employees.Commands.UpdateEmployee
{
    /// <summary>
    /// Command request to update an existing employee profile.
    /// </summary>
    public record UpdateEmployeeCommand(
        int Id,
        string? Name,
        string? Email,
        string? Mobile,
        decimal Salary,
        int DepartmentId,
        string? Designation,
        string? Address) : IRequest<Result>;

    /// <summary>
    /// Validator for UpdateEmployeeCommand.
    /// </summary>
    public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
    {
        public UpdateEmployeeCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Valid employee ID is required.");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Employee name cannot be empty.");
            RuleFor(x => x.Salary).GreaterThanOrEqualTo(0).WithMessage("Salary cannot be negative.");
        }
    }

    /// <summary>
    /// Command handler for updating employee details with cache invalidation.
    /// </summary>
    public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UpdateEmployeeCommandHandler> _logger;

        public UpdateEmployeeCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<UpdateEmployeeCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing UpdateEmployeeCommand for ID {EmployeeId}", request.Id);
            var existing = await _unitOfWork.Employees.GetByIdAsync(request.Id, cancellationToken);
            if (existing == null || !existing.IsActive)
            {
                return Result.Failure(Error.NotFound("Employee.NotFound", $"Employee with ID {request.Id} was not found."));
            }

            _mapper.Map(request, existing);

            _unitOfWork.Employees.Update(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            await _cacheService.RemoveAsync("employees:all", cancellationToken);
            await _cacheService.RemoveAsync($"employee:{request.Id}", cancellationToken);

            return Result.Success();
        }
    }
}
