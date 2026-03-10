using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class MessageRepository(ApplicationDbContext _context) : IMessageRepository
    {
     
        public async Task<Conversation?> GetConversationAsync(Guid user1Id, Guid user2Id) =>
            await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .FirstOrDefaultAsync(c =>
                    (c.User1Id == user1Id && c.User2Id == user2Id) ||
                    (c.User1Id == user2Id && c.User2Id == user1Id));

        public async Task<Conversation> GetOrCreateConversationAsync(Guid user1Id, Guid user2Id)
        {
            var existing = await GetConversationAsync(user1Id, user2Id);
            if (existing != null) return existing;

            var conversation = Conversation.Create(user1Id, user2Id);
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task<IEnumerable<Message>> GetMessagesAsync(Guid conversationId, int page, int pageSize) =>
            await _context.Messages
                .Where(m =>
                    (_context.Conversations
                        .Where(c => c.Id == conversationId)
                        .Any(c => (c.User1Id == m.SenderId && c.User2Id == m.ReceiverId) ||
                                  (c.User1Id == m.ReceiverId && c.User2Id == m.SenderId))))
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(m => m.Sender)
                .ToListAsync();

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId) =>
            await _context.Conversations
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .Include(c => c.User1)
                .Include(c => c.User2)
                .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.SentAt))
                .ToListAsync();

        public async Task AddMessageAsync(Message message) =>
            await _context.Messages.AddAsync(message);

        public async Task<Message?> GetMessageByIdAsync(Guid messageId) =>
            await _context.Messages.FindAsync(messageId);

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
