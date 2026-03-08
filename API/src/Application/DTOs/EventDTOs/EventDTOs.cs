using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    // ── Request DTOs ──────────────────────────────────────────────

    public class CreateEventDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid ServiceTypeId { get; set; }
        public DateTime EventDate { get; set; }
        public AddressDto Location { get; set; }
        public decimal TotalBudget { get; set; }
        public int GuestCount { get; set; }
        public string Notes { get; set; }
    }

    public class UpdateEventDto
    {
        public string Title { get; set; } = string.Empty;
        public Guid ServiceTypeId { get; set; }
        public DateTime EventDate { get; set; }
        public AddressDto Location { get; set; }
        public decimal TotalBudget { get; set; }
        public int GuestCount { get; set; }
        public string Notes { get; set; }
        public string EventStatus { get; set; }
    }

    public class AddressDto
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }

    // ── Response DTOs ─────────────────────────────────────────────

    public class EventResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public string ServiceTypeName { get; set; }
        public DateTime EventDate { get; set; }
        public AddressDto Location { get; set; }
        public decimal TotalBudget { get; set; }
        public int GuestCount { get; set; }
        public string Notes { get; set; }
        public string EventStatus { get; set; }
        public List<EventItemResponseDto> EventItems { get; set; } = new();
    }

    public class EventSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime EventDate { get; set; }
        public string EventStatus { get; set; }
        public decimal TotalBudget { get; set; }
        public int ItemCount { get; set; }
    }
}
