# Production-Grade BI Reporting System — Architecture Guide

> ASP.NET Core · EF Core · Groq Llama 3 · QuestPDF · Hangfire · SQL Server

---

## 1. Folder & Project Structure

```
Solution/
├── src/
│   ├── Domain/                         # Pure domain models — no framework deps
│   │   ├── Entities/
│   │   │   ├── Order.cs
│   │   │   ├── Vendor.cs
│   │   │   ├── ReportRecord.cs         # persisted report history
│   │   │   └── ScheduledReport.cs
│   │   ├── Enums/
│   │   │   ├── ReportScope.cs          # Admin | Vendor
│   │   │   └── ReportFrequency.cs      # OnDemand | Weekly | Monthly
│   │   └── ValueObjects/
│   │       └── DateRange.cs
│   │
│   ├── Application/                    # Use-cases, interfaces, DTOs
│   │   ├── Contracts/
│   │   │   ├── IAnalyticsService.cs
│   │   │   ├── IAiInsightService.cs
│   │   │   ├── IReportingService.cs
│   │   │   ├── IPdfReportService.cs
│   │   │   ├── IEmailService.cs
│   │   │   └── IReportHistoryRepository.cs
│   │   ├── DTOs/
│   │   │   ├── Reports/
│   │   │   │   ├── ExecutiveReportDto.cs
│   │   │   │   ├── KpiSectionDto.cs
│   │   │   │   ├── RevenueHistoryItemDto.cs
│   │   │   │   ├── TopServiceDto.cs
│   │   │   │   ├── RecentOrderDto.cs
│   │   │   │   └── AdminMetricsDto.cs
│   │   │   └── Ai/
│   │   │       ├── AiInsightRequestDto.cs
│   │   │       └── AiInsightResponseDto.cs
│   │   └── UseCases/
│   │       ├── GenerateExecutiveReport/
│   │       │   ├── GenerateExecutiveReportCommand.cs
│   │       │   └── GenerateExecutiveReportHandler.cs
│   │       └── ScheduleReport/
│   │           ├── ScheduleReportCommand.cs
│   │           └── ScheduleReportHandler.cs
│   │
│   ├── Infrastructure/                 # All external concerns
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Repositories/
│   │   │   │   └── ReportHistoryRepository.cs
│   │   │   └── Analytics/
│   │   │       └── AnalyticsQueryService.cs  # all EF Core heavy queries
│   │   ├── Ai/
│   │   │   └── GroqAiInsightService.cs
│   │   ├── Reporting/
│   │   │   ├── ReportingService.cs
│   │   │   └── PdfReportService.cs     # QuestPDF
│   │   ├── Email/
│   │   │   └── SmtpEmailService.cs
│   │   ├── Jobs/                       # Hangfire background jobs
│   │   │   ├── ScheduledReportJob.cs
│   │   │   └── JobRegistry.cs
│   │   └── Caching/
│   │       └── HybridCacheService.cs
│   │
│   └── Web.Api/
│       ├── Controllers/
│       │   ├── DashboardController.cs  # thin — delegates only
│       │   └── ReportsController.cs
│       └── Program.cs
│
└── tests/
    ├── Application.Tests/
    └── Infrastructure.Tests/
```

---

## 2. Domain Enums & Value Objects

```csharp
// Domain/Enums/ReportScope.cs
public enum ReportScope { Admin, Vendor }

// Domain/Enums/ReportFrequency.cs
public enum ReportFrequency { OnDemand, Weekly, Monthly }

// Domain/Entities/ReportRecord.cs
public class ReportRecord
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? VendorId { get; private set; }          // null = admin report
    public ReportScope Scope { get; private set; }
    public ReportFrequency Frequency { get; private set; }
    public string PdfStoragePath { get; private set; } = default!;
    public DateTime GeneratedAt { get; private set; } = DateTime.UtcNow;
    public bool EmailSent { get; private set; }

    public static ReportRecord Create(
        Guid? vendorId,
        ReportScope scope,
        ReportFrequency frequency,
        string pdfPath) =>
        new()
        {
            VendorId = vendorId,
            Scope = scope,
            Frequency = frequency,
            PdfStoragePath = pdfPath,
            EmailSent = false
        };

    public void MarkEmailSent() => EmailSent = true;
}
```

---

## 3. DTOs

```csharp
// Application/DTOs/Reports/KpiSectionDto.cs
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

// Application/DTOs/Reports/RevenueHistoryItemDto.cs
public sealed record RevenueHistoryItemDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string Label { get; init; } = default!;     // "Jan 2025"
    public decimal Revenue { get; init; }
    public int Orders { get; init; }
    public decimal? GrowthPercentage { get; init; }    // null for first month
}

// Application/DTOs/Reports/TopServiceDto.cs
public sealed record TopServiceDto
{
    public Guid ServiceId { get; init; }
    public string ServiceName { get; init; } = default!;
    public decimal Revenue { get; init; }
    public int Orders { get; init; }
    public int? QuantitySold { get; init; }
    public decimal RevenueShare { get; init; }         // % of total revenue
}

// Application/DTOs/Reports/RecentOrderDto.cs
public sealed record RecentOrderDto
{
    public Guid OrderId { get; init; }
    public string CustomerName { get; init; } = default!;
    public string? VendorName { get; init; }           // admin-only
    public string ServiceName { get; init; } = default!;
    public decimal Amount { get; init; }
    public DateTime CreatedAt { get; init; }
}

// Application/DTOs/Reports/AdminMetricsDto.cs
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

// Application/DTOs/Reports/ExecutiveReportDto.cs
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
```

