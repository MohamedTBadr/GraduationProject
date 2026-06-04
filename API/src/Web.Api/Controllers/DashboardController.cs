using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Attributes;
using Web.Api.Controllers.Attributes;

namespace Web.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Vendor")]
    [Route("api/[controller]")]
    public class DashboardController : APIController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
            
        }

        [HttpGet("stats")]
        [HybridCache(300, "dashboard-stats", Variance = CacheVariance.Adaptive)]
        public async Task<IActionResult> GetDashboardStats()
        {
            var now = DateTime.UtcNow;
            var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfCurrentMonth.AddMonths(-1);

            if (IsAdmin())
            {
                return await AdminStat(now);
            }
            else if (IsVendor())
            {
                return await VendorStats(startOfCurrentMonth, startOfLastMonth);
            }

            return Forbid();
        }


        [HttpPost("executive-report")]
        public async Task<IActionResult> GenerateAdminExecutiveReport()
        {
            if (!IsAdmin())
                return Forbid();

            return await ProcessAdminExecutiveReport();
        }

        [HttpPost("vendor-report")]
        public async Task<IActionResult> GenerateVendorExecutiveReport()
        {
            if (!IsVendor())
                return Forbid();

            return await ProcessVendorExecutiveReport();
        }

        private async Task<IActionResult> ProcessAdminExecutiveReport()
        {
            var now = DateTime.UtcNow;

            var startOfCurrentMonth =
                new DateTime(now.Year, now.Month, 1);

            var startOfLastMonth =
                startOfCurrentMonth.AddMonths(-1);

            var paidStatuses = new[]
            {
        "Paid",
        "Completed"
    };

            // ───────────────────────────────────────
            // BASE QUERY
            // ───────────────────────────────────────

            var eventItemsQuery = _context.EventItems
                .Where(ei =>
                    paidStatuses.Contains(
                        ei.Event.Order.PaymentStatus));

            // ───────────────────────────────────────
            // REVENUE METRICS
            // ───────────────────────────────────────

            var lifetimeRevenue = await eventItemsQuery
                .SumAsync(ei => ei.Price * ei.Quantity);

            var currentMonthRevenue = await eventItemsQuery
                .Where(ei =>
                    ei.Event.Order.CreatedAt >=
                    startOfCurrentMonth)
                .SumAsync(ei => ei.Price * ei.Quantity);

            var lastMonthRevenue = await eventItemsQuery
                .Where(ei =>
                    ei.Event.Order.CreatedAt >=
                    startOfLastMonth &&
                    ei.Event.Order.CreatedAt <
                    startOfCurrentMonth)
                .SumAsync(ei => ei.Price * ei.Quantity);

            decimal growthPercentage = 0;

            if (lastMonthRevenue > 0)
            {
                growthPercentage =
                    ((currentMonthRevenue - lastMonthRevenue)
                    / lastMonthRevenue) * 100;
            }
            else if (currentMonthRevenue > 0)
            {
                growthPercentage = 100;
            }

            // ───────────────────────────────────────
            // RECENT ORDERS
            // ───────────────────────────────────────

            var recentOrders = await eventItemsQuery
                .OrderByDescending(ei =>
                    ei.Event.Order.CreatedAt)
                .Take(10)
                .Select(ei => new
                {
                    OrderId = ei.Event.Order.Id,

                    Customer =
                        ei.Event.Order.User.FirstName +
                        " " +
                        ei.Event.Order.User.LastName,

                    Vendor =
                        ei.Service.Vendor.BusinessName,

                    Service =
                        ei.Service.Name,

                    Amount =
                        ei.Price * ei.Quantity,

                    CreatedAt =
                        ei.Event.Order.CreatedAt
                })
                .ToListAsync();

            // ───────────────────────────────────────
            // REVENUE HISTORY
            // ───────────────────────────────────────

            var rawHistory = await eventItemsQuery
                .GroupBy(ei => new
                {
                    ei.Event.Order.CreatedAt.Year,
                    ei.Event.Order.CreatedAt.Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,

                    Revenue =
                        g.Sum(x =>
                            x.Price * x.Quantity),

                    Orders =
                        g.Select(x =>
                            x.Event.Order.Id)
                        .Distinct()
                        .Count()
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Take(12)
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var revenueHistory = rawHistory
                .Select((x, i) => new
                {
                    x.Year,
                    x.Month,
                    x.Revenue,
                    x.Orders,

                    Label =
                        new DateTime(
                            x.Year,
                            x.Month,
                            1)
                        .ToString("MMM yyyy"),

                    Growth =
                        i == 0 ||
                        rawHistory[i - 1].Revenue == 0
                            ? (decimal?)null
                            : Math.Round(
                                ((x.Revenue -
                                  rawHistory[i - 1].Revenue)
                                 / rawHistory[i - 1].Revenue)
                                * 100,
                                2)
                })
                .ToList();

            // ───────────────────────────────────────
            // TOP SERVICES
            // ───────────────────────────────────────

            var topServices = await eventItemsQuery
                .GroupBy(ei => new
                {
                    ei.ServiceId,
                    ei.Service.Name
                })
                .Select(g => new
                {
                    ServiceId = g.Key.ServiceId,

                    Service = g.Key.Name,

                    Revenue =
                        g.Sum(x =>
                            x.Price * x.Quantity),

                    Orders =
                        g.Select(x =>
                            x.Event.Order.Id)
                        .Distinct()
                        .Count()
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            // ───────────────────────────────────────
            // ADMIN METRICS
            // ───────────────────────────────────────

            var adminMetrics = new
            {
                TotalVendors =
                    await _context.Vendors
                        .CountAsync(),

                TotalCustomers =
                    await _context.Users
                        .CountAsync() -
                    await _context.Vendors
                        .CountAsync(),

                TotalOrders =
                    await _context.Orders
                        .CountAsync(),

                ActiveVendors =
                    await _context.EventItems
                        .Select(x =>
                            x.Service.VendorId)
                        .Distinct()
                        .CountAsync()
            };

            // ───────────────────────────────────────
            // EXECUTIVE SUMMARY
            // ───────────────────────────────────────

            var executiveSummary = $@"
Platform generated total revenue of {lifetimeRevenue:C}.

Current month revenue: {currentMonthRevenue:C}.

Growth: {Math.Round(growthPercentage, 2)}%.

The platform currently contains {((dynamic)adminMetrics).TotalVendors} vendors.";

            // ───────────────────────────────────────
            // FINAL RESPONSE
            // ───────────────────────────────────────

            return Ok(new
            {
                Scope = "Admin",

                GeneratedAt = now,

                KPIs = new
                {
                    LifetimeRevenue = lifetimeRevenue,
                    CurrentMonthRevenue = currentMonthRevenue,
                    LastMonthRevenue = lastMonthRevenue,
                    GrowthPercentage =
                        Math.Round(growthPercentage, 2)
                },

                RevenueHistory = revenueHistory,

                TopServices = topServices,

                RecentOrders = recentOrders,

                AdminMetrics = adminMetrics,

                ExecutiveSummary = executiveSummary
            });
        }

        private async Task<IActionResult> ProcessVendorExecutiveReport()
        {
            var vendorId = GetUserIdFromToken();

            var now = DateTime.UtcNow;
            var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfCurrentMonth.AddMonths(-1);

            var paidStatuses = new[] { "Paid", "Completed" };

            // ─────────────────────────────────────────────
            // REVENUE METRICS
            // ─────────────────────────────────────────────

            var lifetimeRevenue = await _context.EventItems
                .Where(ei =>
                    ei.Service.VendorId == vendorId &&
                    paidStatuses.Contains(ei.Event.Order.PaymentStatus))
                .SumAsync(ei => ei.Price * ei.Quantity);

            var currentMonthRevenue = await _context.EventItems
                .Where(ei =>
                    ei.Service.VendorId == vendorId &&
                    paidStatuses.Contains(ei.Event.Order.PaymentStatus) &&
                    ei.Event.Order.CreatedAt >= startOfCurrentMonth)
                .SumAsync(ei => ei.Price * ei.Quantity);

            var lastMonthRevenue = await _context.EventItems
                .Where(ei =>
                    ei.Service.VendorId == vendorId &&
                    paidStatuses.Contains(ei.Event.Order.PaymentStatus) &&
                    ei.Event.Order.CreatedAt >= startOfLastMonth &&
                    ei.Event.Order.CreatedAt < startOfCurrentMonth)
                .SumAsync(ei => ei.Price * ei.Quantity);

            decimal growthPercentage = 0;

            if (lastMonthRevenue > 0)
            {
                growthPercentage =
                    ((currentMonthRevenue - lastMonthRevenue)
                    / lastMonthRevenue) * 100;
            }
            else if (currentMonthRevenue > 0)
            {
                growthPercentage = 100;
            }

            // ─────────────────────────────────────────────
            // RECENT ORDERS
            // ─────────────────────────────────────────────

            var recentOrders = await _context.EventItems
                .Where(ei =>
                    ei.Service.VendorId == vendorId &&
                    paidStatuses.Contains(ei.Event.Order.PaymentStatus))
                .OrderByDescending(ei => ei.Event.Order.CreatedAt)
                .Take(10)
                .Select(ei => new
                {
                    OrderId = ei.Event.Order.Id,
                    Customer =
                        ei.Event.Order.User.FirstName + " " +
                        ei.Event.Order.User.LastName,
                    Service = ei.Service.Name,
                    Quantity = ei.Quantity,
                    Amount = ei.Price * ei.Quantity,
                    CreatedAt = ei.Event.Order.CreatedAt
                })
                .ToListAsync();

            // ─────────────────────────────────────────────
            // REVENUE HISTORY
            // ─────────────────────────────────────────────

            var rawRevenueHistory = await _context.EventItems
                .Where(ei =>
                    ei.Service.VendorId == vendorId &&
                    paidStatuses.Contains(ei.Event.Order.PaymentStatus))
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
                    Orders = g.Select(x => x.Event.Order.Id)
                        .Distinct()
                        .Count()
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Take(12)
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var revenueHistory = rawRevenueHistory
                .Select((x, i) => new
                {
                    x.Year,
                    x.Month,
                    x.Revenue,
                    x.Orders,
                    Label = new DateTime(x.Year, x.Month, 1)
                        .ToString("MMM yyyy"),

                    Growth =
                        i == 0 ||
                        rawRevenueHistory[i - 1].Revenue == 0
                            ? (decimal?)null
                            : Math.Round(
                                ((x.Revenue -
                                  rawRevenueHistory[i - 1].Revenue)
                                 / rawRevenueHistory[i - 1].Revenue)
                                * 100,
                                2)
                })
                .ToList();

            // ─────────────────────────────────────────────
            // TOP SERVICES
            // ─────────────────────────────────────────────

            var topServices = await _context.EventItems
                .Where(ei =>
                    ei.Service.VendorId == vendorId &&
                    paidStatuses.Contains(ei.Event.Order.PaymentStatus))
                .GroupBy(ei => new
                {
                    ei.ServiceId,
                    ei.Service.Name
                })
                .Select(g => new
                {
                    ServiceId = g.Key.ServiceId,
                    Service = g.Key.Name,

                    Revenue = g.Sum(x => x.Price * x.Quantity),

                    Orders = g.Select(x => x.Event.Order.Id)
                        .Distinct()
                        .Count(),

                    QuantitySold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            // ─────────────────────────────────────────────
            // BUSINESS INSIGHTS
            // ─────────────────────────────────────────────

            var bestMonth = revenueHistory
                .OrderByDescending(x => x.Revenue)
                .FirstOrDefault();

            var worstMonth = revenueHistory
                .OrderBy(x => x.Revenue)
                .FirstOrDefault();

            var averageMonthlyRevenue =
                revenueHistory.Any()
                    ? revenueHistory.Average(x => x.Revenue)
                    : 0;

            var totalOrders = await _context.EventItems
                .Where(ei =>
                    ei.Service.VendorId == vendorId &&
                    paidStatuses.Contains(ei.Event.Order.PaymentStatus))
                .Select(ei => ei.Event.Order.Id)
                .Distinct()
                .CountAsync();

            var averageOrderValue =
                totalOrders > 0
                    ? lifetimeRevenue / totalOrders
                    : 0;

            // ─────────────────────────────────────────────
            // AI EXECUTIVE SUMMARY
            // ─────────────────────────────────────────────

            var executiveSummary = $@"
This vendor generated a total lifetime revenue of
{lifetimeRevenue:C} with a current monthly revenue of
{currentMonthRevenue:C}.

Revenue growth for the current month is
{Math.Round(growthPercentage, 2)}%.

The vendor processed {totalOrders} paid orders with an
average order value of {averageOrderValue:C}.

Best performing month:
{bestMonth?.Label} with revenue of
{bestMonth?.Revenue:C}.

Lowest performing month:
{worstMonth?.Label} with revenue of
{worstMonth?.Revenue:C}.

Average monthly revenue:
{averageMonthlyRevenue:C}.

Top performing services:
{string.Join(", ",
            topServices.Select(x =>
                $"{x.Service} ({x.Revenue:C})"))}.
";

            // ─────────────────────────────────────────────
            // AI RECOMMENDATIONS
            // ─────────────────────────────────────────────

            var recommendations = new List<string>();

            if (growthPercentage < 0)
            {
                recommendations.Add(
                    "Revenue has declined compared to last month. Consider promotional campaigns and customer retention strategies.");
            }

            if (averageOrderValue < 100)
            {
                recommendations.Add(
                    "Average order value is relatively low. Consider bundle pricing or premium offerings.");
            }

            if (topServices.Count >= 1)
            {
                recommendations.Add(
                    $"Focus marketing efforts on '{topServices.First().Service}' since it is currently the highest revenue generating service.");
            }

            if (!recommendations.Any())
            {
                recommendations.Add(
                    "Vendor performance is stable with healthy revenue trends.");
            }

            // ─────────────────────────────────────────────
            // FINAL RESPONSE
            // ─────────────────────────────────────────────

            return Ok(new
            {
                GeneratedAt = now,

                Vendor = new
                {
                    VendorId = vendorId
                },

                KPIs = new
                {
                    LifetimeRevenue = lifetimeRevenue,
                    CurrentMonthRevenue = currentMonthRevenue,
                    LastMonthRevenue = lastMonthRevenue,
                    GrowthPercentage = Math.Round(growthPercentage, 2),
                    TotalOrders = totalOrders,
                    AverageOrderValue = Math.Round(averageOrderValue, 2),
                    AverageMonthlyRevenue =
                        Math.Round(averageMonthlyRevenue, 2)
                },

                Insights = new
                {
                    BestMonth = bestMonth,
                    WorstMonth = worstMonth
                },

                TopServices = topServices,

                RevenueHistory = revenueHistory,

                RecentOrders = recentOrders,

                ExecutiveSummary = executiveSummary,

                Recommendations = recommendations
            });
        }

        private async Task<IActionResult> AdminStat(DateTime now)
        {
            var insights = await _context.OrderInsights
                .FirstOrDefaultAsync(x => x.Year == now.Year && x.Month == now.Month);

            var paidStatuses = new[] { "Paid", "Completed" };

            var totalLifetime = await _context.Orders
                .Where(o => paidStatuses.Contains(o.PaymentStatus))
                .SumAsync(o => o.Amount);

            var recent = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new { o.Id, User = o.UserId, o.Amount, o.CreatedAt })
                .ToListAsync();

            var revenueHistory = await _context.OrderInsights
                .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                .Take(12)
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .Select(x => new
                {
                    x.Year,
                    x.Month,
                    x.MonthlyRevenue,
                    x.OrderCount,
                    x.PercentageGrowth,
                    Label = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy")
                })
                .ToListAsync();

            return Ok(FormatDashboardResponse(
                totalLifetime,
                insights?.MonthlyRevenue ?? 0,
                insights?.PercentageGrowth ?? 0,
                recent,
                revenueHistory));
        }

        private async Task<IActionResult> VendorStats(DateTime startOfCurrentMonth, DateTime startOfLastMonth)
        {
            var vendorId = GetUserIdFromToken();

            var paidStatuses = new[] { "Paid", "Completed" };

            var lifetimeRevenue = await _context.EventItems
                .Where(ei => ei.Service.VendorId == vendorId && paidStatuses.Contains(ei.Event.Order.PaymentStatus))
                .SumAsync(ei => ei.Price * ei.Quantity);

            var currentMonthRevenue = await _context.EventItems
                .Where(ei => ei.Service.VendorId == vendorId &&
                             paidStatuses.Contains(ei.Event.Order.PaymentStatus) &&
                             ei.Event.Order.CreatedAt >= startOfCurrentMonth)
                .SumAsync(ei => ei.Price * ei.Quantity);

            var lastMonthRevenue = await _context.EventItems
                .Where(ei => ei.Service.VendorId == vendorId &&
                             paidStatuses.Contains(ei.Event.Order.PaymentStatus) &&
                             ei.Event.Order.CreatedAt >= startOfLastMonth &&
                             ei.Event.Order.CreatedAt < startOfCurrentMonth)
                .SumAsync(ei => ei.Price * ei.Quantity);

            decimal growth = 0;
            if (lastMonthRevenue > 0)
                growth = ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100;
            else if (currentMonthRevenue > 0)
                growth = 100;

            // ── Fix 1: filter by PaymentStatus so unpaid orders are excluded ──
            var recentItems = await _context.EventItems
                .Where(ei => ei.Service.VendorId == vendorId && paidStatuses.Contains(ei.Event.Order.PaymentStatus))
                .OrderByDescending(ei => ei.Event.Order.CreatedAt)
                .Take(5)
                .Select(ei => new
                {
                    OrderId = ei.Event.Order.Id,
                    CustomerName = ei.Event.Order.User.FirstName + " " + ei.Event.Order.User.LastName,   // readable name instead of raw GUID
                    Amount = ei.Price * ei.Quantity,
                    CreatedAt = ei.Event.Order.CreatedAt
                })
                .ToListAsync();

            var revenueHistory = await _context.EventItems
                .Where(ei => ei.Service.VendorId == vendorId && paidStatuses.Contains(ei.Event.Order.PaymentStatus))
                .GroupBy(ei => new { ei.Event.Order.CreatedAt.Year, ei.Event.Order.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    MonthlyRevenue = g.Sum(ei => ei.Price * ei.Quantity),
                    OrderCount = g.Select(ei => ei.Event.Order.Id).Distinct().Count()
                })
                .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                .Take(12)
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // ── Fix 2: compute PercentageGrowth per row instead of always null ──
            var revenueHistoryWithLabel = revenueHistory
                .Select((x, i) => new
                {
                    x.Year,
                    x.Month,
                    x.MonthlyRevenue,
                    x.OrderCount,
                    Label = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
                    PercentageGrowth = i == 0 || revenueHistory[i - 1].MonthlyRevenue == 0
                        ? (decimal?)null
                        : Math.Round(
                            ((x.MonthlyRevenue - revenueHistory[i - 1].MonthlyRevenue)
                                / revenueHistory[i - 1].MonthlyRevenue) * 100, 2)
                });

            return Ok(FormatDashboardResponse(
                lifetimeRevenue,
                currentMonthRevenue,
                growth,
                recentItems,
                revenueHistoryWithLabel));
        }
        private object FormatDashboardResponse(
            decimal total,
            decimal monthly,
            decimal growth,
            object recent,
            object revenueHistory)
        {
            return new
            {
                TotalLifetimeRevenue = total,
                CurrentMonthRevenue = monthly,
                Growth = new
                {
                    Percentage = Math.Round(growth, 2),
                    Status = growth >= 0 ? "Increased" : "Decreased",
                    IsUp = growth >= 0,
                    Message = $"Your revenue is {(growth >= 0 ? "up" : "down")} by {Math.Abs(Math.Round(growth, 1))}% compared to last month"
                },
                RecentActivity = recent,
                RevenueHistory = revenueHistory
            };
        }
    }
}