// ─── IISupportTicketRepository.cs ─────────────────────────────────────────────

using Domain.Entities;
using Domain.Enums;

namespace Domain.Contracts
{
    public interface ISupportTicketRepository
    {
        // ── Queries ──────────────────────────────────────────────────────────────
        Task<SupportTicket?>              GetByTicketNumberAsync(string ticketNumber, CancellationToken ct);
        Task<(IReadOnlyList<SupportTicket> Items, int Total)> GetAllAsync(
            TicketStatus?   status,
            TicketPriority? priority,
            TicketType?     type,
            int             page,
            int             limit,
            CancellationToken ct);

        Task<int> CountByStatusAsync(TicketStatus status, CancellationToken ct);
        Task<int> CountByPriorityAsync(TicketPriority priority, CancellationToken ct);
        Task<int> GetResolutionRateAsync(CancellationToken ct);

        // ── Agent ────────────────────────────────────────────────────────────────
        Task<SupportAgent?> GetAgentByCodeAsync(string agentCode, CancellationToken ct);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task AddReplyAsync(TicketReply reply, CancellationToken ct);
        Task UpdateAsync(SupportTicket ticket, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}


