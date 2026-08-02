using Application.DTOs.Orders;
using Application.DTOs.PaymobDTOs;
using Application.Interfaces.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public class PaymobService(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymobOptions> options,
    IOrderService orderService) : IPaymobService
{
    private readonly PaymobOptions _options = options.Value;

    // ─── Public Methods ────────────────────────────────────────────────────────

    public async Task<string> CreatePaymentAsync(
        Guid internalOrderId,
        decimal amount,
        BillingData billing,
        CancellationToken ct)
    {
        var authToken = await AuthenticateAsync(ct);
        var paymobOrderId = await CreateOrderAsync(authToken, internalOrderId, amount, ct);
        var paymentKey = await GeneratePaymentKeyAsync(authToken, paymobOrderId, amount, billing, ct);

        return $"{_options.BaseUrl}acceptance/iframes/{_options.IframeId}?payment_token={paymentKey}";
    }

    public async Task HandleWebhookAsync(PaymobWebhookPayload payload, CancellationToken ct)
    {
        if (!Guid.TryParse(payload.Obj.Order.MerchantOrderId, out var internalOrderId))
            throw new InvalidOperationException("Invalid merchant_order_id in Paymob callback.");

        var status = payload.Obj.Success
            ? new UpdateOrderStatusRequest("Paid")
            : new UpdateOrderStatusRequest("Failed");

        await orderService.UpdatePaymentStatusAsync(internalOrderId, status, ct);

        // Store Paymob transaction ID for refund/support reference
        await orderService.SetPaymentIntentAsync(internalOrderId, payload.Obj.Id.ToString(), ct);
    }

    public bool ValidateHmac(PaymobWebhookPayload payload, string receivedHmac)
    {
        // Paymob specifies exact fields in exact order — do NOT change
        var message = string.Concat(
            payload.Obj.AmountCents,
            payload.Obj.CreatedAt,
            payload.Obj.Currency,
            payload.Obj.ErrorOccurred.ToString().ToLower(),
            payload.Obj.HasParentTransaction.ToString().ToLower(),
            payload.Obj.Id,
            payload.Obj.IntegrationId,
            payload.Obj.IsCaptured.ToString().ToLower(),
            payload.Obj.IsRefunded.ToString().ToLower(),
            payload.Obj.IsStandalonePayment.ToString().ToLower(),
            payload.Obj.IsVoided.ToString().ToLower(),
            payload.Obj.Order.Id,
            payload.Obj.Owner,
            payload.Obj.Pending.ToString().ToLower(),
            payload.Obj.SourceData?.Pan,
            payload.Obj.SourceData?.SubType,
            payload.Obj.SourceData?.Type,
            payload.Obj.Success.ToString().ToLower()
        );

        var keyBytes = Encoding.UTF8.GetBytes(_options.HmacSecret);
        var msgBytes = Encoding.UTF8.GetBytes(message);
        var hash = new HMACSHA512(keyBytes).ComputeHash(msgBytes);
        var computed = Convert.ToHexString(hash).ToLower();

        return computed == receivedHmac;
    }

    // ─── Private Helpers ───────────────────────────────────────────────────────

    private async Task<string> AuthenticateAsync(CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("PaymobClient");

        var response = await client.PostAsJsonAsync(
            "auth/tokens",
            new { api_key = _options.ApiKey },
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Paymob Auth Error: {error}");
        }

        var data = await response.Content.ReadFromJsonAsync<AuthResponse>(ct);
        return data!.token;
    }

    private async Task<string> CreateOrderAsync(
        string token,
        Guid internalOrderId,
        decimal amount,
        CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("PaymobClient");

        var request = new HttpRequestMessage(HttpMethod.Post, "ecommerce/orders")
        {
            Content = JsonContent.Create(new
            {
                merchant_order_id = internalOrderId.ToString(),  // ← reconciliation key
                amount_cents = (int)(amount * 100),
                currency = "EGP",
                delivery_needed = false,
                items = Array.Empty<object>()
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Paymob Order Error: {error}");
        }

        var data = await response.Content.ReadFromJsonAsync<Application.DTOs.PaymobDTOs.OrderResponse>(ct);
        return data!.id.ToString();
    }

    private async Task<string> GeneratePaymentKeyAsync(
        string token,
        string paymobOrderId,
        decimal amount,
        BillingData billing,
        CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("PaymobClient");

        var request = new HttpRequestMessage(HttpMethod.Post, "acceptance/payment_keys")
        {
            Content = JsonContent.Create(new
            {
                auth_token = token,
                amount_cents = (int)(amount * 100),
                expiration = 3600,
                order_id = paymobOrderId,
                billing_data = billing,
                currency = "EGP",
                integration_id = _options.IntegrationId,
                // add these
                notification_url =
                _options.WebhookUrl,

                redirection_url =
                _options.RedirectUrl
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Paymob Payment Key Error: {error}");
        }

        var data = await response.Content.ReadFromJsonAsync<PaymentKeyResponse>(ct);
        return data!.token;
    }
}
