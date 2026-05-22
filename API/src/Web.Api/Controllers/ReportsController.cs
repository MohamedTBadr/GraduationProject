using Application.Contracts;
using Domain.Enums;
using Hangfire;
using Infrastructure.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public sealed class ReportsController : ControllerBase
    {
        private readonly IReportingService _reporting;
        private readonly IPdfReportService _pdf;
        private readonly IBackgroundJobClient _jobs;

        public ReportsController(
            IReportingService reporting,
            IPdfReportService pdf,
            IBackgroundJobClient jobs)
        {
            _reporting = reporting;
            _pdf = pdf;
            _jobs = jobs;
        }

        [HttpGet("executive")]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> GetExecutiveReport(CancellationToken ct)
        {
            var (vendorId, scope) = ResolveContext();

            if (scope is null) return Forbid();

            var report = await _reporting.GenerateFullReportAsync(vendorId, scope.Value, ct);

            return Ok(report);
        }

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
}
