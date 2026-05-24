using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task InviteCollaboratorAsync(string email, string eventTitle, string role);
        Task SendCongratulatoryEmailAsync(string userEmail, string userFirstName, string eventTitle);
    }
}
