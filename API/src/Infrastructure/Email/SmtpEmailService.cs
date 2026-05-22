using Application.Contracts;
using Application.DTOs.Reports;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Email
{
    public sealed class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendReportEmailAsync(
            string toEmail,
            string recipientName,
            ExecutiveReportDto report,
            byte[] pdfAttachment,
            CancellationToken ct = default)
        {
            using var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_config["EmailSettings:SenderName"], _config["EmailSettings:FromEmail"]));
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

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(_config["EmailSettings:SmtpHost"], int.Parse(_config["EmailSettings:SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_config["EmailSettings:SmtpUser"], _config["EmailSettings:SmtpPass"], ct);
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
}
