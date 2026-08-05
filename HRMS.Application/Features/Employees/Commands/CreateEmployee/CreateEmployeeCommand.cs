using AutoMapper;
using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Results;
using HRMS.Application.Common.Validation;
using HRMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Features.Employees.Commands.CreateEmployee
{
    /// <summary>
    /// Command request to register a new employee.
    /// </summary>
    public record CreateEmployeeCommand(
        string? Name,
        string? Email,
        string? Mobile,
        decimal Salary,
        int DepartmentId,
        DateTime JoinDate,
        string? Designation,
        string? Address) : IRequest<Result<Employee>>;

    /// <summary>
    /// Validator enforcing rules for employee registration.
    /// </summary>
    public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
    {
        public CreateEmployeeCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Employee name is required.")
                .MaximumLength(100).WithMessage("Employee name must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .MustBeValidEmail();

            RuleFor(x => x.Mobile)
                .MustBeValidPhone();

            RuleFor(x => x.Salary)
                .MustBeValidSalary();

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Valid department assignment is required.");
        }
    }

    /// <summary>
    /// Command handler for creating a new employee using AutoMapper, cache invalidation, and real-time SignalR notification.
    /// </summary>
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<Employee>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CreateEmployeeCommandHandler> _logger;

        public CreateEmployeeCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            INotificationService notificationService,
            ILogger<CreateEmployeeCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Employee>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing CreateEmployeeCommand for Name: {Name}", request.Name);

            if (request.DepartmentId > 0)
            {
                var deptExists = await _unitOfWork.Departments.ExistsAsync(d => d.Id == request.DepartmentId, cancellationToken);
                if (!deptExists)
                {
                    return Result.Failure<Employee>(Error.Validation("Department.NotFound", $"Department ID {request.DepartmentId} does not exist."));
                }
            }

            var employee = _mapper.Map<Employee>(request);

            await _unitOfWork.Employees.AddAsync(employee, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            await _cacheService.RemoveAsync("employees:all", cancellationToken);
            await _cacheService.RemoveAsync("dashboard:hr:stats", cancellationToken);

            // Real-Time Notification
            await _notificationService.SendNotificationToAllAsync(
                "Employee Created",
                $"New employee '{employee.Name}' has joined the organization.",
                "EmployeeCreated",
                cancellationToken);

            return Result.Success(employee);
        }
    }
}
