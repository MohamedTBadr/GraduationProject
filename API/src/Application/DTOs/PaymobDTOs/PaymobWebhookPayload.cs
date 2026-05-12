using System.Text.Json.Serialization;

namespace Application.DTOs.PaymobDTOs
{
    public class PaymobWebhookPayload
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }           // ← long, not Guid (Paymob uses numeric IDs)

        [JsonPropertyName("type")]
        public string Type { get; set; }       // "TRANSACTION"

        [JsonPropertyName("obj")]
        public PaymobTransactionObj Obj { get; set; }
    }

    public class PaymobTransactionObj
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("error_occured")]
        public bool ErrorOccurred { get; set; }

        [JsonPropertyName("has_parent_transaction")]
        public bool HasParentTransaction { get; set; }

        [JsonPropertyName("is_captured")]
        public bool IsCaptured { get; set; }

        [JsonPropertyName("is_refunded")]
        public bool IsRefunded { get; set; }

        [JsonPropertyName("is_standalone_payment")]
        public bool IsStandalonePayment { get; set; }

        [JsonPropertyName("is_voided")]
        public bool IsVoided { get; set; }

        [JsonPropertyName("integration_id")]
        public long IntegrationId { get; set; }

        [JsonPropertyName("owner")]
        public long Owner { get; set; }

        [JsonPropertyName("pending")]
        public bool Pending { get; set; }

        [JsonPropertyName("order")]
        public PaymobOrderRef Order { get; set; }

        [JsonPropertyName("source_data")]
        public PaymobSourceData SourceData { get; set; }
    }

    public class PaymobOrderRef
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("merchant_order_id")]
        public string MerchantOrderId { get; set; }  // ← your internal Guid as string
    }

    public class PaymobSourceData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }        // "card"

        [JsonPropertyName("pan")]
        public string Pan { get; set; }         // last 4 digits

        [JsonPropertyName("sub_type")]
        public string SubType { get; set; }     // "MasterCard", "Visa"
    }
}