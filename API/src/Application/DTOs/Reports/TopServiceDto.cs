using System;

namespace Application.DTOs.Reports
{
    public sealed record TopServiceDto
    {
        public Guid ServiceId { get; init; }
        public string ServiceName { get; init; } = default!;
        public decimal Revenue { get; init; }
        public int Orders { get; init; }
        public int? QuantitySold { get; init; }
        public decimal RevenueShare { get; init; }         // % of total revenue
    }
}