```csharp
// Application/DTOs/Ai/AiInsightRequestDto.cs
public sealed record AiInsightRequestDto
{
    public ReportScope Scope { get; init; }
    public KpiSectionDto KPIs { get; init; } = default!;
    public IReadOnlyList<RevenueHistoryItemDto> RevenueHistory { get; init; } = [];
    public IReadOnlyList<TopServiceDto> TopServices { get; init; } = [];
    public AdminMetricsDto? AdminMetrics { get; init; }
}

// Application/DTOs/Ai/AiInsightResponseDto.cs
public sealed record AiInsightResponseDto
{
    public string Summary { get; init; } = default!;
    public IReadOnlyList<string> Risks { get; init; } = [];
    public IReadOnlyList<string> Opportunities { get; init; } = [];
    public IReadOnlyList<string> Recommendations { get; init; } = [];
    public string Conclusion { get; init; } = default!;
    public string ModelUsed { get; init; } = default!;
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}
```

---

## 4. Service Interfaces

```csharp
// Application/Contracts/IAnalyticsService.cs
public interface IAnalyticsService
{
    Task<ExecutiveReportDto> BuildAdminReportAsync(CancellationToken ct = default);
    Task<ExecutiveReportDto> BuildVendorReportAsync(Guid vendorId, CancellationToken ct = default);
}

// Application/Contracts/IAiInsightService.cs
public interface IAiInsightService
{
    Task<AiInsightResponseDto> GenerateInsightsAsync(
        AiInsightRequestDto request,
        CancellationToken ct = default);
}

// Application/Contracts/IReportingService.cs
public interface IReportingService
{
    Task<ExecutiveReportDto> GenerateFullReportAsync(
        Guid? vendorId,
        ReportScope scope,
        CancellationToken ct = default);
}

// Application/Contracts/IPdfReportService.cs
public interface IPdfReportService
{
    Task<byte[]> RenderAsync(ExecutiveReportDto report, CancellationToken ct = default);
}

// Application/Contracts/IEmailService.cs
public interface IEmailService
{
    Task SendReportEmailAsync(
        string toEmail,
        string recipientName,
        ExecutiveReportDto report,
        byte[] pdfAttachment,
        CancellationToken ct = default);
}

// Application/Contracts/IReportHistoryRepository.cs
public interface IReportHistoryRepository
{
    Task<ReportRecord> SaveAsync(ReportRecord record, CancellationToken ct = default);
    Task<IReadOnlyList<ReportRecord>> GetByVendorAsync(Guid vendorId, CancellationToken ct = default);
    Task<IReadOnlyList<ReportRecord>> GetRecentAdminReportsAsync(int count, CancellationToken ct = default);
}
```

---

## 5. Analytics Service — All EF Core Queries (Single Source of Truth)

