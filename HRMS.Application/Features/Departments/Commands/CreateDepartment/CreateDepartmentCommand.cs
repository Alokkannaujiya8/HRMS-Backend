using AutoMapper;
using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Results;
using HRMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Features.Departments.Commands.CreateDepartment
{
    /// <summary>
    /// Command to create a new department.
    /// </summary>
    public record CreateDepartmentCommand(string Name, string? Code = null) : IRequest<Result<Department>>;

    public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
    {
        public CreateDepartmentCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Department name is required.")
                .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters.");
        }
    }

    public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<Department>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CreateDepartmentCommandHandler> _logger;

        public CreateDepartmentCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            INotificationService notificationService,
            ILogger<CreateDepartmentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Department>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing CreateDepartmentCommand for Name: {Name}", request.Name);

            var dept = _mapper.Map<Department>(request);

            await _unitOfWork.Departments.AddAsync(dept, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            await _cacheService.RemoveAsync("departments:all", cancellationToken);

            // Real-Time Notification
            await _notificationService.SendNotificationToAllAsync(
                "Department Updated",
                $"Department '{dept.Name}' was created/updated.",
                "DepartmentUpdated",
                cancellationToken);

            return Result.Success(dept);
        }
    }
}
