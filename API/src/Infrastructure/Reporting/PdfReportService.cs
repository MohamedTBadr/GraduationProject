using Application.Contracts;
using Application.DTOs.Ai;
using Application.DTOs.Reports;
using Domain.Enums;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Reporting
{
    // ─────────────────────────────────────────────────────────────────
    //  Design System Colors (matches your CSS custom properties)
    // ─────────────────────────────────────────────────────────────────
    internal static class DS
    {
        public static readonly DeviceRgb Navy = Hex("#1A2540");
        public static readonly DeviceRgb Navy2 = Hex("#243050");
        public static readonly DeviceRgb Dark = Hex("#0E1627");
        public static readonly DeviceRgb Cream = Hex("#F9F6F0");
        public static readonly DeviceRgb Cream2 = Hex("#F0EBE0");
        public static readonly DeviceRgb White = Hex("#FFFFFF");
        public static readonly DeviceRgb LGray = Hex("#E8E4DC");
        public static readonly DeviceRgb Gray = Hex("#6B7280");
        public static readonly DeviceRgb Gold = Hex("#C9A84C");
        public static readonly DeviceRgb Gold2 = Hex("#E8C97A");
        public static readonly DeviceRgb Green = Hex("#16A34A");
        public static readonly DeviceRgb Amber = Hex("#CA8A04");

        private static DeviceRgb Hex(string hex)
        {
            hex = hex.TrimStart('#');
            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);
            return new DeviceRgb(r, g, b);
        }
    }

    public sealed class PdfReportService : IPdfReportService
    {
        public Task<byte[]> RenderAsync(ExecutiveReportDto report, CancellationToken ct = default)
        {
            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            using var doc = new Document(pdf, PageSize.A4);

            doc.SetMargins(56, 56, 56, 56); // ~2cm margins

            // Fonts — iText7 ships these built-in, no file needed
            var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var fontItalic = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);

            BuildCoverPage(doc, pdf, report, fontBold, fontNormal, fontItalic);
            BuildKpiPage(doc, pdf, report, fontBold, fontNormal);
            BuildRevenueHistoryPage(doc, pdf, report, fontBold, fontNormal);
            BuildTopServicesPage(doc, pdf, report, fontBold, fontNormal);

            if (report.AiInsights is not null)
                BuildAiInsightsPage(doc, pdf, report.AiInsights, fontBold, fontNormal, fontItalic);

            doc.Close();
            return Task.FromResult(ms.ToArray());
        }

        // ─────────────────────────────────────────────────────────────
        //  Cover Page
        // ─────────────────────────────────────────────────────────────
        private static void BuildCoverPage(Document doc, PdfDocument pdf,
            ExecutiveReportDto report,
            PdfFont fontBold, PdfFont fontNormal, PdfFont fontItalic)
        {
            // Navy background rectangle at top
            var page = pdf.GetLastPage();
            var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(page);
            canvas.SetFillColor(DS.Navy)
                  .Rectangle(0, PageSize.A4.GetHeight() - 280, PageSize.A4.GetWidth(), 280)
                  .Fill();
            canvas.SetFillColor(DS.Gold)
                  .Rectangle(0, PageSize.A4.GetHeight() - 284, PageSize.A4.GetWidth(), 4)
                  .Fill();

            // Logo text on navy band
            var logoText = new Paragraph("Epic Hub")
                .SetFont(fontBold).SetFontSize(40).SetFontColor(DS.Gold2)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(40);
            doc.Add(logoText);

            var tagline = new Paragraph("Don't Plan, Go Epic")
                .SetFont(fontItalic).SetFontSize(13).SetFontColor(DS.Gold)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(4);
            doc.Add(tagline);

            // Spacer to push below navy band
            doc.Add(new Paragraph(" ").SetFontSize(40));

            var title = new Paragraph("Executive Report")
                .SetFont(fontBold).SetFontSize(32).SetFontColor(DS.Navy)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(24);
            doc.Add(title);

            var subtitle = new Paragraph(report.Scope == ReportScope.Admin
                    ? "Platform Overview — Admin"
                    : "Vendor Performance Report")
                .SetFont(fontNormal).SetFontSize(14).SetFontColor(DS.Navy2)
                .SetTextAlignment(TextAlignment.CENTER);
            doc.Add(subtitle);

            // Gold divider line (via 1-cell table)
            doc.Add(GoldDivider());

            var dateLine = new Paragraph($"Generated: {report.GeneratedAt:MMMM dd, yyyy  HH:mm} UTC")
                .SetFont(fontNormal).SetFontSize(10).SetFontColor(DS.Gray)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(12);
            doc.Add(dateLine);

            var confidential = new Paragraph("Confidential — For internal use only")
                .SetFont(fontItalic).SetFontSize(9).SetFontColor(DS.Gray)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(60);
            doc.Add(confidential);

            doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        }

        // ─────────────────────────────────────────────────────────────
        //  KPI Page
        // ─────────────────────────────────────────────────────────────
        private static void BuildKpiPage(Document doc, PdfDocument pdf,
            ExecutiveReportDto report, PdfFont fontBold, PdfFont fontNormal)
        {
            doc.Add(PageTitle("Key Performance Indicators", fontBold));
            doc.Add(GoldDivider());

            // KPI cards as a 3-column table
            var kpiTable = new iText.Layout.Element.Table(UnitValue.CreatePercentArray(new float[] { 1, 1, 1 }))
                .UseAllAvailableWidth()
                .SetMarginTop(20);

            kpiTable.AddCell(KpiCard("Lifetime Revenue",
                report.KPIs.LifetimeRevenue.ToString("C"), DS.Green, fontBold, fontNormal));
            kpiTable.AddCell(KpiCard("This Month",
                report.KPIs.CurrentMonthRevenue.ToString("C"), DS.Gold, fontBold, fontNormal));

            var growthColor = report.KPIs.IsGrowthPositive ? DS.Green : DS.Amber;
            kpiTable.AddCell(KpiCard("Growth",
                $"{report.KPIs.GrowthPercentage:+0.00;-0.00}%", growthColor, fontBold, fontNormal));

            doc.Add(kpiTable);

            if (report.AdminMetrics is not null)
            {
                doc.Add(new Paragraph("Platform Metrics")
                    .SetFont(fontBold).SetFontSize(18).SetFontColor(DS.Navy)
                    .SetMarginTop(28));

                var table = new iText.Layout.Element.Table(UnitValue.CreatePercentArray(new float[] { 1, 1 }))
                    .UseAllAvailableWidth()
                    .SetMarginTop(12);

                bool alt = false;
                void AddRow(string label, string value)
                {
                    var bg = alt ? DS.Cream2 : DS.White;
                    table.AddCell(StyledCell(label, fontNormal, 11, DS.Gray, bg));
                    table.AddCell(StyledCell(value, fontBold, 11, DS.Navy, bg));
                    alt = !alt;
                }

                AddRow("Total Vendors", report.AdminMetrics.TotalVendors.ToString());
                AddRow("Verified Vendors", $"{report.AdminMetrics.VerifiedVendors} ({report.AdminMetrics.VendorVerificationRate}%)");
                AddRow("Total Customers", report.AdminMetrics.TotalCustomers.ToString());
                AddRow("Total Orders", report.AdminMetrics.TotalOrders.ToString());

                doc.Add(table);
            }

            doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        }

        // ─────────────────────────────────────────────────────────────
        //  Revenue History Page
        // ─────────────────────────────────────────────────────────────
        private static void BuildRevenueHistoryPage(Document doc, PdfDocument pdf,
            ExecutiveReportDto report, PdfFont fontBold, PdfFont fontNormal)
        {
            doc.Add(PageTitle("Revenue History (Last 12 Months)", fontBold));
            doc.Add(GoldDivider());

            var table = new iText.Layout.Element.Table(UnitValue.CreatePercentArray(new float[] { 2, 3, 2, 2 }))
                .UseAllAvailableWidth()
                .SetMarginTop(20);

            // Header
            foreach (var h in new[] { "Month", "Revenue", "Orders", "Growth" })
                table.AddHeaderCell(HeaderCell(h, fontBold));

            int idx = 0;
            foreach (var item in report.RevenueHistory
                .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month))
            {
                var bg = idx++ % 2 == 0 ? DS.Cream : DS.White;
                string growthText = item.GrowthPercentage.HasValue
                    ? $"{item.GrowthPercentage:+0.00;-0.00}%" : "—";
                var growthColor = (item.GrowthPercentage ?? 0) >= 0 ? DS.Green : DS.Amber;
                var growthFontColor = item.GrowthPercentage.HasValue ? growthColor : DS.Gray;

                table.AddCell(StyledCell(item.Label, fontNormal, 10, DS.Navy2, bg));
                table.AddCell(StyledCell(item.Revenue.ToString("C"), fontBold, 10, DS.Navy, bg));
                table.AddCell(StyledCell(item.Orders.ToString(), fontNormal, 10, DS.Gray, bg));
                table.AddCell(StyledCell(growthText, fontNormal, 10, growthFontColor, bg));
            }

            doc.Add(table);
            doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        }

        // ─────────────────────────────────────────────────────────────
        //  Top Services Page
        // ─────────────────────────────────────────────────────────────
        private static void BuildTopServicesPage(Document doc, PdfDocument pdf,
            ExecutiveReportDto report, PdfFont fontBold, PdfFont fontNormal)
        {
            doc.Add(PageTitle("Top Services by Revenue", fontBold));
            doc.Add(GoldDivider());

            var table = new iText.Layout.Element.Table(UnitValue.CreatePercentArray(new float[] { 3, 2, 2, 2 }))
                .UseAllAvailableWidth()
                .SetMarginTop(20);

            foreach (var h in new[] { "Service", "Revenue", "Share", "Orders" })
                table.AddHeaderCell(HeaderCell(h, fontBold));

            int i = 0;
            foreach (var svc in report.TopServices)
            {
                var bg = i++ % 2 == 0 ? DS.Cream : DS.White;
                table.AddCell(StyledCell(svc.ServiceName, fontNormal, 10, DS.Navy, bg));
                table.AddCell(StyledCell(svc.Revenue.ToString("C"), fontBold, 10, DS.Navy, bg));
                table.AddCell(StyledCell($"{svc.RevenueShare}%", fontNormal, 10, DS.Gold, bg));
                table.AddCell(StyledCell(svc.Orders.ToString(), fontNormal, 10, DS.Gray, bg));
            }

            doc.Add(table);
            doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        }

        // ─────────────────────────────────────────────────────────────
        //  AI Insights Page
        // ─────────────────────────────────────────────────────────────
        private static void BuildAiInsightsPage(Document doc, PdfDocument pdf,
            AiInsightResponseDto insights,
            PdfFont fontBold, PdfFont fontNormal, PdfFont fontItalic)
        {
            doc.Add(PageTitle("AI Business Intelligence Analysis", fontBold));
            doc.Add(GoldDivider());

            doc.Add(new Paragraph($"Model: {insights.ModelUsed}  ·  {insights.GeneratedAt:MMM dd, yyyy  HH:mm} UTC")
                .SetFont(fontItalic).SetFontSize(9).SetFontColor(DS.Gray).SetMarginTop(4));

            doc.Add(InsightSection("Executive Summary", insights.Summary, fontBold, fontNormal));
            doc.Add(BulletSection("Risks", insights.Risks, DS.Amber, fontBold, fontNormal));
            doc.Add(BulletSection("Opportunities", insights.Opportunities, DS.Green, fontBold, fontNormal));
            doc.Add(BulletSection("Recommendations", insights.Recommendations, DS.Gold, fontBold, fontNormal));
            doc.Add(InsightSection("Conclusion", insights.Conclusion, fontBold, fontNormal));

            doc.Add(GoldDivider());
            doc.Add(new Paragraph("⚠ AI-generated analysis is for guidance only. All figures are system-calculated and not modified by AI.")
                .SetFont(fontItalic).SetFontSize(8).SetFontColor(DS.Gray).SetMarginTop(8));
        }

        // ─────────────────────────────────────────────────────────────
        //  Reusable Helpers
        // ─────────────────────────────────────────────────────────────

        private static Paragraph PageTitle(string text, PdfFont font) =>
            new Paragraph(text)
                .SetFont(font).SetFontSize(26).SetFontColor(DS.Navy)
                .SetMarginTop(0).SetMarginBottom(4);

        private static iText.Layout.Element.Table GoldDivider()
        {
            var t = new iText.Layout.Element.Table(UnitValue.CreatePercentArray(new float[] { 1 }))
                .UseAllAvailableWidth()
                .SetMarginTop(4).SetMarginBottom(8)
                .SetBorder(Border.NO_BORDER);
            var cell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(new SolidBorder(DS.Gold, 1.5f))
                .SetHeight(4);
            t.AddCell(cell);
            return t;
        }

        private static Cell KpiCard(string label, string value, DeviceRgb accentColor,
            PdfFont fontBold, PdfFont fontNormal)
        {
            var cell = new Cell()
                .SetBorder(new SolidBorder(DS.LGray, 1))
                .SetBackgroundColor(DS.White)
                .SetPadding(14);

            cell.Add(new Paragraph(label)
                .SetFont(fontNormal).SetFontSize(10).SetFontColor(DS.Gray).SetMarginBottom(6));

            // Accent line
            var accent = new iText.Layout.Element.Table(UnitValue.CreatePercentArray(new float[] { 1 }))
                .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(6);
            accent.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                .SetBorderBottom(new SolidBorder(accentColor, 2)).SetHeight(4));
            cell.Add(accent);

            cell.Add(new Paragraph(value)
                .SetFont(fontBold).SetFontSize(22).SetFontColor(accentColor));

            return cell;
        }

        private static Cell HeaderCell(string text, PdfFont fontBold) =>
            new Cell()
                .SetBackgroundColor(DS.Navy)
                .SetPadding(10)
                .SetBorder(Border.NO_BORDER)
                .Add(new Paragraph(text)
                    .SetFont(fontBold).SetFontSize(10).SetFontColor(DS.Gold2));

        private static Cell StyledCell(string text, PdfFont font, float fontSize,
            DeviceRgb color, DeviceRgb bg) =>
            new Cell()
                .SetBackgroundColor(bg)
                .SetPadding(9)
                .SetBorder(Border.NO_BORDER)
                .Add(new Paragraph(text)
                    .SetFont(font).SetFontSize(fontSize).SetFontColor(color));

        private static IBlockElement InsightSection(string title, string body,
            PdfFont fontBold, PdfFont fontNormal)
        {
            var div = new Div().SetMarginTop(16);
            div.Add(new Paragraph(title)
                .SetFont(fontBold).SetFontSize(16).SetFontColor(DS.Navy));
            div.Add(new Paragraph(body)
                .SetFont(fontNormal).SetFontSize(11).SetFontColor(DS.Navy2)
                .SetFixedLeading(17f));
            return div;
        }

        private static IBlockElement BulletSection(string title,
            System.Collections.Generic.IReadOnlyList<string> items,
            DeviceRgb bulletColor, PdfFont fontBold, PdfFont fontNormal)
        {
            var div = new Div().SetMarginTop(16);
            div.Add(new Paragraph(title)
                .SetFont(fontBold).SetFontSize(16).SetFontColor(DS.Navy));

            foreach (var item in items ?? new System.Collections.Generic.List<string>())
            {
                var row = new iText.Layout.Element.Table(UnitValue.CreatePercentArray(new float[] { 0.05f, 0.95f }))
                    .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginTop(4);

                row.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPaddingTop(2)
                    .Add(new Paragraph("◆")
                        .SetFont(fontBold).SetFontSize(7).SetFontColor(bulletColor)));

                row.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPaddingLeft(4)
                    .Add(new Paragraph(item)
                        .SetFont(fontNormal).SetFontSize(11).SetFontColor(DS.Navy2)
                        .SetFixedLeading(16f)));

                div.Add(row);
            }

            return div;
        }
    }
}