using Application.Contracts;
using Application.DTOs.Reports;
using Application.Interfaces;
using Hangfire;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Helpers
{
    public class HangfireEmailSender(IBackgroundJobClient backgroundJobClient) : IEmailSender, IEmailService
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            backgroundJobClient.Enqueue<EmailSenderService>(x => x.SendEmailAsync(email, subject, htmlMessage));
            return Task.CompletedTask;
        }

        public Task SendReportEmailAsync(
            string toEmail,
            string recipientName,
            ExecutiveReportDto report,
            byte[] pdfAttachment,
            CancellationToken ct = default)
        {
            backgroundJobClient.Enqueue<EmailSenderService>(x =>
                x.SendReportEmailAsync(toEmail, recipientName, report, pdfAttachment));

            return Task.CompletedTask;
        }
    }
}
