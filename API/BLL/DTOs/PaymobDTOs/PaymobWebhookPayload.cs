namespace BLL.DTOs.PaymobDTOs
{
    public class PaymobWebhookPayload
{
    public long Id { get; set; } // transaction ID
    public bool Success { get; set; }
    public int AmountCents { get; set; }
    public PaymobOrder Order { get; set; }
}
}
