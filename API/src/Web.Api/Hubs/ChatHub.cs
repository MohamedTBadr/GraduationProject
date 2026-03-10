// Web.Api/Hubs/ChatHub.cs
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Web.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("User connected: {UserId}", Context.UserIdentifier);
            await Clients.Others.SendAsync("UserOnline", Context.UserIdentifier);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("User disconnected: {UserId}", Context.UserIdentifier);
            await Clients.Others.SendAsync("UserOffline", Context.UserIdentifier);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendPrivateMessage(Guid receiverId, string content)
        {
            var senderId = Guid.Parse(Context.UserIdentifier!);
            var message = await _chatService.SendMessageAsync(senderId, receiverId, content);
            // Echo back to sender too (for multi-device)
            await Clients.Caller.SendAsync("ReceivePrivateMessage", message);
        }

        public async Task MarkMessageAsRead(Guid messageId)
        {
            var userId = Guid.Parse(Context.UserIdentifier!);
            await _chatService.MarkAsReadAsync(messageId, userId);
        }

        public async Task Typing(string toUserId)
        {
            await Clients.User(toUserId)
                .SendAsync("UserTyping", Context.UserIdentifier);
        }
    }
}