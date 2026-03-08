using System;

namespace BLL.DTOs
{
    // ── Request DTOs ──────────────────────────────────────────────

    public class CreateEventItemDto
    {
        public Guid EventId { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string VendorName { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateEventItemDto
    {
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string VendorName { get; set; }
        public int Quantity { get; set; }
    }

    // ── Response DTOs ─────────────────────────────────────────────

    public class EventItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string VendorName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }
}
