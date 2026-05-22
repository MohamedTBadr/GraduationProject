using System;

namespace Application.DTOs.Reports
{
    public sealed record AdminMetricsDto
    {
        public int TotalVendors { get; init; }
        public int VerifiedVendors { get; init; }
        public int TotalCustomers { get; init; }
        public int TotalOrders { get; init; }
        public decimal VendorVerificationRate =>
            TotalVendors > 0
                ? Math.Round((decimal)VerifiedVendors / TotalVendors * 100, 2)
                : 0;
    }
}
