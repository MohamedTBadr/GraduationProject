namespace Application.DTOs.Reports
{
    public sealed record KpiSectionDto
    {
        public decimal LifetimeRevenue { get; init; }
        public decimal CurrentMonthRevenue { get; init; }
        public decimal LastMonthRevenue { get; init; }
        public decimal GrowthPercentage { get; init; }
        public bool IsGrowthPositive => GrowthPercentage >= 0;

        // Vendor-only
        public int? TotalOrders { get; init; }
        public decimal? AverageOrderValue { get; init; }
        public decimal? AverageMonthlyRevenue { get; init; }
    }
}
