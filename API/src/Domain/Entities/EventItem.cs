using Domain.Entities;

public class 
    EventItem
{
    public Guid Id { get; set; }
    public Event Event { get; set; }
    public Guid EventId { get; set; }

    public decimal Price { get; set; }
    public int Quantity { get; set; }

    // ── Add these ──────────────────────────────
    // public Guid VendorId { get; set; }                          // links item to vendor
    public Guid ServiceId { get; set; }                                        // links item to the booked service
    public Service Service { get; set; }
    public string ItemStatus { get; set; } = "Pending";         // Pending, Approved, Rejected
    public string? RejectionReason { get; set; }
}