```csharp
// Infrastructure/Persistence/Analytics/AnalyticsQueryService.cs
public sealed class AnalyticsQueryService : IAnalyticsService
{
    private readonly ApplicationDbContext _db;
    private static readonly string[] PaidStatuses = ["Paid", "Completed"];

    public AnalyticsQueryService(ApplicationDbContext db) => _db = db;

    // ─── ADMIN ─────────────────────────────────────────────────────────────

    public async Task<ExecutiveReportDto> BuildAdminReportAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Use pre-aggregated OrderInsights for admin (avoids full table scans)
        var insight = await _db.OrderInsights
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Year == now.Year && x.Month == now.Month, ct);

        var lifetimeRevenue = await _db.Orders
            .AsNoTracking()
            .Where(o => PaidStatuses.Contains(o.PaymentStatus))
            .SumAsync(o => o.Amount, ct);

        var recentOrders = await _db.Orders
            .AsNoTracking()
            .Where(o => PaidStatuses.Contains(o.PaymentStatus))
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new RecentOrderDto
            {
                OrderId = o.Id,
                CustomerName = o.User.FirstName + " " + o.User.LastName,
                VendorName = o.EventItems
                    .Select(ei => ei.Service.Vendor.BusinessName)
                    .FirstOrDefault(),
                ServiceName = o.EventItems
                    .Select(ei => ei.Service.Name)
                    .FirstOrDefault() ?? "—",
                Amount = o.Amount,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(ct);

        var rawHistory = await _db.OrderInsights
            .AsNoTracking()
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
            .Take(12)
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(ct);

        var revenueHistory = rawHistory
            .Select((x, i) => new RevenueHistoryItemDto
            {
                Year = x.Year,
                Month = x.Month,
                Label = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
                Revenue = x.MonthlyRevenue,
                Orders = x.OrderCount,
                GrowthPercentage = i == 0 || rawHistory[i - 1].MonthlyRevenue == 0
                    ? null
                    : Math.Round(
                        (x.MonthlyRevenue - rawHistory[i - 1].MonthlyRevenue)
                        / rawHistory[i - 1].MonthlyRevenue * 100, 2)
            })
            .ToList();

        var adminMetrics = new AdminMetricsDto
        {
            TotalVendors = await _db.Vendors.AsNoTracking().CountAsync(ct),
            VerifiedVendors = await _db.Vendors.AsNoTracking()
                .CountAsync(v => v.IsVerified, ct),
            TotalCustomers = await _db.Users.AsNoTracking().CountAsync(ct)
                - await _db.Vendors.AsNoTracking().CountAsync(ct),
            TotalOrders = await _db.Orders.AsNoTracking().CountAsync(ct)
        };

        var lastMonthRevenue = rawHistory.Count >= 2
            ? rawHistory[^2].MonthlyRevenue
            : 0;

        var currentMonthRevenue = insight?.MonthlyRevenue ?? 0;
        var growth = CalculateGrowth(currentMonthRevenue, lastMonthRevenue);

        return new ExecutiveReportDto
        {
            Scope = ReportScope.Admin,
            GeneratedAt = now,
            KPIs = new KpiSectionDto
            {
                LifetimeRevenue = lifetimeRevenue,
                CurrentMonthRevenue = currentMonthRevenue,
                LastMonthRevenue = lastMonthRevenue,
                GrowthPercentage = growth
            },
            RevenueHistory = revenueHistory,
            RecentOrders = recentOrders,
            AdminMetrics = adminMetrics,
            TopServices = await GetTopServicesAsync(null, ct)
        };
    }

    // ─── VENDOR ────────────────────────────────────────────────────────────

    public async Task<ExecutiveReportDto> BuildVendorReportAsync(
        Guid vendorId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
        var startOfLastMonth = startOfCurrentMonth.AddMonths(-1);

        // Single base query — re-used for all vendor metrics
        var baseQuery = _db.EventItems
            .AsNoTracking()
            .Where(ei =>
                ei.Service.VendorId == vendorId &&
                PaidStatuses.Contains(ei.Event.Order.PaymentStatus));

        var lifetimeRevenue = await baseQuery
            .SumAsync(ei => ei.Price * ei.Quantity, ct);

        var currentMonthRevenue = await baseQuery
            .Where(ei => ei.Event.Order.CreatedAt >= startOfCurrentMonth)
            .SumAsync(ei => ei.Price * ei.Quantity, ct);

        var lastMonthRevenue = await baseQuery
            .Where(ei =>
                ei.Event.Order.CreatedAt >= startOfLastMonth &&
                ei.Event.Order.CreatedAt < startOfCurrentMonth)
            .SumAsync(ei => ei.Price * ei.Quantity, ct);

        var totalOrders = await baseQuery
            .Select(ei => ei.Event.Order.Id)
            .Distinct()
            .CountAsync(ct);

        var recentOrders = await baseQuery
            .OrderByDescending(ei => ei.Event.Order.CreatedAt)
            .Take(10)
            .Select(ei => new RecentOrderDto
            {
                OrderId = ei.Event.Order.Id,
                CustomerName = ei.Event.Order.User.FirstName + " " + ei.Event.Order.User.LastName,
                ServiceName = ei.Service.Name,
                Amount = ei.Price * ei.Quantity,
                CreatedAt = ei.Event.Order.CreatedAt
            })
            .ToListAsync(ct);

        var rawHistory = await baseQuery
            .GroupBy(ei => new
            {
                ei.Event.Order.CreatedAt.Year,
                ei.Event.Order.CreatedAt.Month
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Revenue = g.Sum(x => x.Price * x.Quantity),
                Orders = g.Select(x => x.Event.Order.Id).Distinct().Count()
            })
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
            .Take(12)
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(ct);

        var revenueHistory = rawHistory
            .Select((x, i) => new RevenueHistoryItemDto
            {
                Year = x.Year,
                Month = x.Month,
                Label = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
                Revenue = x.Revenue,
                Orders = x.Orders,
                GrowthPercentage = i == 0 || rawHistory[i - 1].Revenue == 0
                    ? null
                    : Math.Round(
                        (x.Revenue - rawHistory[i - 1].Revenue)
                        / rawHistory[i - 1].Revenue * 100, 2)
            })
            .ToList();

        var avgMonthlyRevenue = revenueHistory.Any()
            ? revenueHistory.Average(x => x.Revenue)
            : 0;

        var growth = CalculateGrowth(currentMonthRevenue, lastMonthRevenue);

        return new ExecutiveReportDto
        {
            Scope = ReportScope.Vendor,
            VendorId = vendorId,
            GeneratedAt = now,
            KPIs = new KpiSectionDto
            {
                LifetimeRevenue = lifetimeRevenue,
                CurrentMonthRevenue = currentMonthRevenue,
                LastMonthRevenue = lastMonthRevenue,
                GrowthPercentage = growth,
                TotalOrders = totalOrders,
                AverageOrderValue = totalOrders > 0
                    ? Math.Round(lifetimeRevenue / totalOrders, 2)
                    : 0,
                AverageMonthlyRevenue = Math.Round((decimal)avgMonthlyRevenue, 2)
            },
            RevenueHistory = revenueHistory,
            RecentOrders = recentOrders,
            TopServices = await GetTopServicesAsync(vendorId, ct)
        };
    }

    // ─── SHARED ────────────────────────────────────────────────────────────

    private async Task<IReadOnlyList<TopServiceDto>> GetTopServicesAsync(
        Guid? vendorId, CancellationToken ct)
    {
        var query = _db.EventItems
            .AsNoTracking()
            .Where(ei => PaidStatuses.Contains(ei.Event.Order.PaymentStatus));

        if (vendorId.HasValue)
            query = query.Where(ei => ei.Service.VendorId == vendorId);

        var raw = await query
            .GroupBy(ei => new { ei.ServiceId, ei.Service.Name })
            .Select(g => new
            {
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.Name,
                Revenue = g.Sum(x => x.Price * x.Quantity),
                Orders = g.Select(x => x.Event.Order.Id).Distinct().Count(),
                QuantitySold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync(ct);

        var totalRevenue = raw.Sum(x => x.Revenue);

        return raw.Select(x => new TopServiceDto
        {
            ServiceId = x.ServiceId,
            ServiceName = x.ServiceName,
            Revenue = x.Revenue,
            Orders = x.Orders,
            QuantitySold = x.QuantitySold,
            RevenueShare = totalRevenue > 0
                ? Math.Round(x.Revenue / totalRevenue * 100, 2)
                : 0
        }).ToList();
    }

    private static decimal CalculateGrowth(decimal current, decimal last) =>
        last > 0 ? Math.Round((current - last) / last * 100, 2)
        : current > 0 ? 100
        : 0;
}
```

