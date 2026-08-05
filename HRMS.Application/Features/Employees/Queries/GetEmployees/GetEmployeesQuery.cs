using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Results;
using HRMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Features.Employees.Queries.GetEmployees
{
    /// <summary>
    /// Query request to retrieve all active employee records.
    /// </summary>
    public record GetEmployeesQuery : IRequest<Result<IReadOnlyList<Employee>>>;

    /// <summary>
    /// Query handler with Redis caching optimization for employee list.
    /// </summary>
    public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, Result<IReadOnlyList<Employee>>>
    {
        private const string CacheKey = "employees:all";
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetEmployeesQueryHandler> _logger;

        public GetEmployeesQueryHandler(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<GetEmployeesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<IReadOnlyList<Employee>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Checking cache for key {CacheKey}", CacheKey);
            var cachedEmployees = await _cacheService.GetAsync<IReadOnlyList<Employee>>(CacheKey, cancellationToken);
            if (cachedEmployees != null)
            {
                return Result.Success(cachedEmployees);
            }

            _logger.LogInformation("Cache miss. Fetching employees from database.");
            var employees = await _unitOfWork.Employees.FindAsync(x => x.IsActive, cancellationToken);
            await _cacheService.SetAsync(CacheKey, employees, TimeSpan.FromMinutes(15), null, cancellationToken);

            return Result.Success(employees);
        }
    }
}
