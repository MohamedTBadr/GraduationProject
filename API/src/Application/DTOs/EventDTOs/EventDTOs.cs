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
        public Guid EventTypeId { get; set; }
        public DateTime EventDate { get; set; }
        public AddressDto Location { get; set; }       
        public decimal TotalBudget { get; set; }
        public int GuestCount { get; set; }
        public string? Notes { get; set; }                  // ← nullable (optional field)
    }

    public class UpdateEventDto
    {
        public string Title { get; set; } = string.Empty;
        public Guid EventTypeId { get; set; }
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
        public string EventTypeName { get; set; } = string.Empty;
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
        public List<EventCollaboratorDto> Collaborators { get; set; } = new();
    }

    public class EventSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventStatus { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public int ItemCount { get; set; }
        public List<EventItemResponseDto> EventItems { get; set; } = new();
    }
}

public class VendorBookingDto
{
    public Guid EventItemId { get; set; }
    public string ServiceName { get; set; }
    public decimal Price { get; set; }
    public string BookingStatus { get; set; }
    public string? Notes { get; set; }

    // Event details
    public Guid EventId { get; set; }
    public string EventTitle { get; set; }
    public string EventType { get; set; }
    public DateTime EventDate { get; set; }
    public string EventStatus { get; set; }
    public int GuestCount { get; set; }
    public string Location { get; set; }
}
//// add to your existing EventDTOs file

//public class CreateEventItemDto
//{

//    public Guid ServiceId { get; set; }
//    public string ServiceName { get; set; } = string.Empty;
//    public string? ServiceImage { get; set; }
//    public decimal Price { get; set; }
//    public Guid VendorId { get; set; }
//    public string VendorName { get; set; } = string.Empty;
//    public int Quantity { get; set; } = 1;
//}

//public class UpdateEventItemDto
//{
//    public string? ServiceImage { get; set; }
//    public string ServiceName { get; set; } = string.Empty;
//    public decimal Price { get; set; }
//    public string VendorName { get; set; } = string.Empty;
//    public int Quantity { get; set; }
//}