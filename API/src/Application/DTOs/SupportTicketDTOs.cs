using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Support
{
    // ─── STATS ───────────────────────────────────────────────────────────────────

    public record TicketStatsDTO(
        int Critical,
        int Open,
        int InProgress,
        int ResolutionRate
    );

    // ─── QUERY ───────────────────────────────────────────────────────────────────

    public class TicketQueryDTO
    {
        public string? Status   { get; set; }
        public string? Priority { get; set; }
        public string? Type     { get; set; }
        public int     Page     { get; set; } = 1;
        public int     Limit    { get; set; } = 20;
    }

    // ─── PAGED RESULT ────────────────────────────────────────────────────────────

    public class PagedResult<T>
    {
        public int        Total { get; set; }
        public int        Page  { get; set; }
        public int        Limit { get; set; }
        public IList<T>   Data  { get; set; } = [];
    }

    // ─── SUMMARY (list item) ─────────────────────────────────────────────────────

    public record TicketSummaryDTO(
        string  TicketId,
        string  Title,
        string  From,
        string  Type,
        string  Priority,
        string  Status,
        DateTime OpenedAt,
        string  Description,
        string? AssignedTo
    );

    // ─── DETAILS (single) ────────────────────────────────────────────────────────

    public record TicketDetailsDTO(
        string   TicketId,
        string   Title,
        string   From,
        string   Type,
        string   Priority,
        string   Status,
        DateTime OpenedAt,
        string   Description,
        string?  BookingRef,
        string?  AssignedTo,
        DateTime? ResolvedAt,
        IList<TicketReplyDTO> Replies
    );

    public record TicketReplyDTO(
        string   ReplyId,
        string   Message,
        string   RepliedBy,
        DateTime RepliedAt
    );

    // ─── REPLY REQUEST / RESPONSE ────────────────────────────────────────────────

    public class TicketReplyRequestDTO
    {
        [Required]
        public string  Message   { get; set; } = default!;
        public bool    SendEmail { get; set; } = true;
        public bool    SendSms   { get; set; } = false;
    }

    public record TicketReplyResponseDTO(
        string   TicketId,
        string   ReplyId,
        string   Message,
        string   RepliedBy,
        DateTime RepliedAt,
        IList<string> NotifiedVia
    );

    // ─── ASSIGN REQUEST / RESPONSE ───────────────────────────────────────────────

    public class TicketAssignRequestDTO
    {
        [Required]
        public string  AgentId { get; set; } = default!;
        public string? Note    { get; set; }
    }

    public record TicketAssignResponseDTO(
        string   TicketId,
        string   Status,
        AgentDTO AssignedTo,
        DateTime AssignedAt
    );

    public record AgentDTO(string AgentId, string Name);

    // ─── RESOLVE REQUEST / RESPONSE ──────────────────────────────────────────────

    public class TicketResolveRequestDTO
    {
        [Required]
        public string ResolutionNote { get; set; } = default!;
    }

    public record TicketResolveResponseDTO(
        string   TicketId,
        string   Status,
        string   ResolvedBy,
        DateTime ResolvedAt,
        string   ResolutionNote
    );

    // ─── ESCALATE REQUEST / RESPONSE ─────────────────────────────────────────────

    public class TicketEscalateRequestDTO
    {
        [Required]
        public string  Reason         { get; set; } = default!;

        [Required]
        public string  EscalateTo     { get; set; } = default!;  // senior_management | legal_team | cto
        public bool    NotifyFinance  { get; set; } = false;
    }

    public record TicketEscalateResponseDTO(
        string   TicketId,
        bool     Escalated,
        string   EscalatedTo,
        string   EscalatedBy,
        DateTime EscalatedAt,
        bool     FinanceNotified
    );
}
