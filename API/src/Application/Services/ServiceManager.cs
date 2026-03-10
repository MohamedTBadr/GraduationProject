using Application.Interfaces;

namespace Application.Services
{
    public class ServiceManager(
        IAttachmentService attachmentService,
        IEmailSender emailSender,
        IAuthenticationService authenticationService,
        IServiceTypeService serviceTypeService,
        ICategoryService categoryService
    ) : IServiceManager
    {
        public IAttachmentService AttachmentService => attachmentService;
        public IEmailSender EmailSender => emailSender;
        public IAuthenticationService AuthenticationService => authenticationService;

        public IServiceTypeService ServiceTypeService => serviceTypeService;
        public ICategoryService CategoryService => categoryService;


    }
}
