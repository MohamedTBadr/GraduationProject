using BLL.DTOs;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using PAL.Hubs;

namespace PAL.Notifications
{
    public class SignalRNotificationPublisher : INotificationPublisher
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationPublisher(
            IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PublishAsync(NotificationMessage message)
        {
            await _hubContext
                .Clients
                .Group(message.RecipientId)
                .SendAsync("ReceiveNotification", message);
        }
    }

}
