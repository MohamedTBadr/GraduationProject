namespace Domain.Entities
{
    // ─── REPLY ───────────────────────────────────────────────────────────────────

    public class TicketReply
    {
        public Guid     Id              { get; set; }
        public string   ReplyNumber     { get; set; } = default!;   // e.g. RPL-001
        public string   Message         { get; set; } = default!;
        public string   RepliedBy       { get; set; } = default!;
        public bool     SentViaEmail    { get; set; }
        public bool     SentViaSms      { get; set; }
        public DateTime RepliedAt       { get; set; }

        // ── FK ───────────────────────────────────────────────────────────────────
        public Guid          TicketId   { get; set; }
        public SupportTicket Ticket     { get; set; } = default!;
    }
}
