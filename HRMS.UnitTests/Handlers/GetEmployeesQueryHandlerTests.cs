using FluentAssertions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Employees.Queries.GetEmployees;
using HRMS.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HRMS.UnitTests.Handlers
{
    public class GetEmployeesQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<GetEmployeesQueryHandler>> _mockLogger;
        private readonly GetEmployeesQueryHandler _handler;

        public GetEmployeesQueryHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<GetEmployeesQueryHandler>>();

            _handler = new GetEmployeesQueryHandler(
                _mockUnitOfWork.Object,
                _mockCacheService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnCachedEmployees_WhenCacheHit()
        {
            // Arrange
            var cachedEmployees = new List<Employee>
            {
                new() { Id = 1, Name = "John Doe", Email = "john@example.com", IsActive = true }
            };

            _mockCacheService
                .Setup(c => c.GetAsync<IReadOnlyList<Employee>>("employees:all", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedEmployees);

            // Act
            var result = await _handler.Handle(new GetEmployeesQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value![0].Name.Should().Be("John Doe");

            _mockUnitOfWork.Verify(u => u.Employees.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
