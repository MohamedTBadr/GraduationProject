using Application.DTOs.MessageDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IChatService
    {
        Task<MessageDto> SendMessageAsync(Guid senderId, Guid receiverId, string content);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid userId, Guid otherUserId, int page, int pageSize);
        Task<IEnumerable<ConversationDto>> GetConversationsAsync(Guid userId);
        Task MarkAsReadAsync(Guid messageId, Guid readerId);
    }
}
