using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendAsync(string userId, object payload)
        {
            await _hubContext.Clients
                .Group(userId)
                .SendAsync("ReceiveNotification", payload);
        }
    }
}
