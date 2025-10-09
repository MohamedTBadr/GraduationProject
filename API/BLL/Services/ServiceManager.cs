using BLL.Services.Interfaces;

namespace BLL.Services
{
    public class ServiceManager(
        IAttachmentService attachmentService,
        IEmailSender emailSender,
        IAuthenticationService authenticationService,
        ICacheService cacheService) : IServiceManager
    {
        public IAttachmentService AttachmentService => attachmentService;
        public IEmailSender EmailSender => emailSender;
        public IAuthenticationService AuthenticationService => authenticationService;
        public ICacheService CacheService => cacheService;
    }
}
