namespace Domain.Entities
{
    // ─── AGENT ───────────────────────────────────────────────────────────────────

    public class SupportAgent
    {
        public Guid                          Id        { get; set; }
        public string                        AgentCode { get; set; } = default!;  // e.g. AGT-012
        public string                        Name      { get; set; } = default!;
        public string                        Email     { get; set; } = default!;

        // ── Navigation ───────────────────────────────────────────────────────────
        public ICollection<SupportTicket>    Tickets   { get; set; } = [];
    }
}
