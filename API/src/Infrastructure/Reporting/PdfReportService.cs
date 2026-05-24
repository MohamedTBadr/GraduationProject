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
    /// <summary>
    /// Color tokens matching the design system's CSS custom properties.
    /// </summary>
    internal static class DS
    {
        // Backgrounds & surfaces
        public const string Navy = "#1A2540"; // --navy
        public const string Navy2 = "#243050"; // --navy2
        public const string Dark = "#0E1627"; // --dark
        public const string Cream = "#F9F6F0"; // --cream
        public const string Cream2 = "#F0EBE0"; // --cream2
        public const string White = "#FFFFFF"; // --white
        public const string LGray = "#E8E4DC"; // --lgray

        // Text
        public const string Gray = "#6B7280"; // --gray

        // Accents
        public const string Gold = "#C9A84C"; // --gold
        public const string Gold2 = "#E8C97A"; // --gold2
        public const string Green = "#16A34A"; // --green
        public const string Amber = "#CA8A04"; // --amber  (replaces red for warnings/risks)

        // Typography — register these via FontManager before generating
        public const string FontDisplay = "Cormorant Garamond"; // --ff-d (headings)
        public const string FontBody = "Outfit";             // --ff-b (body / tables)
    }

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

        // ─────────────────────────────────────────────────────────────
        //  Cover Page
        // ─────────────────────────────────────────────────────────────
        private static Action<PageDescriptor> BuildCoverPage(ExecutiveReportDto report) =>
            page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Background(DS.Cream);
                page.DefaultTextStyle(t => t.FontFamily(DS.FontBody));

                page.Content().PaddingVertical(20).Column(col =>
                {
                    // Logo mark
                    col.Item().AlignCenter().PaddingTop(40).Width(120).Height(120)
                        .Background(DS.Navy)
                        .Border(3)
                        .BorderColor(DS.Gold)
                        .AlignCenter()
                        .AlignMiddle()
                        .Text("Epic Hub")
                        .FontSize(44)
                        .Bold()
                        .FontFamily(DS.FontDisplay)
                        .FontColor(DS.Gold2);

                    // Tagline
                    col.Item().AlignCenter().PaddingTop(24)
                        .Text("Don't Plan, Go Epic")
                        .FontSize(14)
                        .FontFamily(DS.FontBody)
                        .FontColor(DS.Gray)
                        .LetterSpacing(0.08f);

                    // Main title
                    col.Item().AlignCenter().PaddingTop(28)
                        .Text("Executive Report")
                        .FontSize(36)
                        .Bold()
                        .FontFamily(DS.FontDisplay)
                        .FontColor(DS.Navy);

                    // Subtitle
                    col.Item().AlignCenter().PaddingTop(8)
                        .Text(report.Scope == ReportScope.Admin
                            ? "Platform Overview — Admin"
                            : "Vendor Performance Report")
                        .FontSize(14)
                        .FontFamily(DS.FontBody)
                        .FontColor(DS.Navy2);

                    // Gold divider
                    col.Item().PaddingTop(36).PaddingHorizontal(60)
                        .LineHorizontal(1.5f)
                        .LineColor(DS.Gold);

                    // Date stamp
                    col.Item().AlignCenter().PaddingTop(16)
                        .Text($"Generated: {report.GeneratedAt:MMMM dd, yyyy  HH:mm} UTC")
                        .FontSize(10)
                        .FontFamily(DS.FontBody)
                        .FontColor(DS.Gray);

                    // Bottom confidential note
                    col.Item().PaddingTop(48).AlignCenter()
                        .Text("Confidential — For internal use only")
                        .FontSize(9)
                        .Italic()
                        .FontColor(DS.Gray);
                });
            };

        // ─────────────────────────────────────────────────────────────
        //  KPI Page
        // ─────────────────────────────────────────────────────────────
        private static Action<PageDescriptor> BuildKpiPage(ExecutiveReportDto report) =>
            page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Background(DS.Cream);
                page.DefaultTextStyle(t => t.FontFamily(DS.FontBody));

                page.Content().Column(col =>
                {
                    col.Item().PaddingBottom(4)
                        .Text("Key Performance Indicators")
                        .FontSize(26).Bold()
                        .FontFamily(DS.FontDisplay)
                        .FontColor(DS.Navy);

                    col.Item().LineHorizontal(1.5f).LineColor(DS.Gold);

                    col.Item().PaddingTop(24).Row(row =>
                    {
                        row.RelativeItem().Component(new KpiCard(
                            "Lifetime Revenue",
                            report.KPIs.LifetimeRevenue.ToString("C"),
                            DS.Green));

                        row.ConstantItem(16);

                        row.RelativeItem().Component(new KpiCard(
                            "This Month",
                            report.KPIs.CurrentMonthRevenue.ToString("C"),
                            DS.Gold));

                        row.ConstantItem(16);

                        row.RelativeItem().Component(new KpiCard(
                            "Growth",
                            $"{report.KPIs.GrowthPercentage:+0.00;-0.00}%",
                            report.KPIs.IsGrowthPositive ? DS.Green : DS.Amber));
                    });

                    if (report.AdminMetrics is not null)
                    {
                        col.Item().PaddingTop(32)
                            .Text("Platform Metrics")
                            .FontSize(18).Bold()
                            .FontFamily(DS.FontDisplay)
                            .FontColor(DS.Navy);

                        col.Item().PaddingTop(12).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            bool alt = false;
                            void AddRow(string label, string value)
                            {
                                string bg = alt ? DS.Cream2 : DS.White;
                                t.Cell().Background(bg).Padding(10)
                                    .Text(label).FontSize(11).FontColor(DS.Gray);
                                t.Cell().Background(bg).Padding(10)
                                    .Text(value).FontSize(11).Bold().FontColor(DS.Navy);
                                alt = !alt;
                            }

                            AddRow("Total Vendors", report.AdminMetrics.TotalVendors.ToString());
                            AddRow("Verified Vendors", $"{report.AdminMetrics.VerifiedVendors} ({report.AdminMetrics.VendorVerificationRate}%)");
                            AddRow("Total Customers", report.AdminMetrics.TotalCustomers.ToString());
                            AddRow("Total Orders", report.AdminMetrics.TotalOrders.ToString());
                        });
                    }
                });
            };

        // ─────────────────────────────────────────────────────────────
        //  Revenue History Page
        // ─────────────────────────────────────────────────────────────
        private static Action<PageDescriptor> BuildRevenueHistoryPage(ExecutiveReportDto report) =>
            page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Background(DS.Cream);
                page.DefaultTextStyle(t => t.FontFamily(DS.FontBody));

                page.Content().Column(col =>
                {
                    col.Item().PaddingBottom(4)
                        .Text("Revenue History (Last 12 Months)")
                        .FontSize(26).Bold()
                        .FontFamily(DS.FontDisplay)
                        .FontColor(DS.Navy);

                    col.Item().LineHorizontal(1.5f).LineColor(DS.Gold);

                    col.Item().PaddingTop(20).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        // Header style
                        IContainer HeaderCell(IContainer c) =>
                            c.Background(DS.Navy).Padding(10);

                        t.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text("Month")
                                .FontColor(DS.Gold2).FontSize(10).Bold();
                            h.Cell().Element(HeaderCell).Text("Revenue")
                                .FontColor(DS.Gold2).FontSize(10).Bold();
                            h.Cell().Element(HeaderCell).Text("Orders")
                                .FontColor(DS.Gold2).FontSize(10).Bold();
                            h.Cell().Element(HeaderCell).Text("Growth")
                                .FontColor(DS.Gold2).FontSize(10).Bold();
                        });

                        foreach (var (item, index) in report.RevenueHistory
                            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                            .Select((x, i) => (x, i)))
                        {
                            string bg = index % 2 == 0 ? DS.Cream : DS.White;
                            string growthText = item.GrowthPercentage.HasValue
                                ? $"{item.GrowthPercentage:+0.00;-0.00}%"
                                : "—";
                            string growthColor = item.GrowthPercentage >= 0 ? DS.Green : DS.Amber;

                            t.Cell().Background(bg).Padding(9)
                                .Text(item.Label).FontSize(10).FontColor(DS.Navy2);
                            t.Cell().Background(bg).Padding(9)
                                .Text(item.Revenue.ToString("C")).FontSize(10).Bold().FontColor(DS.Navy);
                            t.Cell().Background(bg).Padding(9)
                                .Text(item.Orders.ToString()).FontSize(10).FontColor(DS.Gray);
                            t.Cell().Background(bg).Padding(9)
                                .Text(growthText).FontSize(10)
                                .FontColor(item.GrowthPercentage.HasValue ? growthColor : DS.Gray);
                        }
                    });
                });
            };

        // ─────────────────────────────────────────────────────────────
        //  Top Services Page
        // ─────────────────────────────────────────────────────────────
        private static Action<PageDescriptor> BuildTopServicesPage(ExecutiveReportDto report) =>
            page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Background(DS.Cream);
                page.DefaultTextStyle(t => t.FontFamily(DS.FontBody));

                page.Content().Column(col =>
                {
                    col.Item().PaddingBottom(4)
                        .Text("Top Services by Revenue")
                        .FontSize(26).Bold()
                        .FontFamily(DS.FontDisplay)
                        .FontColor(DS.Navy);

                    col.Item().LineHorizontal(1.5f).LineColor(DS.Gold);

                    col.Item().PaddingTop(20).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        IContainer HeaderCell(IContainer c) =>
                            c.Background(DS.Navy).Padding(10);

                        t.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text("Service")
                                .FontColor(DS.Gold2).FontSize(10).Bold();
                            h.Cell().Element(HeaderCell).Text("Revenue")
                                .FontColor(DS.Gold2).FontSize(10).Bold();
                            h.Cell().Element(HeaderCell).Text("Share")
                                .FontColor(DS.Gold2).FontSize(10).Bold();
                            h.Cell().Element(HeaderCell).Text("Orders")
                                .FontColor(DS.Gold2).FontSize(10).Bold();
                        });

                        foreach (var (svc, i) in report.TopServices.Select((x, i) => (x, i)))
                        {
                            string bg = i % 2 == 0 ? DS.Cream : DS.White;
                            t.Cell().Background(bg).Padding(9)
                                .Text(svc.ServiceName).FontSize(10).FontColor(DS.Navy);
                            t.Cell().Background(bg).Padding(9)
                                .Text(svc.Revenue.ToString("C")).FontSize(10).Bold().FontColor(DS.Navy);
                            t.Cell().Background(bg).Padding(9)
                                .Text($"{svc.RevenueShare}%").FontSize(10).FontColor(DS.Gold);
                            t.Cell().Background(bg).Padding(9)
                                .Text(svc.Orders.ToString()).FontSize(10).FontColor(DS.Gray);
                        }
                    });
                });
            };

        // ─────────────────────────────────────────────────────────────
        //  AI Insights Page
        // ─────────────────────────────────────────────────────────────
        private static Action<PageDescriptor> BuildAiInsightsPage(AiInsightResponseDto insights) =>
            page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Background(DS.Cream);
                page.DefaultTextStyle(t => t.FontFamily(DS.FontBody));

                page.Content().Column(col =>
                {
                    col.Item().PaddingBottom(4)
                        .Text("AI Business Intelligence Analysis")
                        .FontSize(26).Bold()
                        .FontFamily(DS.FontDisplay)
                        .FontColor(DS.Navy);

                    col.Item().LineHorizontal(1.5f).LineColor(DS.Gold);

                    col.Item().PaddingTop(6)
                        .Text($"Model: {insights.ModelUsed}  ·  {insights.GeneratedAt:MMM dd, yyyy  HH:mm} UTC")
                        .FontSize(9).FontColor(DS.Gray).Italic();

                    col.Item().PaddingTop(20).Component(new InsightSection("Executive Summary", insights.Summary));
                    col.Item().PaddingTop(14).Component(new BulletSection("Risks", insights.Risks, DS.Amber));
                    col.Item().PaddingTop(14).Component(new BulletSection("Opportunities", insights.Opportunities, DS.Green));
                    col.Item().PaddingTop(14).Component(new BulletSection("Recommendations", insights.Recommendations, DS.Gold));
                    col.Item().PaddingTop(14).Component(new InsightSection("Conclusion", insights.Conclusion));

                    col.Item().PaddingTop(28).LineHorizontal(1).LineColor(DS.LGray);
                    col.Item().PaddingTop(8)
                        .Text("⚠ AI-generated analysis is for guidance only. All figures are system-calculated and not modified by AI.")
                        .FontSize(8).FontColor(DS.Gray).Italic();
                });
            };
    }

    // ─────────────────────────────────────────────────────────────────
    //  Components
    // ─────────────────────────────────────────────────────────────────

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
                .Border(1)
                .BorderColor(DS.LGray)
                .Background(DS.White)
                .Padding(18)
                .Column(col =>
                {
                    col.Item()
                        .Text(_label)
                        .FontSize(10)
                        .FontFamily(DS.FontBody)
                        .FontColor(DS.Gray);

                    // Gold accent rule
                    col.Item().PaddingTop(6).PaddingBottom(6)
                        .LineHorizontal(2)
                        .LineColor(_accentColor);

                    col.Item()
                        .Text(_value)
                        .FontSize(24)
                        .Bold()
                        .FontFamily(DS.FontDisplay)
                        .FontColor(_accentColor);
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
                col.Item()
                    .Text(_title)
                    .FontSize(16)
                    .Bold()
                    .FontFamily(DS.FontDisplay)
                    .FontColor(DS.Navy);

                col.Item().PaddingTop(5)
                    .Text(_body)
                    .FontSize(11)
                    .FontFamily(DS.FontBody)
                    .FontColor(DS.Navy2)
                    .LineHeight(1.6f);
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
                col.Item()
                    .Text(_title)
                    .FontSize(16)
                    .Bold()
                    .FontFamily(DS.FontDisplay)
                    .FontColor(DS.Navy);

                foreach (var item in _items)
                {
                    col.Item().PaddingTop(5).Row(row =>
                    {
                        row.ConstantItem(18)
                            .PaddingTop(2)
                            .Text("◆")
                            .FontColor(_bulletColor)
                            .FontSize(7);

                        row.RelativeItem()
                            .PaddingLeft(4)
                            .Text(item)
                            .FontSize(11)
                            .FontFamily(DS.FontBody)
                            .FontColor(DS.Navy2)
                            .LineHeight(1.5f);
                    });
                }
            });
        }
    }
}