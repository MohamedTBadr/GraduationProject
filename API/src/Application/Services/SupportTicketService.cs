
using Application;
using Application.DTOs.Support;

using Application.Interfaces;
using Domain.Contracts;
using Domain.Entities;
using Domain.Enums;


namespace Application.Services
{
    public class SupportTicketService(ISupportTicketRepository repository) : ISupportTicketService
    {
        // ── Stats ────────────────────────────────────────────────────────────────

        public async Task<Result<TicketDetailsDTO>> CreateAsync(CreateTicketRequestDTO request, string fromUser, CancellationToken ct)
        {
            var ticketCount = await repository.CountByStatusAsync(TicketStatus.Open, ct); 
            // In a real system, you might want to generate a better unique standard ticket number
            var ticketNumber = $"TK-{DateTime.UtcNow.ToString("yyyyMMdd")}-{new Random().Next(1000, 9999)}";

            var ticket = new SupportTicket
            {
                Id = Guid.NewGuid(),
                TicketNumber = ticketNumber,
                Title = request.Title,
                Description = request.Description,
                From = fromUser,
                Type = ParseEnum<TicketType>(request.Type) ?? TicketType.Client,
                Priority = ParseEnum<TicketPriority>(request.Priority) ?? TicketPriority.Low,
                Status = TicketStatus.Open,
                OpenedAt = DateTime.UtcNow,
                BookingRef = request.BookingRef
            };

            await repository.AddAsync(ticket, ct);
            await repository.SaveChangesAsync(ct);

            return Result<TicketDetailsDTO>.Success(ToDetailsDTO(ticket));
        }

        public async Task<Result<TicketStatsDTO>> GetStatsAsync(CancellationToken ct)
        {
            var critical       = await repository.CountByPriorityAsync(TicketPriority.Critical, ct);
            var open           = await repository.CountByStatusAsync(TicketStatus.Open, ct);
            var inProgress     = await repository.CountByStatusAsync(TicketStatus.InProgress, ct);
            var resolutionRate = await repository.GetResolutionRateAsync(ct);

            return Result<TicketStatsDTO>.Success(new TicketStatsDTO(
                critical,
                open,
                inProgress,
                resolutionRate
            ));
        }

        // ── List ─────────────────────────────────────────────────────────────────

        public async Task<Result<PagedResult<TicketSummaryDTO>>> GetAllAsync(
            TicketQueryDTO query,
            CancellationToken ct)
        {
            var status   = ParseEnum<TicketStatus>(query.Status);
            var priority = ParseEnum<TicketPriority>(query.Priority);
            var type     = ParseEnum<TicketType>(query.Type);

            var (items, total) = await repository.GetAllAsync(
                status, priority, type,
                query.Page, query.Limit, ct);

            var dtos = items.Select(ToSummaryDTO).ToList();

            return Result<PagedResult<TicketSummaryDTO>>.Success(new PagedResult<TicketSummaryDTO>
            {
                Total = total,
                Page  = query.Page,
                Limit = query.Limit,
                Data  = dtos
            });
        }

        // ── Get By ID ────────────────────────────────────────────────────────────

        public async Task<Result<TicketDetailsDTO>> GetByIdAsync(string ticketNumber, CancellationToken ct)
        {
            var ticket = await repository.GetByTicketNumberAsync(ticketNumber, ct);
            if (ticket is null)
                return Result<TicketDetailsDTO>.Failure(
                    Error.NotFound(404, $"Ticket {ticketNumber} not found"));

            return Result<TicketDetailsDTO>.Success(ToDetailsDTO(ticket));
        }

        // ── Reply ────────────────────────────────────────────────────────────────

        public async Task<Result<TicketReplyResponseDTO>> ReplyAsync(
            string ticketNumber,
            TicketReplyRequestDTO request,
            CancellationToken ct)
        {
            var ticket = await repository.GetByTicketNumberAsync(ticketNumber, ct);
            if (ticket is null)
                return Result<TicketReplyResponseDTO>.Failure(
                    Error.NotFound(404, $"Ticket {ticketNumber} not found"));

            var replyCount  = ticket.Replies.Count + 1;
            var replyNumber = $"RPL-{replyCount:D3}";

            var reply = new TicketReply
            {
                Id           = Guid.NewGuid(),
                ReplyNumber  = replyNumber,
                Message      = request.Message,
                RepliedBy    = "Super Admin",           // replace with current user from HttpContext
                SentViaEmail = request.SendEmail,
                SentViaSms   = request.SendSms,
                RepliedAt    = DateTime.UtcNow,
                TicketId     = ticket.Id
            };

            await repository.AddReplyAsync(reply, ct);
            await repository.SaveChangesAsync(ct);

            var notifiedVia = new List<string>();
            if (request.SendEmail) notifiedVia.Add("email");
            if (request.SendSms)   notifiedVia.Add("sms");

            return Result<TicketReplyResponseDTO>.Success(new TicketReplyResponseDTO(
                ticketNumber,
                replyNumber,
                reply.Message,
                reply.RepliedBy,
                reply.RepliedAt,
                notifiedVia
            ));
        }

        // ── Assign ───────────────────────────────────────────────────────────────