---

## 6. Orchestration — ReportingService

```csharp
// Infrastructure/Reporting/ReportingService.cs
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
        catch (Exception ex)
        {
            // Log but do not fail the report — AI is enhancement, not core
            // _logger.LogWarning(ex, "AI insight generation failed; report will be generated without insights.");
        }

        return report with { AiInsights = insights };
    }
}
```

---

## 7. AI Service — Groq / Llama 3 Integration

```csharp
// Infrastructure/Ai/GroqAiInsightService.cs
public sealed class GroqAiInsightService : IAiInsightService
{
    private readonly HttpClient _http;
    private readonly GroqOptions _options;
    private const string Model = "llama3-70b-8192";

    public GroqAiInsightService(HttpClient http, IOptions<GroqOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<AiInsightResponseDto> GenerateInsightsAsync(
        AiInsightRequestDto request,
        CancellationToken ct = default)
    {
        var prompt = BuildPrompt(request);

        var payload = new
        {
            model = Model,
            temperature = 0.3,          // low temp = factual, deterministic tone
            max_tokens = 1200,
            messages = new[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user",   content = prompt }
            }
        };

        var response = await _http.PostAsJsonAsync(
            "https://api.groq.com/openai/v1/chat/completions",
            payload,
            ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<GroqResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Empty response from Groq");

        var rawJson = result.Choices[0].Message.Content;

        return ParseInsights(rawJson, Model);
    }

    // ── SYSTEM PROMPT ──────────────────────────────────────────────────────
    //
    // CRITICAL RULES baked in:
    //  • Never invent or recalculate numbers
    //  • Only reference KPIs provided
    //  • Output strict JSON — no markdown, no prose outside JSON
    //
    private const string SystemPrompt = """
        You are a senior business intelligence analyst.
        Your role is to interpret pre-calculated financial KPIs and generate
        a structured executive report in JSON format.

        STRICT RULES — violating any rule makes the output unusable:
        1. NEVER invent, estimate, or recalculate any financial figures.
        2. ONLY reference the exact numbers provided in the input data.
        3. If you cannot determine something from the data, say "Insufficient data."
        4. Output ONLY valid JSON — no markdown, no prose outside the JSON object.
        5. All arrays must contain 2–4 items unless data is insufficient.
        6. Tone: professional, concise, executive-level. No filler phrases.

        Output format (strict):
        {
          "summary": "string",
          "risks": ["string", "string"],
          "opportunities": ["string", "string"],
          "recommendations": ["string", "string"],
          "conclusion": "string"
        }
        """;

    private static string BuildPrompt(AiInsightRequestDto req)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"REPORT SCOPE: {req.Scope}");
        sb.AppendLine();
        sb.AppendLine("=== KEY PERFORMANCE INDICATORS ===");
        sb.AppendLine($"Lifetime Revenue: {req.KPIs.LifetimeRevenue:C}");
        sb.AppendLine($"Current Month Revenue: {req.KPIs.CurrentMonthRevenue:C}");
        sb.AppendLine($"Last Month Revenue: {req.KPIs.LastMonthRevenue:C}");
        sb.AppendLine($"Month-over-Month Growth: {req.KPIs.GrowthPercentage}%");

        if (req.KPIs.TotalOrders.HasValue)
        {
            sb.AppendLine($"Total Orders: {req.KPIs.TotalOrders}");
            sb.AppendLine($"Average Order Value: {req.KPIs.AverageOrderValue:C}");
            sb.AppendLine($"Average Monthly Revenue: {req.KPIs.AverageMonthlyRevenue:C}");
        }

        sb.AppendLine();
        sb.AppendLine("=== REVENUE TREND (last 12 months) ===");

        foreach (var h in req.RevenueHistory)
        {
            var growth = h.GrowthPercentage.HasValue
                ? $"{h.GrowthPercentage:+0.00;-0.00}%"
                : "baseline";

            sb.AppendLine($"  {h.Label}: {h.Revenue:C} | {h.Orders} orders | growth: {growth}");
        }

        sb.AppendLine();
        sb.AppendLine("=== TOP SERVICES BY REVENUE ===");

        foreach (var s in req.TopServices)
            sb.AppendLine(
                $"  {s.ServiceName}: {s.Revenue:C} ({s.RevenueShare}% of total) | {s.Orders} orders");

        if (req.AdminMetrics is not null)
        {
            sb.AppendLine();
            sb.AppendLine("=== PLATFORM METRICS (Admin) ===");
            sb.AppendLine($"Total Vendors: {req.AdminMetrics.TotalVendors}");
            sb.AppendLine($"Verified Vendors: {req.AdminMetrics.VerifiedVendors} " +
                          $"({req.AdminMetrics.VendorVerificationRate}%)");
            sb.AppendLine($"Total Customers: {req.AdminMetrics.TotalCustomers}");
            sb.AppendLine($"Total Orders: {req.AdminMetrics.TotalOrders}");
        }

        sb.AppendLine();
        sb.AppendLine("Analyze the above data and return your JSON response now.");

        return sb.ToString();
    }

    private static AiInsightResponseDto ParseInsights(string rawJson, string model)
    {
        // Strip any accidental markdown fences
        var clean = rawJson
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        var parsed = JsonSerializer.Deserialize<AiInsightOutput>(clean,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Failed to parse AI response JSON");

        return new AiInsightResponseDto
        {
            Summary = parsed.Summary,
            Risks = parsed.Risks,
            Opportunities = parsed.Opportunities,
            Recommendations = parsed.Recommendations,
            Conclusion = parsed.Conclusion,
            ModelUsed = model,
            GeneratedAt = DateTime.UtcNow
        };
    }

    // Internal deserialization shape
    private sealed record AiInsightOutput(
        string Summary,
        List<string> Risks,
        List<string> Opportunities,
        List<string> Recommendations,
        string Conclusion);
}

// Infrastructure/Ai/GroqOptions.cs
public sealed class GroqOptions
{
    public string ApiKey { get; init; } = default!;
    public int TimeoutSeconds { get; init; } = 30;
}

// Infrastructure/Ai/GroqResponse.cs (matches Groq's OpenAI-compatible schema)
internal sealed record GroqResponse(
    [property: JsonPropertyName("choices")] List<GroqChoice> Choices);

internal sealed record GroqChoice(
    [property: JsonPropertyName("message")] GroqMessage Message);

internal sealed record GroqMessage(
    [property: JsonPropertyName("content")] string Content);
```

