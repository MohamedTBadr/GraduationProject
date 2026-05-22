using Application.Contracts;
using Domain.Contracts;
using Domain.Entities;
using Domain.Enums;
using Hangfire;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Jobs
{
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

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 900])]
        public async Task SendMonthlyVendorReportsAsync(CancellationToken ct = default)
        {
            var vendors = await _db.Vendors
                .Include(v => v.User)
                .Where(v => v.IsVerified && v.User.Email != null)
                .Select(v => new { v.UserId, v.User.Email, v.BusinessName, v.User.FirstName })
                .ToListAsync(ct);

            _logger.LogInformation("Generating monthly reports for {Count} vendors", vendors.Count);

            var semaphore = new SemaphoreSlim(5);

            var tasks = vendors.Select(async vendor =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    await GenerateAndSendVendorReportAsync(
                        vendor.UserId, vendor.Email!, vendor.FirstName,
                        ReportFrequency.Monthly, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to generate report for vendor {VendorId}", vendor.UserId);
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

        public async Task GenerateAndSendVendorReportAsync(
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
}
