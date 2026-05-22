using Domain.Enums;
using Application.DTOs.Reports;
using System.Collections.Generic;

namespace Application.DTOs.Ai
{
    public sealed record AiInsightRequestDto
    {
        public ReportScope Scope { get; init; }
        public KpiSectionDto KPIs { get; init; } = default!;
        public IReadOnlyList<RevenueHistoryItemDto> RevenueHistory { get; init; } = [];
        public IReadOnlyList<TopServiceDto> TopServices { get; init; } = [];
        public AdminMetricsDto? AdminMetrics { get; init; }
    }
}
