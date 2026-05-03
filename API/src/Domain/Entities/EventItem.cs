using Domain.Entities;

public class 
    EventItem
{
    public Guid Id { get; set; }
    public Event Event { get; set; }
    public Guid EventId { get; set; }
    public string ServiceImage { get; set; }
    public string ServiceName { get; set; }
    public decimal Price { get; set; }
    public string VendorName { get; set; }
    public int Quantity { get; set; }

    // ── Add these ──────────────────────────────
    public Guid VendorId { get; set; }                          // links item to vendor
    public ApplicationUser Vendor { get; set; }
    public string ItemStatus { get; set; } = "Pending";         // Pending, Approved, Rejected
    public string? RejectionReason { get; set; }
}