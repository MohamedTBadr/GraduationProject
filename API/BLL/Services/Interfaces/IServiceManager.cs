using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IServiceManager
    {
        IAttachmentService AttachmentService { get; }
        IEmailSender EmailSender { get; }
        IAuthenticationService AuthenticationService { get; }
        ICacheService CacheService { get; }
    }
}
