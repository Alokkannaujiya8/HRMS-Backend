using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Results;
using HRMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Features.Employees.Queries.GetEmployeeById
{
    /// <summary>
    /// Query request to retrieve a single employee by unique identifier.
    /// </summary>
    public record GetEmployeeByIdQuery(int Id) : IRequest<Result<Employee>>;

    /// <summary>
    /// Query handler with Redis caching optimization for single employee.
    /// </summary>
    public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, Result<Employee>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetEmployeeByIdQueryHandler> _logger;

        public GetEmployeeByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<GetEmployeeByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Employee>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"employee:{request.Id}";
            _logger.LogInformation("Checking cache for key {CacheKey}", cacheKey);

            var cachedEmployee = await _cacheService.GetAsync<Employee>(cacheKey, cancellationToken);
            if (cachedEmployee != null)
            {
                return Result.Success(cachedEmployee);
            }

            _logger.LogInformation("Cache miss. Fetching employee ID {EmployeeId} from database.", request.Id);
            var employee = await _unitOfWork.Employees.GetByIdAsync(request.Id, cancellationToken);
            if (employee == null || !employee.IsActive)
            {
                return Result.Failure<Employee>(Error.NotFound("Employee.NotFound", $"Employee with ID {request.Id} was not found."));
            }

            await _cacheService.SetAsync(cacheKey, employee, TimeSpan.FromMinutes(30), null, cancellationToken);

            return Result.Success(employee);
        }
    }
}