---

## 8. PDF Report Service — QuestPDF

```csharp
// Infrastructure/Reporting/PdfReportService.cs
// Package: QuestPDF
public sealed class PdfReportService : IPdfReportService
{
    public Task<byte[]> RenderAsync(ExecutiveReportDto report, CancellationToken ct = default)
    {
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

    // ── PAGE 1: Cover ──────────────────────────────────────────────────────
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

    // ── PAGE 2: KPIs ───────────────────────────────────────────────────────
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

    // ── PAGE 3: Revenue History table ─────────────────────────────────────
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

                    // Header
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

                    // Rows
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

    // ── PAGE 4: Top Services ───────────────────────────────────────────────
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

    // ── PAGE 5: AI Insights ────────────────────────────────────────────────
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

// ── QuestPDF Components ────────────────────────────────────────────────────

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
        _items = items;
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
```

---

## 9. Email Service

```csharp
// Infrastructure/Email/SmtpEmailService.cs
public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;

    public SmtpEmailService(IOptions<EmailOptions> options)
        => _options = options.Value;

    public async Task SendReportEmailAsync(
        string toEmail,
        string recipientName,
        ExecutiveReportDto report,
        byte[] pdfAttachment,
        CancellationToken ct = default)
    {
        using var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.To.Add(new MailboxAddress(recipientName, toEmail));

        message.Subject = report.Scope == ReportScope.Admin
            ? $"[Admin] Platform Executive Report — {report.GeneratedAt:MMMM yyyy}"
            : $"[Vendor] Executive Report — {report.GeneratedAt:MMMM yyyy}";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = BuildEmailHtml(report, recipientName)
        };

        var fileName = $"report_{report.GeneratedAt:yyyy-MM}.pdf";
        bodyBuilder.Attachments.Add(fileName, pdfAttachment, ContentType.Parse("application/pdf"));

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.Host, _options.Port, _options.UseSsl, ct);
        await client.AuthenticateAsync(_options.Username, _options.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }

    private static string BuildEmailHtml(ExecutiveReportDto report, string name) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family:Arial,sans-serif;color:#333;max-width:600px;margin:auto">
          <div style="background:#1A1A2E;padding:24px;border-radius:8px 8px 0 0">
            <h1 style="color:white;margin:0;font-size:22px">Executive Report</h1>
            <p style="color:#aaa;margin:4px 0 0">{report.GeneratedAt:MMMM yyyy}</p>
          </div>
          <div style="padding:24px;border:1px solid #eee;border-top:none">
            <p>Hello <strong>{name}</strong>,</p>
            <p>Your {(report.Scope == ReportScope.Admin ? "platform" : "vendor")}
               executive report for <strong>{report.GeneratedAt:MMMM yyyy}</strong> is ready.</p>

            <div style="display:flex;gap:12px;margin:20px 0">
              {KpiBox("Lifetime Revenue", report.KPIs.LifetimeRevenue.ToString("C"), "#2ECC71")}
              {KpiBox("This Month", report.KPIs.CurrentMonthRevenue.ToString("C"), "#3498DB")}
              {KpiBox("Growth", $"{report.KPIs.GrowthPercentage:+0.0;-0.0}%",
                  report.KPIs.IsGrowthPositive ? "#2ECC71" : "#E74C3C")}
            </div>

            {(report.AiInsights is not null
                ? $"<blockquote style='border-left:4px solid #3498DB;padding-left:12px;color:#555'>" +
                  $"{report.AiInsights.Summary}</blockquote>"
                : "")}

            <p>Please find the full PDF report attached.</p>
            <p style="color:#999;font-size:12px">This report was auto-generated. Do not reply to this email.</p>
          </div>
        </body>
        </html>
        """;

    private static string KpiBox(string label, string value, string color) => $"""
        <div style="flex:1;background:#f9f9f9;border-left:4px solid {color};
                    padding:12px;border-radius:4px">
          <div style="font-size:11px;color:#888">{label}</div>
          <div style="font-size:20px;font-weight:bold;color:{color}">{value}</div>
        </div>
        """;
}

// Infrastructure/Email/EmailOptions.cs
public sealed class EmailOptions
{
    public string Host { get; init; } = default!;
    public int Port { get; init; } = 587;
    public bool UseSsl { get; init; } = true;
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string SenderName { get; init; } = default!;
    public string SenderEmail { get; init; } = default!;
}
```

