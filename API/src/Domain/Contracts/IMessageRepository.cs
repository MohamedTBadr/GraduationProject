using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts
{
    public interface IMessageRepository
    {
        Task<Conversation?> GetConversationAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken);
        Task<Conversation> GetOrCreateConversationAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken);
        Task<IEnumerable<Message>> GetMessagesAsync(Guid conversationId, int page, int pageSize, CancellationToken cancellationToken);
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken);
        Task AddMessageAsync(Message message, CancellationToken cancellationToken);
        Task<Message?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}

