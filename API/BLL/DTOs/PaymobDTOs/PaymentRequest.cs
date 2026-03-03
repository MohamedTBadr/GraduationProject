namespace BLL.DTOs.PaymobDTOs
{
    public record PaymentRequest
    {
        public decimal Amount { get; set; }
        public BillingData Billing { get; set; }
    }
}
