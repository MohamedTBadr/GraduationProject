using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts
{
    public interface IMessageRepository
    {
        Task<Conversation?> GetConversationAsync(Guid user1Id, Guid user2Id);
        Task<Conversation> GetOrCreateConversationAsync(Guid user1Id, Guid user2Id);
        Task<IEnumerable<Message>> GetMessagesAsync(Guid conversationId, int page, int pageSize);
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId);
        Task AddMessageAsync(Message message);
        Task<Message?> GetMessageByIdAsync(Guid messageId);
        Task SaveChangesAsync();
    }
}

