using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Controllers
{
    [Authorize(Policy = "DashboardAccess")]
    public class DashboardController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
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
                .Where(ei => ei.VendorId == vendorId && paidStatuses.Contains(ei.Event.Order.PaymentStatus))
                .SumAsync(ei => ei.Price * ei.Quantity);

            var currentMonthRevenue = await _context.EventItems
                .Where(ei => ei.VendorId == vendorId &&
                             paidStatuses.Contains(ei.Event.Order.PaymentStatus) &&
                             ei.Event.Order.CreatedAt >= startOfCurrentMonth)
                .SumAsync(ei => ei.Price * ei.Quantity);

            var lastMonthRevenue = await _context.EventItems
                .Where(ei => ei.VendorId == vendorId &&
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
                .Where(ei => ei.VendorId == vendorId && paidStatuses.Contains(ei.Event.Order.PaymentStatus))
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
                .Where(ei => ei.VendorId == vendorId && paidStatuses.Contains(ei.Event.Order.PaymentStatus))
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