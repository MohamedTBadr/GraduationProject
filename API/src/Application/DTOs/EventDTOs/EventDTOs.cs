using Application.DTOs.ServiceTypesDTOs;
using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    // ── Request DTOs ──────────────────────────────────────────────

    public class CreateEventDto
    {
        public Guid? UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public DateTime EventDate { get; set; }
        public AddressDto? Location { get; set; }           // ← nullable (optional field)
        public decimal TotalBudget { get; set; }
        public int GuestCount { get; set; }
        public string? Notes { get; set; }                  // ← nullable (optional field)
    }

    public class UpdateEventDto
    {
        public string Title { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public DateTime EventDate { get; set; }
        public AddressDto? Location { get; set; }           // ← nullable (optional field)
        public decimal TotalBudget { get; set; }
        public int GuestCount { get; set; }
        public string? Notes { get; set; }                  // ← nullable (optional field)
        public string EventStatus { get; set; } = string.Empty;
    }

    public class AddressDto
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }

    // ── Response DTOs ─────────────────────────────────────────────

    public class EventResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public AddressDto? Location { get; set; }           // ← nullable (event may have no location)
        public decimal TotalBudget { get; set; }
        public int GuestCount { get; set; }
        public string? Notes { get; set; }                  // ← nullable
        public string EventStatus { get; set; } = string.Empty;
        public string? CancellationReason { get; set; }     // ← was missing
        public string? AdditionalNotes { get; set; }        // ← was missing
          // ← was missing
        public DateTime? CancelledAt { get; set; }          // ← was missing
        public List<EventItemResponseDto> EventItems { get; set; } = new();
    }

    public class EventSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventStatus { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public int ItemCount { get; set; }
    }
}