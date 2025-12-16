using BLL.DTOs.PaymobDTOs;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class PaymobService 
    {
        private readonly HttpClient _http;
        private readonly PaymobOptions _options;

        public PaymobService(HttpClient http, IOptions<PaymobOptions> options)
        {
            _http = http;
            _options = options.Value;
        }

        // -------------------------------
        // PUBLIC ENTRY POINT
        // -------------------------------
        public async Task<string> CreatePaymentAsync(decimal amount, BillingData billing)
        {
            var token = await AuthenticateAsync();
            var orderId = await CreateOrderAsync(token, amount);
            var paymentKey = await GeneratePaymentKeyAsync(token, orderId, amount, billing);

            return $"{_options.BaseUrl}/acceptance/iframes/{_options.IframeId}?payment_token={paymentKey}";
        }

        // -------------------------------
        // AUTH
        // -------------------------------
        private async Task<string> AuthenticateAsync()
        {
            var response = await _http.PostAsJsonAsync(
                $"{_options.BaseUrl}/auth/tokens",
                new { api_key = _options.ApiKey });

            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return data.token;
        }

        // -------------------------------
        // CREATE ORDER
        // -------------------------------
        private async Task<string> CreateOrderAsync(string token, decimal amount)
        {
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await _http.PostAsJsonAsync(
                $"{_options.BaseUrl}/ecommerce/orders",
                new
                {
                    amount_cents = (int)(amount * 100),
                    currency = "EGP",
                    delivery_needed = false,
                    items = Array.Empty<object>()
                });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Paymob Order Error: {error}");
            }

            var data = await response.Content.ReadFromJsonAsync<OrderResponse>();
            return data.id.ToString();
        }


        // -------------------------------
        // PAYMENT KEY
        // -------------------------------
        private async Task<string> GeneratePaymentKeyAsync(
            string token,
            string orderId,
            decimal amount,
            BillingData billing)
        {
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await _http.PostAsJsonAsync(
                $"{_options.BaseUrl}/acceptance/payment_keys",
                new
                {
                    amount_cents = (int)(amount * 100),
                    expiration = 3600,
                    order_id = orderId,
                    billing_data = billing,
                    currency = "EGP",
                    integration_id = _options.IntegrationId
                });

            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<PaymentKeyResponse>();
            return data.token;
        }

        // -------------------------------
        // WEBHOOK HANDLING
        // -------------------------------
        public async Task HandleWebhookAsync(PaymobWebhookPayload payload)
        {
            bool success = payload.Success;
            long orderId = payload.Order.Id;

            if (success)
            {
                // Update DB: Status = Paid
                Console.WriteLine("Success");
            }
            else
            {
                // Update DB: Status = Failed
            }

            await Task.CompletedTask;
        }
    }

}