---

## 10. Report History Repository

```csharp
// Infrastructure/Persistence/Repositories/ReportHistoryRepository.cs
public sealed class ReportHistoryRepository : IReportHistoryRepository
{
    private readonly ApplicationDbContext _db;

    public ReportHistoryRepository(ApplicationDbContext db) => _db = db;

    public async Task<ReportRecord> SaveAsync(ReportRecord record, CancellationToken ct = default)
    {
        _db.ReportRecords.Add(record);
        await _db.SaveChangesAsync(ct);
        return record;
    }

    public async Task<IReadOnlyList<ReportRecord>> GetByVendorAsync(
        Guid vendorId, CancellationToken ct = default) =>
        await _db.ReportRecords
            .AsNoTracking()
            .Where(r => r.VendorId == vendorId)
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ReportRecord>> GetRecentAdminReportsAsync(
        int count, CancellationToken ct = default) =>
        await _db.ReportRecords
            .AsNoTracking()
            .Where(r => r.VendorId == null)
            .OrderByDescending(r => r.GeneratedAt)
            .Take(count)
            .ToListAsync(ct);
}
```

---

## 11. Hangfire Scheduled Report Job

```csharp
// Infrastructure/Jobs/ScheduledReportJob.cs
public sealed class ScheduledReportJob
{
    private readonly IReportingService _reporting;
    private readonly IPdfReportService _pdf;
    private readonly IEmailService _email;
    private readonly IReportHistoryRepository _history;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ScheduledReportJob> _logger;

    public ScheduledReportJob(
        IReportingService reporting,
        IPdfReportService pdf,
        IEmailService email,
        IReportHistoryRepository history,
        ApplicationDbContext db,
        ILogger<ScheduledReportJob> logger)
    {
        _reporting = reporting;
        _pdf = pdf;
        _email = email;
        _history = history;
        _db = db;
        _logger = logger;
    }

    // Called by Hangfire — Monthly
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 900])]
    public async Task SendMonthlyVendorReportsAsync(CancellationToken ct = default)
    {
        var vendors = await _db.Vendors
            .Where(v => v.IsVerified && v.User.Email != null)
            .Select(v => new { v.Id, v.User.Email, v.BusinessName, v.User.FirstName })
            .ToListAsync(ct);

        _logger.LogInformation("Generating monthly reports for {Count} vendors", vendors.Count);

        // Parallel with concurrency limit — avoid hammering Groq API
        var semaphore = new SemaphoreSlim(5);

        var tasks = vendors.Select(async vendor =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                await GenerateAndSendVendorReportAsync(
                    vendor.Id, vendor.Email!, vendor.FirstName,
                    ReportFrequency.Monthly, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to generate report for vendor {VendorId}", vendor.Id);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task SendAdminMonthlyReportAsync(string adminEmail, CancellationToken ct = default)
    {
        var report = await _reporting.GenerateFullReportAsync(null, ReportScope.Admin, ct);
        var pdfBytes = await _pdf.RenderAsync(report, ct);

        await _email.SendReportEmailAsync(adminEmail, "Admin", report, pdfBytes, ct);

        var record = ReportRecord.Create(null, ReportScope.Admin, ReportFrequency.Monthly,
            $"reports/admin/{report.GeneratedAt:yyyy-MM}.pdf");
        await _history.SaveAsync(record, ct);
    }

    private async Task GenerateAndSendVendorReportAsync(
        Guid vendorId,
        string email,
        string name,
        ReportFrequency frequency,
        CancellationToken ct)
    {
        var report = await _reporting.GenerateFullReportAsync(vendorId, ReportScope.Vendor, ct);
        var pdfBytes = await _pdf.RenderAsync(report, ct);

        await _email.SendReportEmailAsync(email, name, report, pdfBytes, ct);

        var record = ReportRecord.Create(
            vendorId, ReportScope.Vendor, frequency,
            $"reports/vendors/{vendorId}/{report.GeneratedAt:yyyy-MM}.pdf");

        await _history.SaveAsync(record, ct);
    }
}

// Infrastructure/Jobs/JobRegistry.cs
public static class JobRegistry
{
    public static void RegisterRecurringJobs()
    {
        // Monthly: 1st of every month at 06:00 UTC
        RecurringJob.AddOrUpdate<ScheduledReportJob>(
            "monthly-vendor-reports",
            job => job.SendMonthlyVendorReportsAsync(CancellationToken.None),
            Cron.Monthly(1, 6));

        // Monthly admin report
        RecurringJob.AddOrUpdate<ScheduledReportJob>(
            "monthly-admin-report",
            job => job.SendAdminMonthlyReportAsync("admin@platform.com", CancellationToken.None),
            Cron.Monthly(1, 7));
    }
}
```

