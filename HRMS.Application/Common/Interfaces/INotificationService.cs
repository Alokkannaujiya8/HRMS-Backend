namespace HRMS.Application.Common.Interfaces
{
    /// <summary>
    /// Contract for broadcasting real-time push notifications across connected SignalR clients.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends a real-time notification to all connected clients.
        /// </summary>
        Task SendNotificationToAllAsync(string title, string message, string type, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a targeted real-time notification to a specific user.
        /// </summary>
        Task SendNotificationToUserAsync(string userId, string title, string message, string type, CancellationToken cancellationToken = default);
    }
}
