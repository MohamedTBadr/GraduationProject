// ─── ENUMS ───────────────────────────────────────────────────────────────────

namespace Domain.Enums
{
    public enum TicketStatus
    {
        Open,
        InProgress,
        Resolved
    }

    public enum TicketPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum TicketType
    {
        Client,
        Vendor
    }

    public enum EscalationTarget
    {
        SeniorManagement,
        LegalTeam,
        Cto
    }
}