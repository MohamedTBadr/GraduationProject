using BLL.DTOs;
using BLL.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class EventService
    {
        private readonly INotificationPublisher _notificationPublisher;

        public EventService(INotificationPublisher notificationPublisher)
        {
            _notificationPublisher = notificationPublisher;
        }

        public async Task InviteGuestAsync(int eventId, int guestId)
        {
            // business rules
            // persistence
            // validations

            await _notificationPublisher.PublishAsync(new NotificationMessage
            {
                RecipientId = guestId.ToString(),
                Type = "GuestInvited",
                Title = "You're invited!",
                Body = "You have been invited to an event."
            });
        }
    }

}
