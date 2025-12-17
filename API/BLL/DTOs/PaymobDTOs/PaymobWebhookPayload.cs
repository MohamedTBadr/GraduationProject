    using System.Text.Json.Serialization;
namespace BLL.DTOs.PaymobDTOs
{

    public class PaymobWebhookPayload
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("amount_cents")]
        public int Amount_Cents { get; set; }

        [JsonPropertyName("order")]
        public long Order { get; set; }

        [JsonPropertyName("hmac")]
        public string Hmac { get; set; }
    }

}
