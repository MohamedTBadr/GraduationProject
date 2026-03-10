using Application.DTOs.MessageDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Hubs
{
    public interface IChatNotificationService
    {
        Task SendMessageAsync(string userId, MessageDto message);
        Task NotifyMessageReadAsync(string userId, Guid messageId);
    }
}
