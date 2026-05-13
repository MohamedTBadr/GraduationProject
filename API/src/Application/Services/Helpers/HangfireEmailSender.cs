using Application.Interfaces;
using Hangfire;
using System.Threading.Tasks;

namespace Application.Services.Helpers
{
    public class HangfireEmailSender(IBackgroundJobClient backgroundJobClient) : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            backgroundJobClient.Enqueue<EmailSenderService>(x => x.SendEmailAsync(email, subject, htmlMessage));
            return Task.CompletedTask;
        }
    }
}
