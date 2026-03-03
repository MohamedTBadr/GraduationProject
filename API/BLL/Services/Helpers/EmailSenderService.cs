using BLL.Services.Interfaces;
//using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace BLL.Services.Helpers
{
    public class EmailSenderService(IConfiguration _config) : IEmailSender
        {
            async Task IEmailSender.SendEmailAsync(string email, string subject, string htmlMessage)
            {
                var smtpHost = _config["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]);
                var smtpUser = _config["EmailSettings:SmtpUser"];
                var smtpPass = _config["EmailSettings:SmtpPass"];
                var from = _config["EmailSettings:FromEmail"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage(from, email, subject, htmlMessage)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mailMessage);
            }
        }
    
}
