namespace Application.DTOs.PaymobDTOs
{
    public record PaymentRequest(Guid OrderId, BillingData Billing);

}
