


// ─── SupportTicketRepository.cs ──────────────────────────────────────────────

using Domain.Contracts;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SupportTicketRepository(ApplicationDbContext context) : ISupportTicketRepository
    {
        // ── Queries ──────────────────────────────────────────────────────────────

        public async Task<SupportTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken ct)
            => await context.SupportTickets
                .Include(t => t.AssignedAgent)
                .Include(t => t.Replies)
                .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber, ct);

        public async Task<(IReadOnlyList<SupportTicket> Items, int Total)> GetAllAsync(
            TicketStatus?   status,
            TicketPriority? priority,
            TicketType?     type,
            int             page,
            int             limit,
            CancellationToken ct)
        {
            var query = context.SupportTickets
                .Include(t => t.AssignedAgent)
                .AsQueryable();

            if (status   is not null) query = query.Where(t => t.Status   == status);
            if (priority is not null) query = query.Where(t => t.Priority == priority);
            if (type     is not null) query = query.Where(t => t.Type     == type);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(t => t.OpenedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync(ct);

            return (items, total);
        }

        public Task<int> CountByStatusAsync(TicketStatus status, CancellationToken ct)
            => context.SupportTickets.CountAsync(t => t.Status == status, ct);

        public Task<int> CountByPriorityAsync(TicketPriority priority, CancellationToken ct)
            => context.SupportTickets.CountAsync(t => t.Priority == priority, ct);

        public async Task<int> GetResolutionRateAsync(CancellationToken ct)
        {
            var total    = await context.SupportTickets.CountAsync(ct);
            if (total == 0) return 0;

            var resolved = await context.SupportTickets
                .CountAsync(t => t.Status == TicketStatus.Resolved, ct);

            return (int)Math.Round((double)resolved / total * 100);
        }

        public Task<SupportAgent?> GetAgentByCodeAsync(string agentCode, CancellationToken ct)
            => context.SupportAgents.FirstOrDefaultAsync(a => a.AgentCode == agentCode, ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task AddReplyAsync(TicketReply reply, CancellationToken ct)
            => await context.TicketReplies.AddAsync(reply, ct);

        public Task UpdateAsync(SupportTicket ticket, CancellationToken ct)
        {
            context.SupportTickets.Update(ticket);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct)
            => context.SaveChangesAsync(ct);
    }
}
