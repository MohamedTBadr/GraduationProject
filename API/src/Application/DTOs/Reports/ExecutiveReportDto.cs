using Domain.Enums;
using Application.DTOs.Ai;
using System;
using System.Collections.Generic;

namespace Application.DTOs.Reports
{
    public sealed record ExecutiveReportDto
    {
        public Guid ReportId { get; init; } = Guid.NewGuid();
        public ReportScope Scope { get; init; }
        public Guid? VendorId { get; init; }
        public DateTime GeneratedAt { get; init; }

        public KpiSectionDto KPIs { get; init; } = default!;
        public IReadOnlyList<RevenueHistoryItemDto> RevenueHistory { get; init; } = [];
        public IReadOnlyList<TopServiceDto> TopServices { get; init; } = [];
        public IReadOnlyList<RecentOrderDto> RecentOrders { get; init; } = [];
        public AdminMetricsDto? AdminMetrics { get; init; }

        // Populated after AI call
        public AiInsightResponseDto? AiInsights { get; init; }
    }
}
