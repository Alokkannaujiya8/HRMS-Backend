using System.Security.Claims;
using FluentAssertions;
using HRMS.Application.DTOs;
using HRMS.Application.Features.Attendance.Commands.CheckIn;
using HRMS.Application.Features.Attendance.Commands.CheckOut;
using HRMS.Application.Interfaces;
using Moq;
using Xunit;

namespace HRMS.UnitTests.Handlers
{
    public class AttendanceCommandHandlerTests
    {
        private readonly Mock<IAttendanceService> _mockAttendanceService;
        private readonly CheckInCommandHandler _checkInHandler;
        private readonly CheckOutCommandHandler _checkOutHandler;

        public AttendanceCommandHandlerTests()
        {
            _mockAttendanceService = new Mock<IAttendanceService>();
            _checkInHandler = new CheckInCommandHandler(_mockAttendanceService.Object);
            _checkOutHandler = new CheckOutCommandHandler(_mockAttendanceService.Object);
        }

        [Fact]
        public async Task CheckIn_ShouldReturnSuccess_WhenCheckInSucceeds()
        {
            // Arrange
            var user = CreateUserPrincipal("user-100", "emp1");
            var expectedResponse = new AttendanceCheckInResponse
            {
                Id = 1,
                Message = "Check-in successful",
                CheckInTime = DateTime.UtcNow
            };

            _mockAttendanceService
                .Setup(s => s.ResolveEmployeeIdAsync("user-100", "emp1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);

            _mockAttendanceService
                .Setup(s => s.CheckInAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _checkInHandler.Handle(new CheckInCommand(user), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(1);
            result.Value.Message.Should().Be("Check-in successful");
        }

        [Fact]
        public async Task CheckIn_ShouldReturnFailure_WhenInvalidEmployee()
        {
            // Arrange
            var user = CreateUserPrincipal("user-invalid", "unknown");

            _mockAttendanceService
                .Setup(s => s.ResolveEmployeeIdAsync("user-invalid", "unknown", It.IsAny<CancellationToken>()))
                .ReturnsAsync((int?)null);

            // Act
            var result = await _checkInHandler.Handle(new CheckInCommand(user), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("Attendance.EmployeeNotFound");
            result.Error.Message.Should().Contain("Employee mapping not found");
        }

        [Fact]
        public async Task CheckIn_ShouldReturnFailure_WhenDuplicateCheckInThrowsException()
        {
            // Arrange
            var user = CreateUserPrincipal("user-duplicate", "emp_dup");

            _mockAttendanceService
                .Setup(s => s.ResolveEmployeeIdAsync("user-duplicate", "emp_dup", It.IsAny<CancellationToken>()))
                .ReturnsAsync(15);

            _mockAttendanceService
                .Setup(s => s.CheckInAsync(15, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Already checked in for today."));

            // Act
            Func<Task> act = async () => await _checkInHandler.Handle(new CheckInCommand(user), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Already checked in for today.");
        }

        [Fact]
        public async Task CheckOut_ShouldReturnSuccess_WhenCheckOutSucceeds()
        {
            // Arrange
            var user = CreateUserPrincipal("user-200", "emp2");
            var expectedResponse = new AttendanceCheckOutResponse
            {
                Id = 2,
                Message = "Check-out successful",
                CheckOutTime = DateTime.UtcNow,
                TotalHours = 8.5
            };

            _mockAttendanceService
                .Setup(s => s.ResolveEmployeeIdAsync("user-200", "emp2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(20);

            _mockAttendanceService
                .Setup(s => s.CheckOutAsync(20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _checkOutHandler.Handle(new CheckOutCommand(user), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(2);
            result.Value.TotalHours.Should().Be(8.5);
        }

        [Fact]
        public async Task CheckOut_ShouldReturnFailure_WhenInvalidEmployee()
        {
            // Arrange
            var user = CreateUserPrincipal("user-invalid", "unknown");

            _mockAttendanceService
                .Setup(s => s.ResolveEmployeeIdAsync("user-invalid", "unknown", It.IsAny<CancellationToken>()))
                .ReturnsAsync((int?)null);

            // Act
            var result = await _checkOutHandler.Handle(new CheckOutCommand(user), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("Attendance.EmployeeNotFound");
        }

        private static ClaimsPrincipal CreateUserPrincipal(string userId, string userName)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName)
            }, "TestAuthType"));
        }
    }
}
