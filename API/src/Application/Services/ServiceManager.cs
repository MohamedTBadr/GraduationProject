using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Services.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services
{
    public class ServiceManager : IServiceManager
    {
        private readonly Lazy<IEmailSender> _emailSender;
        private readonly Lazy<IAuthenticationService> _authenticationService;
        private readonly Lazy<IAttachmentService> _attachmentService;
        private readonly Lazy<IVendorService> _vendorService;
        private readonly Lazy<IServiceTypeService> _serviceTypeService;
        private readonly Lazy<IEventService> _eventService;
        private readonly Lazy<IEventItemService> _eventItemService;
        private readonly Lazy<IChatService> _chatService;
        private readonly Lazy<IServiceService> _serviceService;
        private readonly Lazy<IOrderService> _orderService;
        private readonly Lazy<IFileService> _fileService;
        private readonly Lazy<IEventTypeService> _eventTypeService;
        private readonly Lazy<IVendorTypeService> _vendorTypeService;
        private readonly Lazy<ICompanyInquiryService> _companyInquiryService;
        private readonly Lazy<ISupportTicketService> _supportTicketService;
        private readonly Lazy<IVoucherService> _voucherService;
        private readonly Lazy<NotificationService> _notificationService;
        private readonly Lazy<LlamaService> _llamaService;
        private readonly Lazy<ISearchService> _searchService;
        private readonly Lazy<IPlanningAIService> _planningAIService;
        private readonly Lazy<IPackageService> _packageService;

        public ServiceManager(IServiceProvider serviceProvider)
        {
            _emailSender = new Lazy<IEmailSender>(() => serviceProvider.GetRequiredService<IEmailSender>());
            _authenticationService = new Lazy<IAuthenticationService>(() => serviceProvider.GetRequiredService<IAuthenticationService>());
            _attachmentService = new Lazy<IAttachmentService>(() => serviceProvider.GetRequiredService<IAttachmentService>());
            _vendorService = new Lazy<IVendorService>(() => serviceProvider.GetRequiredService<IVendorService>());
            _serviceTypeService = new Lazy<IServiceTypeService>(() => serviceProvider.GetRequiredService<IServiceTypeService>());
            _eventService = new Lazy<IEventService>(() => serviceProvider.GetRequiredService<IEventService>());
            _eventItemService = new Lazy<IEventItemService>(() => serviceProvider.GetRequiredService<IEventItemService>());
            _chatService = new Lazy<IChatService>(() => serviceProvider.GetRequiredService<IChatService>());
            _serviceService = new Lazy<IServiceService>(() => serviceProvider.GetRequiredService<IServiceService>());
            _orderService = new Lazy<IOrderService>(() => serviceProvider.GetRequiredService<IOrderService>());
            _fileService = new Lazy<IFileService>(() => serviceProvider.GetRequiredService<IFileService>());
            _eventTypeService = new Lazy<IEventTypeService>(() => serviceProvider.GetRequiredService<IEventTypeService>());
            _vendorTypeService = new Lazy<IVendorTypeService>(() => serviceProvider.GetRequiredService<IVendorTypeService>());
            _companyInquiryService = new Lazy<ICompanyInquiryService>(() => serviceProvider.GetRequiredService<ICompanyInquiryService>());
            _supportTicketService = new Lazy<ISupportTicketService>(() => serviceProvider.GetRequiredService<ISupportTicketService>());
            _voucherService = new Lazy<IVoucherService>(() => serviceProvider.GetRequiredService<IVoucherService>());
            _notificationService = new Lazy<NotificationService>(() => serviceProvider.GetRequiredService<NotificationService>());
            _llamaService = new Lazy<LlamaService>(() => serviceProvider.GetRequiredService<LlamaService>());
            _searchService = new Lazy<ISearchService>(() => serviceProvider.GetRequiredService<ISearchService>());
            _planningAIService = new Lazy<IPlanningAIService>(() => serviceProvider.GetRequiredService<IPlanningAIService>());
            _packageService = new Lazy<IPackageService>(() => serviceProvider.GetRequiredService<IPackageService>());
        }

        public IEmailSender EmailSender => _emailSender.Value;
        public IAuthenticationService AuthenticationService => _authenticationService.Value;
        public IAttachmentService AttachmentService => _attachmentService.Value;
        public IVendorService VendorService => _vendorService.Value;
        public IServiceTypeService ServiceTypeService => _serviceTypeService.Value;
        public IEventService EventService => _eventService.Value;
        public IEventItemService EventItemService => _eventItemService.Value;
        public IChatService ChatService => _chatService.Value;
        public IServiceService ServiceService => _serviceService.Value;
        public IOrderService OrderService => _orderService.Value;
        public IFileService FileService => _fileService.Value;
        public IEventTypeService EventTypeService => _eventTypeService.Value;
        public IVendorTypeService VendorTypeService => _vendorTypeService.Value;
        public ICompanyInquiryService CompanyInquiryService => _companyInquiryService.Value;
        public ISupportTicketService SupportTicketService => _supportTicketService.Value;
        public IVoucherService VoucherService => _voucherService.Value;
        public NotificationService NotificationService => _notificationService.Value;
        public LlamaService LlamaService => _llamaService.Value;
        public ISearchService SearchService => _searchService.Value;
        public IPlanningAIService PlanningAIService => _planningAIService.Value;
        public IPackageService PackageService => _packageService.Value;
    }
}