        public async Task<Result<TicketAssignResponseDTO>> AssignAsync(
            string ticketNumber,
            TicketAssignRequestDTO request,
            CancellationToken ct)
        {
            var ticket = await repository.GetByTicketNumberAsync(ticketNumber, ct);
            if (ticket is null)
                return Result<TicketAssignResponseDTO>.Failure(
                    Error.NotFound(404, $"Ticket {ticketNumber} not found"));

            var agent = await repository.GetAgentByCodeAsync(request.AgentId, ct);
            if (agent is null)
                return Result<TicketAssignResponseDTO>.Failure(
                    Error.NotFound(404, $"Agent {request.AgentId} not found"));

            ticket.AssignedAgentId = agent.Id;
            ticket.AssignmentNote  = request.Note;
            ticket.AssignedAt      = DateTime.UtcNow;
            ticket.Status          = TicketStatus.InProgress;

            await repository.UpdateAsync(ticket, ct);
            await repository.SaveChangesAsync(ct);

            return Result<TicketAssignResponseDTO>.Success(new TicketAssignResponseDTO(
                ticketNumber,
                TicketStatus.InProgress.ToString(),
                new AgentDTO(agent.AgentCode, agent.Name),
                ticket.AssignedAt.Value
            ));
        }

        // ── Resolve ──────────────────────────────────────────────────────────────

        public async Task<Result<TicketResolveResponseDTO>> ResolveAsync(
            string ticketNumber,
            TicketResolveRequestDTO request,
            CancellationToken ct)
        {
            var ticket = await repository.GetByTicketNumberAsync(ticketNumber, ct);
            if (ticket is null)
                return Result<TicketResolveResponseDTO>.Failure(
                    Error.NotFound(404, $"Ticket {ticketNumber} not found"));

            if (ticket.Status == TicketStatus.Resolved)
                return Result<TicketResolveResponseDTO>.Failure(
                    Error.BusinessRule(400, "Ticket is already resolved"));

            ticket.Status         = TicketStatus.Resolved;
            ticket.ResolvedAt     = DateTime.UtcNow;
            ticket.ResolvedBy     = "Super Admin";      // replace with current user from HttpContext
            ticket.ResolutionNote = request.ResolutionNote;

            await repository.UpdateAsync(ticket, ct);
            await repository.SaveChangesAsync(ct);

            return Result<TicketResolveResponseDTO>.Success(new TicketResolveResponseDTO(
                ticketNumber,
                TicketStatus.Resolved.ToString(),
                ticket.ResolvedBy,
                ticket.ResolvedAt.Value,
                ticket.ResolutionNote
            ));
        }

        // ── Escalate ─────────────────────────────────────────────────────────────

        public async Task<Result<TicketEscalateResponseDTO>> EscalateAsync(
            string ticketNumber,
            TicketEscalateRequestDTO request,
            CancellationToken ct)
        {
            var ticket = await repository.GetByTicketNumberAsync(ticketNumber, ct);
            if (ticket is null)
                return Result<TicketEscalateResponseDTO>.Failure(
                    Error.NotFound(404, $"Ticket {ticketNumber} not found"));

            if (ticket.IsEscalated)
                return Result<TicketEscalateResponseDTO>.Failure(
                    Error.BusinessRule(400, "Ticket has already been escalated"));
            ticket.IsEscalated      = true;
            ticket.EscalatedTo      = request.EscalateTo;
            ticket.EscalationReason = request.Reason;
            ticket.EscalatedBy      = "Super Admin";    // replace with current user from HttpContext
            ticket.EscalatedAt      = DateTime.UtcNow;
            ticket.FinanceNotified  = request.NotifyFinance;

            await repository.UpdateAsync(ticket, ct);
            await repository.SaveChangesAsync(ct);

            return Result<TicketEscalateResponseDTO>.Success(new TicketEscalateResponseDTO(
                ticketNumber,
                true,
                ticket.EscalatedTo,
                ticket.EscalatedBy,
                ticket.EscalatedAt.Value,
                ticket.FinanceNotified
            ));
        }

        // ─── Mappers ─────────────────────────────────────────────────────────────

        private static TicketSummaryDTO ToSummaryDTO(SupportTicket t) => new(
            t.TicketNumber,
            t.Title,
            t.From,
            t.Type.ToString(),
            t.Priority.ToString().ToLower(),
            t.Status.ToString().ToLower(),
            t.OpenedAt,
            t.Description,
            t.AssignedAgent?.Name
        );

        private static TicketDetailsDTO ToDetailsDTO(SupportTicket t) => new(
            t.TicketNumber,
            t.Title,
            t.From,
            t.Type.ToString(),
            t.Priority.ToString().ToLower(),
            t.Status.ToString().ToLower(),
            t.OpenedAt,
            t.Description,
            t.BookingRef,
            t.AssignedAgent?.Name,
            t.ResolvedAt,
            t.Replies.Select(r => new TicketReplyDTO(
                r.ReplyNumber,
                r.Message,
                r.RepliedBy,
                r.RepliedAt
            )).ToList()
        );

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private static TEnum? ParseEnum<TEnum>(string? value) where TEnum : struct, Enum
            => Enum.TryParse<TEnum>(value, ignoreCase: true, out var result) ? result : null;
    }
}