---

## 12. Thin Controller

```csharp
// Web.Api/Controllers/ReportsController.cs
[ApiController]
[Route("api/reports")]
[Authorize(Policy = "DashboardAccess")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportingService _reporting;
    private readonly IPdfReportService _pdf;
    private readonly IEmailService _email;
    private readonly IBackgroundJobClient _jobs;

    public ReportsController(
        IReportingService reporting,
        IPdfReportService pdf,
        IEmailService email,
        IBackgroundJobClient jobs)
    {
        _reporting = reporting;
        _pdf = pdf;
        _email = email;
        _jobs = jobs;
    }

    /// <summary>Returns the full executive report as JSON.</summary>
    [HttpGet("executive")]
    [Authorize(Roles = "Admin,Vendor")]
    public async Task<IActionResult> GetExecutiveReport(CancellationToken ct)
    {
        var (vendorId, scope) = ResolveContext();

        if (scope is null) return Forbid();

        var report = await _reporting.GenerateFullReportAsync(vendorId, scope.Value, ct);

        return Ok(report);
    }

    /// <summary>Generates PDF and returns it as a download.</summary>
    [HttpGet("executive/pdf")]
    [Authorize(Roles = "Admin,Vendor")]
    public async Task<IActionResult> DownloadExecutiveReportPdf(CancellationToken ct)
    {
        var (vendorId, scope) = ResolveContext();

        if (scope is null) return Forbid();

        var report = await _reporting.GenerateFullReportAsync(vendorId, scope.Value, ct);
        var pdfBytes = await _pdf.RenderAsync(report, ct);

        return File(pdfBytes, "application/pdf",
            $"executive-report-{report.GeneratedAt:yyyy-MM}.pdf");
    }

    /// <summary>Enqueues report generation + email delivery as a background job.</summary>
    [HttpPost("executive/send-email")]
    [Authorize(Roles = "Admin,Vendor")]
    public IActionResult EnqueueReportEmail()
    {
        var (vendorId, scope) = ResolveContext();

        if (scope is null) return Forbid();

        var email = User.FindFirstValue(ClaimTypes.Email)!;
        var name = User.FindFirstValue(ClaimTypes.GivenName) ?? "User";

        _jobs.Enqueue<ScheduledReportJob>(job =>
            job.GenerateAndSendVendorReportAsync(
                vendorId!.Value, email, name,
                ReportFrequency.OnDemand,
                CancellationToken.None));

        return Accepted(new { message = "Report is being generated. You'll receive an email shortly." });
    }

    private (Guid? vendorId, ReportScope? scope) ResolveContext()
    {
        if (User.IsInRole("Admin"))
            return (null, ReportScope.Admin);

        if (User.IsInRole("Vendor"))
        {
            var id = Guid.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier), out var g) ? g : (Guid?)null;

            return id.HasValue ? (id, ReportScope.Vendor) : (null, null);
        }

        return (null, null);
    }
}
```

---

## 13. Dependency Registration

```csharp
// Web.Api/Program.cs (service registration excerpt)

// Analytics
builder.Services.AddScoped<IAnalyticsService, AnalyticsQueryService>();

// Reporting pipeline
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();
builder.Services.AddScoped<IReportHistoryRepository, ReportHistoryRepository>();

// AI — typed HttpClient with auth header injection
builder.Services
    .AddHttpClient<IAiInsightService, GroqAiInsightService>(client =>
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer",
                builder.Configuration["Groq:ApiKey"]);
        client.Timeout = TimeSpan.FromSeconds(
            builder.Configuration.GetValue<int>("Groq:TimeoutSeconds", 30));
    })
    .AddStandardResilienceHandler();   // Polly: retry + circuit breaker

// Email
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<GroqOptions>(builder.Configuration.GetSection("Groq"));

// Hangfire
builder.Services.AddHangfire(cfg =>
    cfg.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer(opts =>
{
    opts.WorkerCount = 4;
    opts.Queues = ["reports", "default"];
});

// QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;  // or Professional

// Register recurring jobs after app builds
var app = builder.Build();
JobRegistry.RegisterRecurringJobs();
```

