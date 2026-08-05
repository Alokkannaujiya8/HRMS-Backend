using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Features.Employees.Commands.DeleteEmployee
{
    /// <summary>
    /// Command request to soft-delete an employee.
    /// </summary>
    public record DeleteEmployeeCommand(int Id) : IRequest<Result>;

    /// <summary>
    /// Command handler for soft-deleting an employee with cache invalidation.
    /// </summary>
    public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<DeleteEmployeeCommandHandler> _logger;

        public DeleteEmployeeCommandHandler(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<DeleteEmployeeCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing DeleteEmployeeCommand for ID {EmployeeId}", request.Id);
            var existing = await _unitOfWork.Employees.GetByIdAsync(request.Id, cancellationToken);
            if (existing == null || !existing.IsActive)
            {
                return Result.Failure(Error.NotFound("Employee.NotFound", $"Employee with ID {request.Id} was not found."));
            }

            existing.IsActive = false;
            _unitOfWork.Employees.Update(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            await _cacheService.RemoveAsync("employees:all", cancellationToken);
            await _cacheService.RemoveAsync($"employee:{request.Id}", cancellationToken);
            await _cacheService.RemoveAsync("dashboard:hr:stats", cancellationToken);

            return Result.Success();
        }
    }
}
