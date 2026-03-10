using Application.DTOs.MessageDTOs;
using Application.Hubs;
using Microsoft.AspNetCore.SignalR;
using Web.Api.Hubs;

namespace Web.Api.Services
{
    public class ChatNotificationService : IChatNotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatNotificationService(IHubContext<ChatHub> hubContext)
            => _hubContext = hubContext;

        public Task SendMessageAsync(string userId, MessageDto message)
            => _hubContext.Clients.User(userId).SendAsync("ReceivePrivateMessage", message);

        public Task NotifyMessageReadAsync(string userId, Guid messageId)
            => _hubContext.Clients.User(userId).SendAsync("MessageRead", messageId);
    }
}