---

## 14. Caching Strategy

```csharp
// appsettings.json
{
  "HybridCache": {
    "DefaultLocalTtl": "00:05:00",
    "DefaultDistributedTtl": "00:30:00"
  }
}

// Cache keys — deterministic, role-scoped
public static class CacheKeys
{
    public static string AdminDashboard() =>
        "dashboard:admin";

    public static string VendorDashboard(Guid vendorId) =>
        $"dashboard:vendor:{vendorId}";

    public static string AdminReport() =>
        "report:admin:executive";

    public static string VendorReport(Guid vendorId) =>
        $"report:vendor:{vendorId}:executive";
}

// Usage in AnalyticsQueryService with HybridCache (.NET 9)
public async Task<ExecutiveReportDto> BuildVendorReportAsync(Guid vendorId, CancellationToken ct)
{
    return await _cache.GetOrCreateAsync(
        CacheKeys.VendorReport(vendorId),
        async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await BuildVendorReportInternalAsync(vendorId, ct);
        },
        ct);
}

// Invalidate on order payment confirmed
public async Task InvalidateVendorCacheAsync(Guid vendorId)
{
    await _cache.RemoveAsync(CacheKeys.VendorDashboard(vendorId));
    await _cache.RemoveAsync(CacheKeys.VendorReport(vendorId));
}
```

---

## 15. Performance & Scaling Best Practices

### Query Optimization

```csharp
// 1. Always use AsNoTracking() for read-only analytics queries
var query = _db.EventItems.AsNoTracking()...

// 2. ProjectTo<DTO> directly — avoid loading full entities
// 3. Use compiled queries for hot paths
private static readonly Func<ApplicationDbContext, Guid, Task<decimal>>
    GetVendorLifetimeRevenueQuery =
    EF.CompileAsyncQuery((ApplicationDbContext db, Guid vendorId) =>
        db.EventItems
            .Where(ei => ei.Service.VendorId == vendorId &&
                         new[] { "Paid", "Completed" }.Contains(ei.Event.Order.PaymentStatus))
            .Sum(ei => ei.Price * ei.Quantity));

// 4. Index recommendations (add to EF migrations)
// modelBuilder.Entity<EventItem>()
//   .HasIndex(ei => new { ei.Event.Order.PaymentStatus, ei.Service.VendorId });
// modelBuilder.Entity<Order>()
//   .HasIndex(o => new { o.PaymentStatus, o.CreatedAt });
```

### SQL Server Index Recommendations

```sql
-- EventItems: most analytics queries filter by VendorId + PaymentStatus
CREATE INDEX IX_EventItems_VendorId_PaymentStatus
ON EventItems (ServiceId)
INCLUDE (Price, Quantity)
WHERE EXISTS (
    SELECT 1 FROM Orders o
    WHERE o.PaymentStatus IN ('Paid','Completed')
);

-- Orders: time-range revenue queries
CREATE INDEX IX_Orders_PaymentStatus_CreatedAt
ON Orders (PaymentStatus, CreatedAt)
INCLUDE (Amount, UserId);
```

### OrderInsights Materialized Table (Admin Performance)

```sql
-- Nightly job updates this table — O(1) read for admin dashboard
CREATE TABLE OrderInsights (
    Year            INT     NOT NULL,
    Month           INT     NOT NULL,
    MonthlyRevenue  DECIMAL(18,2) NOT NULL,
    OrderCount      INT     NOT NULL,
    PercentageGrowth DECIMAL(6,2) NULL,
    UpdatedAt       DATETIME2 NOT NULL,
    PRIMARY KEY (Year, Month)
);
```

---

## 16. Package Reference Summary

```xml
<!-- Core packages needed -->
<PackageReference Include="QuestPDF" Version="2024.*" />
<PackageReference Include="MailKit" Version="4.*" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.*" />
<PackageReference Include="Hangfire.SqlServer" Version="1.*" />
<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="8.*" />
<PackageReference Include="Microsoft.Extensions.Caching.Hybrid" Version="9.*" />
```

---

## 17. Architecture Decision Summary

| Concern | Decision | Reason |
|---|---|---|
| KPI Calculation | Backend only (EF Core) | Deterministic, auditable, no hallucination risk |
| AI Role | Interpretation only | LLM receives pre-computed data; cannot alter numbers |
| AI Failure | Graceful degradation | Report generated without insights if Groq is unavailable |
| PDF | QuestPDF | No license headaches, full C# control, no headless browser |
| Email | MailKit | Production-grade SMTP, supports attachments cleanly |
| Scheduling | Hangfire | Persistent jobs, retry logic, dashboard UI |
| Caching | HybridCache (.NET 9) | L1 memory + L2 Redis, vendor-scoped keys |
| Report History | SQL Server entity | Queryable, auditable, no file system dependency |
| AI Temperature | 0.3 | Reduces creativity/hallucination in favor of factual tone |
| Concurrency | SemaphoreSlim(5) | Prevents Groq rate-limit hits during bulk vendor reports |
