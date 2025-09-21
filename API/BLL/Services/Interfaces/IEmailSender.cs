using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
