namespace PAL.Hubs
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;

    [Authorize] // Protect Hub
    public class ChatHub : Hub
    {
        // Triggered when a client connects
        public override Task OnConnectedAsync()
        {
            var name = Context.User?.Identity?.Name;
            Console.WriteLine($"Connected: {name}");
            return base.OnConnectedAsync();
        }

        // Broadcast message to all users
        public async Task SendMessage(string message)
        {
            var sender = Context.User.Identity.Name;
            await Clients.All.SendAsync("ReceiveMessage", sender, message);
        }

        // =======================
        // 🚀 GROUP CHAT METHODS
        // =======================

        // Join a group (room)
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName)
                .SendAsync("SystemMessage", $"{Context.User.Identity.Name} joined {groupName}");
        }

        // Leave a group
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName)
                .SendAsync("SystemMessage", $"{Context.User.Identity.Name} left {groupName}");
        }

        // Send a message to a group
        public async Task SendMessageToGroup(string groupName, string message)
        {
            var sender = Context.User.Identity.Name;

            await Clients.Group(groupName)
                .SendAsync("ReceiveGroupMessage", groupName, sender, message);
        }
    }

}
