using Domain.Entities;
using Domain.Enums;

namespace EpicHub.IntegrationTests.Infrastructure.Shared;

/// <summary>
/// Centralised factory for building valid test entities.
/// All IDs are generated fresh per call so tests never share state accidentally.
/// </summary>
internal static class EntityBuilders
{
    public static ApplicationUser BuildUser(string? email = null)
    {
        email ??= $"{Guid.NewGuid():N}@test.com";
        return new ApplicationUser
        {
            Id          = Guid.NewGuid(),
            UserName    = email,
            Email       = email,
            FirstName   = "Test",
            LastName    = "User"
        };
    }

    public static ApplicationUser BuildUserWithReferralCode(string email, string referralCode) =>
        new()
        {
            Id           = Guid.NewGuid(),
            UserName     = email,
            Email        = email,
            FirstName    = "Test",
            LastName     = "User",
            ReferralCode = referralCode
        };

    public static EventType BuildEventType(string name = "Wedding") =>
        new() { Id = Guid.NewGuid(), Name = name };

    public static VendorType BuildVendorType(string name = "Photography") =>
        new() { Id = Guid.NewGuid(), Name = name };

    public static ServiceType BuildServiceType(Guid vendorTypeId, string name = "Decor") =>
        new() { Id = Guid.NewGuid(), Name = name, VendorTypeId = vendorTypeId };

    public static Event BuildEvent(
        Guid userId,
        Guid eventTypeId,
        string title  = "Test Event",
        string status = EventStatuses.Planned) =>
        new()
        {
            Id          = Guid.NewGuid(),
            UserId      = userId,
            EventTypeId = eventTypeId,
            Title       = title,
            EventDate   = DateTime.UtcNow.AddDays(30),
            Location    = new Address { Street = "123 Main St", City = "Cairo", State = "Cairo" },
            TotalBudget = 10_000,
            GuestCount  = 100,
            Notes       = "Test notes",
            EventStatus = status
        };

    public static Vendor BuildVendor(Guid userId, Guid vendorTypeId) =>
        new()
        {
            UserId       = userId,
            VendorTypeId = vendorTypeId,
            BusinessName = "Test Vendor Co",
            Description  = "Test vendor description",
            ProfilePicture = string.Empty,
            PortfolioLink  = string.Empty,
            Address      = new Address { Street = "123 Main St", City = "Cairo", State = "Cairo" },
            Document     = string.Empty,
            Services     = [],
            Packages     = [],
            VendorRatings = [],
            ServiceAreas = []
        };

    public static Service BuildService(Guid vendorId, Guid serviceTypeId, decimal price = 200m) =>
        new()
        {
            Id            = Guid.NewGuid(),
            Name          = "Test Service",
            Description   = "Test service description",
            Price         = price,
            VendorId      = vendorId,
            ServiceTypeId = serviceTypeId,
            ServiceImages = [],
            EventTypes    = []
        };

    public static EventItem BuildEventItem(Guid eventId, Guid serviceId, int quantity = 3, decimal price = 200m) =>
        new()
        {
            Id        = Guid.NewGuid(),
            EventId   = eventId,
            ServiceId = serviceId,
            Quantity  = quantity,
            Price     = price
        };

    public static Order BuildOrder(
        Guid userId,
        Guid eventId,
        decimal amount          = 600m,
        string? paymentIntentId = null) =>
        new()
        {
            Id              = Guid.NewGuid(),
            UserId          = userId,
            EventId         = eventId,
            Amount          = amount,
            Currency        = "EGP",
            PaymentIntentId = paymentIntentId ?? $"intent-{Guid.NewGuid():N}",
            ShippingAddress = new Address { Street = "123 Main St", City = "Cairo", State = "Cairo" }
        };

    public static Voucher BuildVoucher(Guid ownerId, string code, int daysOld = 0) =>
        new()
        {
            Id        = Guid.NewGuid(),
            OwnerId   = ownerId,
            Code      = code,
            CreatedAt = DateTime.UtcNow.AddDays(-daysOld),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

    public static CorporationInquiry BuildInquiry(
        Guid eventTypeId,
        string companyName = "Test Company",
        string status      = InquiryStatuses.Pending,
        int daysUntilEvent = 30) =>
        new()
        {
            Id                     = Guid.NewGuid(),
            CompanyName            = companyName,
            ContactPerson          = "Ali Hassan",
            PhoneNumber            = "0100000000",
            Email                  = $"{companyName.Replace(" ", "").ToLower()}@test.com",
            EventTypeId            = eventTypeId,
            ExpectedDate           = DateTime.UtcNow.AddDays(daysUntilEvent),
            EstimatedAttendees     = 100,
            ApproximateBudget      = 50_000,
            AdditionalRequirements = "AV equipment",
            Status                 = status
        };

    public static SupportAgent BuildSupportAgent(string code = "AGT-001") =>
        new()
        {
            Id        = Guid.NewGuid(),
            AgentCode = code,
            Name      = "Support Agent",
            Email     = $"agent-{code.ToLower()}@support.com"
        };

    public static SupportTicket BuildTicket(
        string ticketNumber,
        TicketStatus   status   = TicketStatus.Open,
        TicketPriority priority = TicketPriority.Low,
        TicketType     type     = TicketType.Client,
        Guid?          agentId  = null) =>
        new()
        {
            Id              = Guid.NewGuid(),
            TicketNumber    = ticketNumber,
            Title           = $"Ticket {ticketNumber}",
            Description     = $"Description for ticket {ticketNumber}",
            From            = "user@test.com",
            Type            = type,
            Priority        = priority,
            Status          = status,
            OpenedAt        = DateTime.UtcNow,
            ResolvedAt      = status == TicketStatus.Resolved ? DateTime.UtcNow : null,
            AssignedAgentId = agentId
        };

    public static TicketReply BuildReply(Guid ticketId, string replyNumber = "RPL-001") =>
        new()
        {
            Id          = Guid.NewGuid(),
            TicketId    = ticketId,
            ReplyNumber = replyNumber,
            Message     = "We are looking into this — expect a response within 24 hours.",
            RepliedBy   = "Support Admin",
            RepliedAt   = DateTime.UtcNow
        };

    public static Notification BuildNotification(
        Guid userId,
        NotificationType type,
        string title,
        int daysOld = 0) =>
        new()
        {
            Id        = Guid.NewGuid(),
            UserId    = userId,
            Type      = type,
            Title     = title,
            Message   = $"Notification message for: {title}",
            CreatedAt = DateTime.UtcNow.AddDays(-daysOld)
        };
}

// ---------------------------------------------------------------------------
// Status string constants — mirrors whatever the domain layer uses.
// If the domain already exposes these as enums or static classes, delete these
// and reference the domain values directly.
// ---------------------------------------------------------------------------

internal static class EventStatuses
{
    public const string Planned   = "Planned";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

internal static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Paid    = "Paid";
    public const string Failed  = "Failed";
}

internal static class InquiryStatuses
{
    public const string Pending  = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}
