using System;

namespace Application.DTOs.Reports
{
    public sealed record RecentOrderDto
    {
        public Guid OrderId { get; init; }
        public string CustomerName { get; init; } = default!;
        public string? VendorName { get; init; }           // admin-only
        public string ServiceName { get; init; } = default!;
        public decimal Amount { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
