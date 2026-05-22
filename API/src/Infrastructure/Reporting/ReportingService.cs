using Application.Contracts;
using Application.DTOs.Ai;
using Application.DTOs.Reports;
using Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Reporting
{
    public sealed class ReportingService : IReportingService
    {
        private readonly IAnalyticsService _analytics;
        private readonly IAiInsightService _ai;

        public ReportingService(IAnalyticsService analytics, IAiInsightService ai)
        {
            _analytics = analytics;
            _ai = ai;
        }

        public async Task<ExecutiveReportDto> GenerateFullReportAsync(
            Guid? vendorId,
            ReportScope scope,
            CancellationToken ct = default)
        {
            // Step 1: Build KPIs deterministically
            var report = scope == ReportScope.Admin
                ? await _analytics.BuildAdminReportAsync(ct)
                : await _analytics.BuildVendorReportAsync(vendorId!.Value, ct);

            // Step 2: Request AI insights (non-blocking failure — gracefully degrade)
            var aiRequest = new AiInsightRequestDto
            {
                Scope = scope,
                KPIs = report.KPIs,
                RevenueHistory = report.RevenueHistory,
                TopServices = report.TopServices,
                AdminMetrics = report.AdminMetrics
            };

            AiInsightResponseDto? insights = null;

            try
            {
                insights = await _ai.GenerateInsightsAsync(aiRequest, ct);
            }
            catch (Exception)
            {
                // Graceful degradation: AI insights is a value-added feature and should not block the report generation.
            }

            return report with { AiInsights = insights };
        }
    }
}
