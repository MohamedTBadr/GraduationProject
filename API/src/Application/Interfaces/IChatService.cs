using Application.DTOs.MessageDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IChatService
    {
        Task<MessageDto> SendMessageAsync(Guid senderId, Guid receiverId, string content, CancellationToken cancellationToken);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid userId, Guid otherUserId, int page, int pageSize, CancellationToken cancellationToken);
        Task<IEnumerable<ConversationDto>> GetConversationsAsync(Guid userId, CancellationToken cancellationToken);
        Task MarkAsReadAsync(Guid messageId, Guid readerId, CancellationToken cancellationToken);
    }
}
