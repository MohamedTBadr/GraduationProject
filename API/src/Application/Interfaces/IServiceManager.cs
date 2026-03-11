using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IServiceManager
    {
        IAttachmentService AttachmentService { get; }
        IEmailSender EmailSender { get; }
        IAuthenticationService AuthenticationService { get; }
        IServiceTypeService ServiceTypeService { get; }
        ICategoryService CategoryService { get; }

    }
}
