using Application.DTOs.Orders;
using Application.DTOs.PaymobDTOs;
using Application.Interfaces.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Infrastructure.Payments; // Moved to Infrastructure (Clean Arch)

public class PaymobService(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymobOptions> options,
    IOrderService orderService
    ) 
{
    private readonly PaymobOptions _options = options.Value;

    public async Task<string> CreatePaymentAsync(decimal amount, BillingData billing, CancellationToken ct )
    {
        // 1. Authenticate
        var authToken = await AuthenticateAsync(ct);

        // 2. Create Order
        var orderId = await CreateOrderAsync(authToken, amount, ct);

        // 3. Generate Payment Key
        var paymentKey = await GeneratePaymentKeyAsync(authToken, orderId, amount, billing, ct);

        return $"{_options.BaseUrl}/acceptance/iframes/{_options.IframeId}?payment_token={paymentKey}";
    }

    private async Task<string> AuthenticateAsync(CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("PaymobClient");

        var response = await client.PostAsJsonAsync("auth/tokens", new { api_key = _options.ApiKey }, ct);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<AuthResponse>(ct);
        return data!.token;
    }

    private async Task<string> CreateOrderAsync(string token, decimal amount, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("PaymobClient");

        // Use HttpRequestMessage to avoid shared header pollution
        var request = new HttpRequestMessage(HttpMethod.Post, "ecommerce/orders")
        {
            Content = JsonContent.Create(new
            {
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

    private async Task<string> GeneratePaymentKeyAsync(string token, string orderId, decimal amount, BillingData billing, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("PaymobClient");

        var request = new HttpRequestMessage(HttpMethod.Post, "acceptance/payment_keys")
        {
            Content = JsonContent.Create(new
            {
                auth_token = token, // Paymob accepts token in body or header for this endpoint
                amount_cents = (int)(amount * 100),
                expiration = 3600,
                order_id = orderId,
                billing_data = billing,
                currency = "EGP",
                integration_id = _options.IntegrationId
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<PaymentKeyResponse>(ct);
        return data!.token;
    }

    public async Task HandleWebhookAsync(PaymobWebhookPayload payload, CancellationToken ct)
    {
        // In Clean Arch, this would likely trigger a Domain Event or MediatR Command
        if (payload.Success)
        {

            // Logic to fulfill order
            var status = new UpdateOrderStatusRequest("Paid");
                await orderService.UpdatePaymentStatusAsync(payload.Order, status, ct);

        }
        await Task.CompletedTask;
    }
}