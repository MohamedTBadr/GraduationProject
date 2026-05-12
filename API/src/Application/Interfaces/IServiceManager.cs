using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Services.Helpers;

namespace Application.Interfaces
{
    public interface IServiceManager
    {
        IEmailSender EmailSender { get; }
        IAuthenticationService AuthenticationService { get; }
        IAttachmentService AttachmentService { get; }
        IVendorService VendorService { get; }
        IServiceTypeService ServiceTypeService { get; }
        IEventService EventService { get; }
        IEventItemService EventItemService { get; }
        IChatService ChatService { get; }
        IServiceService ServiceService { get; }
        IOrderService OrderService { get; }
        IFileService FileService { get; }
        IEventTypeService EventTypeService { get; }
        IVendorTypeService VendorTypeService { get; }
        ICompanyInquiryService CompanyInquiryService { get; }
        ISupportTicketService SupportTicketService { get; }
        IVoucherService VoucherService { get; }
        NotificationService NotificationService { get; }
        LlamaService LlamaService { get; }
    }
}