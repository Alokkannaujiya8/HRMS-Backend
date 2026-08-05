using Microsoft.AspNetCore.SignalR;

namespace HRMS.API.Hubs
{
    /// <summary>
    /// SignalR Hub endpoint handling client connections and group assignments for real-time HRMS notifications.
    /// </summary>
    public class HrmsHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, "AllUsers");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AllUsers");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
