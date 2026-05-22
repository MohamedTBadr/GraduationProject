namespace Application.DTOs.Reports
{
    public sealed record RevenueHistoryItemDto
    {
        public int Year { get; init; }
        public int Month { get; init; }
        public string Label { get; init; } = default!;     // "Jan 2025"
        public decimal Revenue { get; init; }
        public int Orders { get; init; }
        public decimal? GrowthPercentage { get; init; }    // null for first month
    }
}
