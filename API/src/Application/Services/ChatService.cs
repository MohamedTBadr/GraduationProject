using Application.DTOs.MessageDTOs;
using Application.Hubs;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IMessageRepository _repo;
        private readonly ILogger<ChatService> _logger;
        private readonly IChatNotificationService _notifications;

        public ChatService(
            IMessageRepository repo,
            IChatNotificationService notifications,
            ILogger<ChatService> logger)
        {
            _repo = repo;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task<MessageDto> SendMessageAsync(Guid senderId, Guid receiverId, string content)
        {
            var conversation = await _repo.GetOrCreateConversationAsync(senderId, receiverId);
            var message = Message.Create(conversation.Id, senderId, receiverId, content);
            await _repo.AddMessageAsync(message);
            await _repo.SaveChangesAsync();

            var dto = MapToDto(message);
            await _notifications.SendMessageAsync(receiverId.ToString(), dto);

            _logger.LogInformation("Message sent from {SenderId} to {ReceiverId}", senderId, receiverId);
            return dto;
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(
            Guid userId, Guid otherUserId, int page, int pageSize)
        {
            var conversation = await _repo.GetConversationAsync(userId, otherUserId);
            if (conversation == null) return Enumerable.Empty<MessageDto>();

            var messages = await _repo.GetMessagesAsync(conversation.Id, page, pageSize);
            return messages.Select(MapToDto);
        }

        public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(Guid userId)
        {
            var conversations = await _repo.GetUserConversationsAsync(userId);

            return conversations.Select(c =>
            {
                var otherUser = c.User1Id == userId ? c.User2 : c.User1;
                var lastMessage = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                var unread = c.Messages.Count(m => m.ReceiverId == userId && !m.IsRead);

                return new ConversationDto(
                    c.Id,
                    otherUser.Id,
                    otherUser.UserName!,
                    lastMessage != null ? MapToDto(lastMessage) : null,
                    unread
                );
            });
        }

        public async Task MarkAsReadAsync(Guid messageId, Guid readerId)
        {
            var message = await _repo.GetMessageByIdAsync(messageId);
            if (message == null || message.ReceiverId != readerId) return;

            message.MarkAsRead();
            await _repo.SaveChangesAsync();
            await _notifications.NotifyMessageReadAsync(message.SenderId.ToString(), messageId);
        }

        private static MessageDto MapToDto(Message m) => new(
            m.Id, m.SenderId,
            m.Sender?.UserName ?? "Unknown",
            m.ReceiverId, m.Content,
            m.SentAt, m.IsRead, m.ReadAt
        );
    }
}