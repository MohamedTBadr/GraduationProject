namespace Common.Hubs
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;

    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("id")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }
    }
}
