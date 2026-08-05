using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Results;
using HRMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Features.Departments.Queries.GetDepartments
{
    /// <summary>
    /// Query to retrieve all departments with distributed caching.
    /// </summary>
    public record GetDepartmentsQuery : IRequest<Result<IReadOnlyList<Department>>>;

    public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, Result<IReadOnlyList<Department>>>
    {
        private const string CacheKey = "departments:all";
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetDepartmentsQueryHandler> _logger;

        public GetDepartmentsQueryHandler(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<GetDepartmentsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<IReadOnlyList<Department>>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Checking cache for key {CacheKey}", CacheKey);
            var cachedDepts = await _cacheService.GetAsync<IReadOnlyList<Department>>(CacheKey, cancellationToken);
            if (cachedDepts != null)
            {
                return Result.Success(cachedDepts);
            }

            _logger.LogInformation("Cache miss. Fetching departments from database.");
            var depts = await _unitOfWork.Departments.GetAllAsync(cancellationToken);
            await _cacheService.SetAsync(CacheKey, depts, TimeSpan.FromHours(1), null, cancellationToken);

            return Result.Success(depts);
        }
    }
}
