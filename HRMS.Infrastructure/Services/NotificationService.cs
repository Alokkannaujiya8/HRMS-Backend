using HRMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HRMS.Infrastructure.Services
{
    /// <summary>
    /// Notification service implementation broadcasting real-time events over SignalR HrmsHub.
    /// </summary>
    public class NotificationService<THub> : INotificationService where THub : Hub
    {
        private readonly IHubContext<THub> _hubContext;
        private readonly ILogger<NotificationService<THub>> _logger;

        public NotificationService(IHubContext<THub> hubContext, ILogger<NotificationService<THub>> logger)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendNotificationToAllAsync(string title, string message, string type, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Broadcasting SignalR notification: {Title} ({Type})", title, type);
            var payload = new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("ReceiveNotification", payload, cancellationToken);
        }

        public async Task SendNotificationToUserAsync(string userId, string title, string message, string type, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending targeted SignalR notification to User {UserId}: {Title} ({Type})", userId, title, type);
            var payload = new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", payload, cancellationToken);
        }
    }
}
