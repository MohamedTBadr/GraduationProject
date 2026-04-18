using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace Infrastructure.Repositories
{
    public class MessageRepository(
        ApplicationDbContext _context,
        ResiliencePipelineProvider<string> pipelineProvider) : IMessageRepository
    {
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("db-pipeline");

        public async Task<Conversation?> GetConversationAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken) =>
            await _pipeline.ExecuteAsync(async token =>
                await _context.Conversations
                    .Include(c => c.User1)
                    .Include(c => c.User2)
                    .FirstOrDefaultAsync(c =>
                        (c.User1Id == user1Id && c.User2Id == user2Id) ||
                        (c.User1Id == user2Id && c.User2Id == user1Id), token),
                cancellationToken);

        public async Task<Conversation> GetOrCreateConversationAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                // We use the context directly here because we are already inside a pipeline execution
                var existing = await _context.Conversations
                    .Include(c => c.User1)
                    .Include(c => c.User2)
                    .FirstOrDefaultAsync(c =>
                        (c.User1Id == user1Id && c.User2Id == user2Id) ||
                        (c.User1Id == user2Id && c.User2Id == user1Id), token);

                if (existing != null) return existing;

                var conversation = Conversation.Create(user1Id, user2Id);
                await _context.Conversations.AddAsync(conversation, token);
                await _context.SaveChangesAsync(token);
                return conversation;
            }, cancellationToken);
        }

        public async Task<IEnumerable<Message>> GetMessagesAsync(Guid conversationId, int page, int pageSize, CancellationToken cancellationToken) =>
            await _pipeline.ExecuteAsync(async token =>
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
                    .ToListAsync(token),
                cancellationToken);

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken) =>
            await _pipeline.ExecuteAsync(async token =>
                await _context.Conversations
                    .Where(c => c.User1Id == userId || c.User2Id == userId)
                    .Include(c => c.User1)
                    .Include(c => c.User2)
                    .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.SentAt))
                    .ToListAsync(token),
                cancellationToken);

        public async Task AddMessageAsync(Message message, CancellationToken cancellationToken) =>
            await _pipeline.ExecuteAsync(async token =>
            {
                await _context.Messages.AddAsync(message, token);
                // Note: If your pattern requires an explicit SaveChangesAsync call later, 
                // you can remove SaveChangesAsync from here.
                await _context.SaveChangesAsync(token);
            }, cancellationToken);

        public async Task<Message?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken) =>
            await _pipeline.ExecuteAsync(async token =>
                await _context.Messages.FindAsync([messageId], token),
                cancellationToken);

        public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
            await _pipeline.ExecuteAsync(async token =>
                await _context.SaveChangesAsync(token),
                cancellationToken);
    }
}