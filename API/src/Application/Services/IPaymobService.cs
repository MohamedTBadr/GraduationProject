using Application.DTOs.PaymobDTOs;

namespace Application.Services
{
    public interface IPaymobService
    {
        Task<string> CreatePaymentAsync(Guid internalOrderId, decimal amount, BillingData billing, CancellationToken ct);
        Task HandleWebhookAsync(PaymobWebhookPayload payload, CancellationToken ct);
        bool ValidateHmac(PaymobWebhookPayload payload, string receivedHmac);
    }
}