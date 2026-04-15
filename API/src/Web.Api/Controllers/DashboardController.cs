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
                // ADMIN LOGIC: Uses the optimized Database View
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

                return Ok(FormatDashboardResponse(
                    totalLifetime,
                    insights?.MonthlyRevenue ?? 0,
                    insights?.PercentageGrowth ?? 0,
                    recent));
            }
            else if (IsVendor())
            {
                // VENDOR LOGIC: Scoped to specific VendorId
                var vendorId = GetUserIdFromToken();

                // 1. Lifetime Revenue for this Vendor
                var lifetimeRevenue = await _context.OrderItems
                    .Where(oi => oi.VendorId == vendorId && oi.Order.PaymentStatus == "Success")
                    .SumAsync(oi => oi.Price * oi.Quantity);

                // 2. Current Month Revenue
                var currentMonthRevenue = await _context.OrderItems
                    .Where(oi => oi.VendorId == vendorId &&
                                 oi.Order.PaymentStatus == "Success" &&
                                 oi.Order.CreatedAt >= startOfCurrentMonth)
                    .SumAsync(oi => oi.Price * oi.Quantity);

                // 3. Last Month Revenue (for Percentage calculation)
                var lastMonthRevenue = await _context.OrderItems
                    .Where(oi => oi.VendorId == vendorId &&
                                 oi.Order.PaymentStatus == "Success" &&
                                 oi.Order.CreatedAt >= startOfLastMonth &&
                                 oi.Order.CreatedAt < startOfCurrentMonth)
                    .SumAsync(oi => oi.Price * oi.Quantity);

                // 4. Calculate Percentage Growth
                decimal growth = 0;
                if (lastMonthRevenue > 0)
                    growth = ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100;
                else if (currentMonthRevenue > 0)
                    growth = 100;

                // 5. Recent Items for this Vendor
                var recentItems = await _context.OrderItems
                    .Where(oi => oi.VendorId == vendorId)
                    .OrderByDescending(oi => oi.Order.CreatedAt)
                    .Take(5)
                    .Select(oi => new { oi.OrderId, User = oi.Order.UserId, Amount = oi.Price * oi.Quantity, oi.Order.CreatedAt })
                    .ToListAsync();

                return Ok(FormatDashboardResponse(lifetimeRevenue, currentMonthRevenue, growth, recentItems));
            }

            return Forbid();
        }

        // Helper to keep the output structure identical for Admin and Vendor
        private object FormatDashboardResponse(decimal total, decimal monthly, decimal growth, object recent)
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
                RecentActivity = recent
            };
        }
    }
}