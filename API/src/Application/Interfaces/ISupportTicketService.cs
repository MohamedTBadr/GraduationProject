// ─── IISupportTicketService.cs ─────────────────────────────────────────────────

using Application.DTOs.Support;

namespace Application.Interfaces
{
    public interface ISupportTicketService
    {
        Task<Result<TicketDetailsDTO>>            CreateAsync(CreateTicketRequestDTO request, string fromUser, CancellationToken ct);
        Task<Result<TicketStatsDTO>>              GetStatsAsync(CancellationToken ct);
        Task<Result<PagedResult<TicketSummaryDTO>>> GetAllAsync(TicketQueryDTO query, CancellationToken ct);
        Task<Result<TicketDetailsDTO>>            GetByIdAsync(string ticketNumber, CancellationToken ct);
        Task<Result<TicketReplyResponseDTO>>      ReplyAsync(string ticketNumber, TicketReplyRequestDTO request, CancellationToken ct);
        Task<Result<TicketAssignResponseDTO>>     AssignAsync(string ticketNumber, TicketAssignRequestDTO request, CancellationToken ct);
        Task<Result<TicketResolveResponseDTO>>    ResolveAsync(string ticketNumber, TicketResolveRequestDTO request, CancellationToken ct);
        Task<Result<TicketEscalateResponseDTO>>   EscalateAsync(string ticketNumber, TicketEscalateRequestDTO request, CancellationToken ct);
    }
}



