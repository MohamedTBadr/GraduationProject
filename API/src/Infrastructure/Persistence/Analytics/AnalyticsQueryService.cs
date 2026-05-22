using Application.Contracts;
using Application.DTOs.Reports;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Analytics
{
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
                    VendorName = o.Event.EventItems
                        .Select(ei => ei.Service.Vendor.BusinessName)
                        .FirstOrDefault(),
                    ServiceName = o.Event.EventItems
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
}
