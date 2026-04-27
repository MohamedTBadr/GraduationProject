using Domain.Enums;

namespace Domain.Entities
{
    // ─── TICKET ──────────────────────────────────────────────────────────────────

    public class SupportTicket
    {
        public Guid             Id               { get; set; }
        public string           TicketNumber     { get; set; } = default!;   // e.g. TK-889
        public string           Title            { get; set; } = default!;
        public string           Description      { get; set; } = default!;
        public string           From             { get; set; } = default!;   // submitter name
        public TicketType       Type             { get; set; }
        public TicketPriority   Priority         { get; set; }
        public TicketStatus     Status           { get; set; }
        public string?          BookingRef       { get; set; }
        public DateTime         OpenedAt         { get; set; }
        public DateTime?        ResolvedAt       { get; set; }
        public string?          ResolutionNote   { get; set; }
        public string?          ResolvedBy       { get; set; }

        // ── Assign ───────────────────────────────────────────────────────────────
        public Guid?            AssignedAgentId  { get; set; }
        public SupportAgent?    AssignedAgent    { get; set; }
        public string?          AssignmentNote   { get; set; }
        public DateTime?        AssignedAt       { get; set; }

        // ── Escalation ───────────────────────────────────────────────────────────
        public bool             IsEscalated      { get; set; }
        public string?          EscalatedTo      { get; set; }
        public string?          EscalatedBy      { get; set; }
        public string?          EscalationReason { get; set; }
        public bool             FinanceNotified  { get; set; }
        public DateTime?        EscalatedAt      { get; set; }

        // ── Navigation ───────────────────────────────────────────────────────────
        public ICollection<TicketReply> Replies  { get; set; } = [];
    }
}


