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
                var insights = await _context.OrderInsights
                    .FirstOrDefaultAsync(x => x.Year == now.Year && x.Month == now.Month);

                var totalLifetime = await _context.Orders
                    .Where(o => o.PaymentStatus == "Success")
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
            else if (IsVendor())
            {
                var vendorId = GetUserIdFromToken();

                var lifetimeRevenue = await _context.OrderItems
                    .Where(oi => oi.VendorId == vendorId && oi.Order.PaymentStatus == "Success")
                    .SumAsync(oi => oi.Price * oi.Quantity);

                var currentMonthRevenue = await _context.OrderItems
                    .Where(oi => oi.VendorId == vendorId &&
                                 oi.Order.PaymentStatus == "Success" &&
                                 oi.Order.CreatedAt >= startOfCurrentMonth)
                    .SumAsync(oi => oi.Price * oi.Quantity);

                var lastMonthRevenue = await _context.OrderItems
                    .Where(oi => oi.VendorId == vendorId &&
                                 oi.Order.PaymentStatus == "Success" &&
                                 oi.Order.CreatedAt >= startOfLastMonth &&
                                 oi.Order.CreatedAt < startOfCurrentMonth)
                    .SumAsync(oi => oi.Price * oi.Quantity);

                decimal growth = 0;
                if (lastMonthRevenue > 0)
                    growth = ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100;
                else if (currentMonthRevenue > 0)
                    growth = 100;

                var recentItems = await _context.OrderItems
                    .Where(oi => oi.VendorId == vendorId)
                    .OrderByDescending(oi => oi.Order.CreatedAt)
                    .Take(5)
                    .Select(oi => new
                    {
                        oi.OrderId,
                        User = oi.Order.UserId,
                        Amount = oi.Price * oi.Quantity,
                        oi.Order.CreatedAt
                    })
                    .ToListAsync();

                var revenueHistory = await _context.OrderItems
                    .Where(oi => oi.VendorId == vendorId && oi.Order.PaymentStatus == "Success")
                    .GroupBy(oi => new { oi.Order.CreatedAt.Year, oi.Order.CreatedAt.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        MonthlyRevenue = g.Sum(oi => oi.Price * oi.Quantity),
                        OrderCount = g.Select(oi => oi.OrderId).Distinct().Count(),
                    })
                    .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                    .Take(12)
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();

                // Label is computed after the DB query because
                // new DateTime(...).ToString() can't translate to SQL
                var revenueHistoryWithLabel = revenueHistory.Select(x => new
                {
                    x.Year,
                    x.Month,
                    x.MonthlyRevenue,
                    x.OrderCount,
                    PercentageGrowth = (decimal?)null,   // not stored for vendors; calculated below if needed
                    Label = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy")
                });

                return Ok(FormatDashboardResponse(
                    lifetimeRevenue,
                    currentMonthRevenue,
                    growth,
                    recentItems,
                    revenueHistoryWithLabel));
            }

            return Forbid();
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