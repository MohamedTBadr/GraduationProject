using Application.DTOs.Reports;
using Domain.Entities;
using Domain.Enums;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Application.Services.Helpers
{
    public class EmailSenderService(IConfiguration config)
    {
        // ─────────────────────────────────────────────────────────────
        //  Core send primitive
        // ─────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────
        //  Collaborator invitation
        // ─────────────────────────────────────────────────────────────
        public async Task InviteCollaboratorAsync(string email, string eventTitle, string role)
        {
            string subject = $"You've Been Invited as a Collaborator — {eventTitle}";
            string htmlBody = BuildInviteHtml(eventTitle, role);

            await SendEmailAsync(email, subject, htmlBody);
        }

        // ─────────────────────────────────────────────────────────────
        //  Congratulatory email on event completion
        // ─────────────────────────────────────────────────────────────
        public async Task SendCongratulatoryEmailAsync(string userEmail, string userFirstName, string eventTitle)
        {
            string subject = $"Congratulations on Your Completed Event: {eventTitle}!";
            string htmlBody = BuildCongratulatoryHtml(userFirstName, eventTitle);

            await SendEmailAsync(userEmail, subject, htmlBody);
        }

        // ─────────────────────────────────────────────────────────────
        //  Executive report email with PDF attachment
        // ─────────────────────────────────────────────────────────────
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
                HtmlBody = BuildReportHtml(report, recipientName)
            };

            bodyBuilder.Attachments.Add(
                $"report_{report.GeneratedAt:yyyy-MM}.pdf",
                pdfAttachment,
                ContentType.Parse("application/pdf"));

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(config["EmailSettings:SmtpHost"], int.Parse(config["EmailSettings:SmtpPort"]), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(config["EmailSettings:SmtpUser"], config["EmailSettings:SmtpPass"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        // ─────────────────────────────────────────────────────────────
        //  Shared layout helpers
        // ─────────────────────────────────────────────────────────────

        /// <summary>Renders the standard Epic Hub email header.</summary>
        private static string Header(string title, string subtitle) => $"""
            <div style="background:#1A2540;padding:32px 36px">
              <table width="100%" cellpadding="0" cellspacing="0">
                <tr>
                  <td>
                    <div style="display:inline-block;width:48px;height:48px;background:#0E1627;
                                border:2px solid #C9A84C;border-radius:10px;
                                text-align:center;line-height:48px;
                                font-family:'Cormorant Garamond',Georgia,serif;
                                font-size:20px;font-weight:700;color:#E8C97A;
                                vertical-align:middle">EH</div>
                    <span style="vertical-align:middle;margin-left:12px">
                      <span style="display:block;font-family:'Cormorant Garamond',Georgia,serif;
                                   font-size:17px;font-weight:700;color:#F9F6F0;letter-spacing:0.04em;
                                   line-height:1.1">Epic Hub</span>
                      <span style="font-family:'Outfit',Arial,sans-serif;
                                   font-size:11px;letter-spacing:0.08em;color:#C9A84C;
                                   font-style:italic">Don't Plan, Go Epic</span>
                    </span>
                  </td>
                </tr>
                <tr>
                  <td style="padding-top:20px">
                    <h1 style="margin:0;font-family:'Cormorant Garamond',Georgia,serif;
                               font-size:32px;font-weight:700;color:#F9F6F0;line-height:1.2">
                      {title}
                    </h1>
                    <p style="margin:6px 0 0;font-size:13px;color:#6B7280;letter-spacing:0.04em">
                      {subtitle}
                    </p>
                  </td>
                </tr>
              </table>
              <div style="margin-top:24px;height:1.5px;background:#C9A84C;border-radius:2px"></div>
            </div>
            """;

        /// <summary>Renders the standard Epic Hub email footer.</summary>
        private static string Footer() => """
            <div style="background:#0E1627;padding:20px 36px">
              <table width="100%" cellpadding="0" cellspacing="0">
                <tr>
                  <td>
                    <span style="font-family:'Cormorant Garamond',Georgia,serif;
                                 font-size:15px;font-weight:700;color:#E8C97A">Epic Hub</span>
                    <span style="font-family:'Outfit',Arial,sans-serif;
                                 font-size:10px;color:#C9A84C;font-style:italic;margin-left:8px">
                      Don't Plan, Go Epic
                    </span>
                  </td>
                  <td align="right">
                    <span style="font-size:11px;color:#6B7280;letter-spacing:0.04em">
                      Confidential — Internal use only
                    </span>
                  </td>
                </tr>
              </table>
            </div>
            """;

        private static string Wrapper(string inner) => $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width,initial-scale=1" />
              <link href="https://fonts.googleapis.com/css2?family=Cormorant+Garamond:wght@400;600;700&family=Outfit:wght@400;500;600&display=swap" rel="stylesheet" />
            </head>
            <body style="margin:0;padding:0;background-color:#F0EBE0;font-family:'Outfit',Arial,sans-serif;color:#1A2540">
              <div style="max-width:620px;margin:40px auto;border-radius:16px;overflow:hidden;
                          box-shadow:0 8px 40px rgba(26,37,64,0.18)">
                {inner}
              </div>
            </body>
            </html>
            """;

        // ─────────────────────────────────────────────────────────────
        //  Per-email HTML builders
        // ─────────────────────────────────────────────────────────────

        private static string BuildInviteHtml(string eventTitle, string role) => Wrapper($"""
            {Header("You're Invited!", "You have a new collaboration request")}

            <div style="background:#F9F6F0;padding:36px">
              <p style="margin:0 0 28px;font-size:14px;color:#243050;line-height:1.7">
                You've been invited to collaborate on an upcoming event. Here are the details:
              </p>

              <!-- Invitation card -->
              <div style="background:#FFFFFF;border-top:3px solid #C9A84C;border-radius:10px;
                          padding:20px 22px;margin-bottom:28px;
                          box-shadow:0 4px 24px rgba(26,37,64,0.08)">
                <table width="100%" cellpadding="0" cellspacing="0">
                  <tr>
                    <td style="padding-bottom:14px">
                      <div style="font-size:10px;font-weight:600;letter-spacing:0.08em;
                                  text-transform:uppercase;color:#6B7280;margin-bottom:6px">Event</div>
                      <div style="font-family:'Cormorant Garamond',Georgia,serif;
                                  font-size:24px;font-weight:700;color:#1A2540;line-height:1.2">
                        {eventTitle}
                      </div>
                    </td>
                  </tr>
                  <tr>
                    <td style="border-top:1px solid #E8E4DC;padding-top:14px">
                      <div style="font-size:10px;font-weight:600;letter-spacing:0.08em;
                                  text-transform:uppercase;color:#6B7280;margin-bottom:6px">Your Role</div>
                      <div style="display:inline-block;background:#F0EBE0;
                                  border:1px solid #C9A84C;border-radius:6px;
                                  padding:4px 12px;font-size:13px;font-weight:600;color:#C9A84C;
                                  letter-spacing:0.04em">{role}</div>
                    </td>
                  </tr>
                </table>
              </div>

              <p style="margin:0 0 28px;font-size:14px;color:#243050;line-height:1.7">
                Register now to join the team and start planning an unforgettable experience!
              </p>

              <div style="height:1px;background:#E8E4DC;margin-bottom:20px"></div>
              <p style="margin:0;font-size:11px;color:#6B7280;line-height:1.6">
                Best regards,<br />
                <strong style="color:#1A2540">The Epic Hub Team</strong>
              </p>
            </div>

            {Footer()}
            """);

        private static string BuildCongratulatoryHtml(string userFirstName, string eventTitle) => Wrapper($"""
            {Header("Congratulations! 🎉", "Your event has been successfully completed")}

            <div style="background:#F9F6F0;padding:36px">
              <p style="margin:0 0 6px;font-size:15px;color:#1A2540">
                Hello <strong style="font-weight:600">{userFirstName}</strong>,
              </p>
              <p style="margin:0 0 28px;font-size:14px;color:#243050;line-height:1.7">
                We're thrilled to let you know that your event has been successfully completed!
              </p>

              <!-- Event highlight card -->
              <div style="background:#FFFFFF;border-top:3px solid #C9A84C;border-radius:10px;
                          padding:20px 22px;margin-bottom:28px;
                          box-shadow:0 4px 24px rgba(26,37,64,0.08)">
                <div style="font-size:10px;font-weight:600;letter-spacing:0.08em;
                            text-transform:uppercase;color:#6B7280;margin-bottom:8px">Completed Event</div>
                <div style="font-family:'Cormorant Garamond',Georgia,serif;
                            font-size:24px;font-weight:700;color:#1A2540;line-height:1.2">
                  {eventTitle}
                </div>
              </div>

              <p style="margin:0 0 16px;font-size:14px;color:#243050;line-height:1.7">
                Thank you for choosing <strong>Epic Hub</strong> to plan and manage your event.
                We hope it was a memorable and wonderful experience for you and all of your guests.
              </p>
              <p style="margin:0 0 28px;font-size:14px;color:#243050;line-height:1.7">
                We look forward to helping you plan your next amazing experience!
              </p>

              <div style="height:1px;background:#E8E4DC;margin-bottom:20px"></div>
              <p style="margin:0;font-size:11px;color:#6B7280;line-height:1.6">
                Best regards,<br />
                <strong style="color:#1A2540">The Epic Hub Team</strong>
              </p>
            </div>

            {Footer()}
            """);

        private static string BuildReportHtml(ExecutiveReportDto report, string name) => Wrapper($"""
            {Header("Executive Report", $"{report.GeneratedAt:MMMM yyyy}")}

            <div style="background:#F9F6F0;padding:36px">
              <p style="margin:0 0 6px;font-size:15px;color:#1A2540">
                Hello <strong style="font-weight:600">{name}</strong>,
              </p>
              <p style="margin:0 0 28px;font-size:14px;color:#243050;line-height:1.7">
                Your <strong>{(report.Scope == ReportScope.Admin ? "platform" : "vendor")}</strong>
                executive report for <strong>{report.GeneratedAt:MMMM yyyy}</strong> is ready.
                Find a summary of your key metrics below, and the full PDF attached.
              </p>

              <!-- KPI cards -->
              <table width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:28px">
                <tr>
                  <td style="padding-right:8px" valign="top">
                    {KpiBox("Lifetime Revenue", report.KPIs.LifetimeRevenue.ToString("C"), "#16A34A")}
                  </td>
                  <td style="padding-right:8px" valign="top">
                    {KpiBox("This Month", report.KPIs.CurrentMonthRevenue.ToString("C"), "#C9A84C")}
                  </td>
                  <td valign="top">
                    {KpiBox("Growth",
                        $"{report.KPIs.GrowthPercentage:+0.0;-0.0}%",
                        report.KPIs.IsGrowthPositive ? "#16A34A" : "#CA8A04")}
                  </td>
                </tr>
              </table>

              <!-- AI insight -->
              {(report.AiInsights is not null ? $"""
              <div style="margin-bottom:28px;padding:18px 20px;background:#F0EBE0;
                          border-radius:10px;border-left:4px solid #C9A84C">
                <p style="margin:0 0 6px;font-size:11px;font-weight:600;letter-spacing:0.08em;
                           color:#C9A84C;text-transform:uppercase">AI Insight</p>
                <p style="margin:0;font-size:13px;color:#243050;line-height:1.7">
                  {report.AiInsights.Summary}
                </p>
              </div>
              """ : "")}

              <p style="margin:0 0 32px;font-size:14px;color:#243050;line-height:1.7">
                The full PDF report is attached to this email with detailed revenue history,
                top services breakdown, and AI-powered recommendations.
              </p>

              <div style="height:1px;background:#E8E4DC;margin-bottom:20px"></div>
              <p style="margin:0;font-size:11px;color:#6B7280;line-height:1.6">
                This report was auto-generated by Epic Hub on {report.GeneratedAt:MMMM dd, yyyy HH:mm} UTC.
                Do not reply to this email.
              </p>
            </div>

            {Footer()}
            """);

        private static string KpiBox(string label, string value, string color) => $"""
            <div style="background:#FFFFFF;border-top:3px solid {color};border-radius:10px;
                        padding:16px 14px;box-shadow:0 4px 24px rgba(26,37,64,0.08)">
              <div style="font-size:10px;font-weight:600;letter-spacing:0.08em;
                          text-transform:uppercase;color:#6B7280;margin-bottom:8px">{label}</div>
              <div style="font-size:22px;font-weight:700;color:{color};
                          font-family:'Cormorant Garamond',Georgia,serif;line-height:1">{value}</div>
            </div>
            """;
    }
}