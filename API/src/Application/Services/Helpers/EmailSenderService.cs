using Application.DTOs.Reports;
using Domain.Enums;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Application.Services.Helpers
{
    public class EmailSenderService(IConfiguration config)
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(config["EmailSettings:FromEmail"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlMessage };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(config["EmailSettings:SmtpHost"], int.Parse(config["EmailSettings:SmtpPort"]), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(config["EmailSettings:SmtpUser"], config["EmailSettings:SmtpPass"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendReportEmailAsync(
            string toEmail,
            string recipientName,
            ExecutiveReportDto report,
            byte[] pdfAttachment)
        {
            using var message = new MimeMessage();

            message.From.Add(new MailboxAddress(config["EmailSettings:SenderName"], config["EmailSettings:FromEmail"]));
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
            await client.ConnectAsync(config["EmailSettings:SmtpHost"], int.Parse(config["EmailSettings:SmtpPort"]), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(config["EmailSettings:SmtpUser"], config["EmailSettings:SmtpPass"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
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
