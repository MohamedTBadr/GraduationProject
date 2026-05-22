using Application.Contracts;
using Application.DTOs.Reports;
using Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Ai;

namespace Infrastructure.Reporting
{
    public sealed class PdfReportService : IPdfReportService
    {
        public Task<byte[]> RenderAsync(ExecutiveReportDto report, CancellationToken ct = default)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var bytes = Document.Create(container =>
            {
                container.Page(BuildCoverPage(report));
                container.Page(BuildKpiPage(report));
                container.Page(BuildRevenueHistoryPage(report));
                container.Page(BuildTopServicesPage(report));

                if (report.AiInsights is not null)
                    container.Page(BuildAiInsightsPage(report.AiInsights));
            })
            .GeneratePdf();

            return Task.FromResult(bytes);
        }

        private static Action<PageDescriptor> BuildCoverPage(ExecutiveReportDto report) =>
            page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontFamily("Helvetica"));

                page.Content().Column(col =>
                {
                    col.Item().PaddingTop(80).Text("Executive Report")
                        .FontSize(36).Bold().FontColor("#1A1A2E");

                    col.Item().PaddingTop(12).Text(
                        report.Scope == ReportScope.Admin
                            ? "Platform Overview — Admin"
                            : $"Vendor Performance Report")
                        .FontSize(18).FontColor("#4A4A6A");

                    col.Item().PaddingTop(8).Text(
                        $"Generated: {report.GeneratedAt:MMMM dd, yyyy HH:mm} UTC")
                        .FontSize(11).FontColor("#888888");

                    col.Item().PaddingTop(60).LineHorizontal(1).LineColor("#DDDDDD");

                    col.Item().PaddingTop(40).Text("Confidential — For internal use only")
                        .FontSize(10).Italic().FontColor("#AAAAAA");
                });
            };

        private static Action<PageDescriptor> BuildKpiPage(ExecutiveReportDto report) =>
            page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Content().Column(col =>
                {
                    col.Item().Text("Key Performance Indicators")
                        .FontSize(22).Bold().FontColor("#1A1A2E");

                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Component(new KpiCard(
                            "Lifetime Revenue",
                            report.KPIs.LifetimeRevenue.ToString("C"),
                            "#2ECC71"));

                        row.ConstantItem(16);

                        row.RelativeItem().Component(new KpiCard(
                            "This Month",
                            report.KPIs.CurrentMonthRevenue.ToString("C"),
                            "#3498DB"));

                        row.ConstantItem(16);

                        row.RelativeItem().Component(new KpiCard(
                            "Growth",
                            $"{report.KPIs.GrowthPercentage:+0.00;-0.00}%",
                            report.KPIs.IsGrowthPositive ? "#2ECC71" : "#E74C3C"));
                    });

                    if (report.AdminMetrics is not null)
                    {
                        col.Item().PaddingTop(24).Text("Platform Metrics")
                            .FontSize(16).Bold();

                        col.Item().PaddingTop(12).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            void AddRow(string label, string value)
                            {
                                t.Cell().Padding(8).Text(label).FontSize(11);
                                t.Cell().Padding(8).Text(value).FontSize(11).Bold();
                            }

                            AddRow("Total Vendors", report.AdminMetrics.TotalVendors.ToString());
                            AddRow("Verified Vendors",
                                $"{report.AdminMetrics.VerifiedVendors} ({report.AdminMetrics.VendorVerificationRate}%)");
                            AddRow("Total Customers", report.AdminMetrics.TotalCustomers.ToString());
                            AddRow("Total Orders", report.AdminMetrics.TotalOrders.ToString());
                        });
                    }
                });
            };

        private static Action<PageDescriptor> BuildRevenueHistoryPage(ExecutiveReportDto report) =>
            page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Content().Column(col =>
                {
                    col.Item().Text("Revenue History (Last 12 Months)")
                        .FontSize(22).Bold().FontColor("#1A1A2E");

                    col.Item().PaddingTop(16).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        static IContainer HeaderCell(IContainer c) =>
                            c.Background("#1A1A2E").Padding(8);

                        t.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text("Month")
                                .FontColor(Colors.White).FontSize(10).Bold();
                            h.Cell().Element(HeaderCell).Text("Revenue")
                                .FontColor(Colors.White).FontSize(10).Bold();
                            h.Cell().Element(HeaderCell).Text("Orders")
                                .FontColor(Colors.White).FontSize(10).Bold();
                            h.Cell().Element(HeaderCell).Text("Growth")
                                .FontColor(Colors.White).FontSize(10).Bold();
                        });

                        foreach (var (item, index) in report.RevenueHistory
                            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                            .Select((x, i) => (x, i)))
                        {
                            var bg = index % 2 == 0 ? "#F8F9FA" : Colors.White;
                            var growthText = item.GrowthPercentage.HasValue
                                ? $"{item.GrowthPercentage:+0.00;-0.00}%"
                                : "—";
                            var growthColor = item.GrowthPercentage >= 0 ? "#2ECC71" : "#E74C3C";

                            t.Cell().Background(bg).Padding(8)
                                .Text(item.Label).FontSize(10);
                            t.Cell().Background(bg).Padding(8)
                                .Text(item.Revenue.ToString("C")).FontSize(10).Bold();
                            t.Cell().Background(bg).Padding(8)
                                .Text(item.Orders.ToString()).FontSize(10);
                            t.Cell().Background(bg).Padding(8)
                                .Text(growthText).FontSize(10)
                                .FontColor(item.GrowthPercentage.HasValue ? growthColor : "#888888");
                        }
                    });
                });
            };

        private static Action<PageDescriptor> BuildTopServicesPage(ExecutiveReportDto report) =>
            page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Content().Column(col =>
                {
                    col.Item().Text("Top Services by Revenue")
                        .FontSize(22).Bold().FontColor("#1A1A2E");

                    col.Item().PaddingTop(16).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        static IContainer HeaderCell(IContainer c) =>
                            c.Background("#1A1A2E").Padding(8);

                        t.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text("Service")
                                .FontColor(Colors.White).FontSize(10).Bold();
                            h.Cell().Element(HeaderCell).Text("Revenue")
                                .FontColor(Colors.White).FontSize(10).Bold();
                            h.Cell().Element(HeaderCell).Text("Share")
                                .FontColor(Colors.White).FontSize(10).Bold();
                            h.Cell().Element(HeaderCell).Text("Orders")
                                .FontColor(Colors.White).FontSize(10).Bold();
                        });

                        foreach (var (svc, i) in report.TopServices.Select((x, i) => (x, i)))
                        {
                            var bg = i % 2 == 0 ? "#F8F9FA" : Colors.White;
                            t.Cell().Background(bg).Padding(8).Text(svc.ServiceName).FontSize(10);
                            t.Cell().Background(bg).Padding(8).Text(svc.Revenue.ToString("C")).FontSize(10).Bold();
                            t.Cell().Background(bg).Padding(8).Text($"{svc.RevenueShare}%").FontSize(10);
                            t.Cell().Background(bg).Padding(8).Text(svc.Orders.ToString()).FontSize(10);
                        }
                    });
                });
            };

        private static Action<PageDescriptor> BuildAiInsightsPage(AiInsightResponseDto insights) =>
            page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Content().Column(col =>
                {
                    col.Item().Text("AI Business Intelligence Analysis")
                        .FontSize(22).Bold().FontColor("#1A1A2E");

                    col.Item().PaddingTop(4).Text($"Model: {insights.ModelUsed} · {insights.GeneratedAt:MMM dd, yyyy HH:mm} UTC")
                        .FontSize(9).FontColor("#999999").Italic();

                    col.Item().PaddingTop(16).Component(new InsightSection("Executive Summary", insights.Summary));
                    col.Item().PaddingTop(12).Component(new BulletSection("Risks", insights.Risks, "#E74C3C"));
                    col.Item().PaddingTop(12).Component(new BulletSection("Opportunities", insights.Opportunities, "#2ECC71"));
                    col.Item().PaddingTop(12).Component(new BulletSection("Recommendations", insights.Recommendations, "#3498DB"));
                    col.Item().PaddingTop(12).Component(new InsightSection("Conclusion", insights.Conclusion));

                    col.Item().PaddingTop(24).LineHorizontal(1).LineColor("#EEEEEE");
                    col.Item().PaddingTop(8)
                        .Text("⚠ AI-generated analysis is for guidance only. All figures are system-calculated and not modified by AI.")
                        .FontSize(8).FontColor("#AAAAAA").Italic();
                });
            };
    }

    public class KpiCard : IComponent
    {
        private readonly string _label;
        private readonly string _value;
        private readonly string _accentColor;

        public KpiCard(string label, string value, string accentColor)
        {
            _label = label;
            _value = value;
            _accentColor = accentColor;
        }

        public void Compose(IContainer container)
        {
            container
                .Border(1).BorderColor("#EEEEEE")
                .Background("#FAFAFA")
                .Padding(16)
                .Column(col =>
                {
                    col.Item().Text(_label).FontSize(10).FontColor("#888888");
                    col.Item().PaddingTop(4).Text(_value).FontSize(22).Bold().FontColor(_accentColor);
                });
        }
    }

    public class InsightSection : IComponent
    {
        private readonly string _title;
        private readonly string _body;

        public InsightSection(string title, string body)
        {
            _title = title;
            _body = body;
        }

        public void Compose(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text(_title).FontSize(14).Bold().FontColor("#1A1A2E");
                col.Item().PaddingTop(4).Text(_body).FontSize(11).LineHeight(1.5f);
            });
        }
    }

    public class BulletSection : IComponent
    {
        private readonly string _title;
        private readonly IReadOnlyList<string> _items;
        private readonly string _bulletColor;

        public BulletSection(string title, IReadOnlyList<string> items, string bulletColor)
        {
            _title = title;
            _items = items ?? new List<string>();
            _bulletColor = bulletColor;
        }

        public void Compose(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text(_title).FontSize(14).Bold().FontColor("#1A1A2E");

                foreach (var item in _items)
                {
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.ConstantItem(16).Text("●").FontColor(_bulletColor).FontSize(8)
                            .AlignMiddle();
                        row.RelativeItem().PaddingLeft(6).Text(item).FontSize(11);
                    });
                }
            });
        }
    }
}
