using System;

namespace Application.DTOs
{
    // ── Request DTOs ──────────────────────────────────────────────

    public class CreateEventItemDto
    {
        public Guid EventId { get; set; }
        public Guid ServiceId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateEventItemDto
    {        public int Quantity { get; set; }
    }

    public class ApproveItemRequest
    {
        public bool Approve { get; set; }
        public string? Reason { get; set; }
    }

    public class CancelEventRequest
    {
        public string? Reason { get; set; }
        public string? AdditionalNotes { get; set; }
    }

    // ── Response DTOs ─────────────────────────────────────────────

    public class EventItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceImage { get; set; } = string.Empty;
         public Guid VendorId { get; set; }                  // ← was missing
        public string VendorName { get; set; } = string.Empty; // ← was missing
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ItemStatus { get; set; } = "Pending"; // ← default value
        public string? RejectionReason { get; set; }
    }
}