using Application.Contracts;
using Application.DTOs.Ai;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Ai
{
    public sealed class GroqAiInsightService : IAiInsightService
    {
        private readonly ChatClient _chatClient;
        private const string Model = "llama-3.3-70b-versatile";

        public GroqAiInsightService(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<AiInsightResponseDto> GenerateInsightsAsync(
            AiInsightRequestDto request,
            CancellationToken ct = default)
        {
            var prompt = BuildPrompt(request);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage(prompt)
            };

            var response = await _chatClient.CompleteChatAsync(messages, cancellationToken: ct);

            if (response?.Value?.Content == null || response.Value.Content.Count == 0)
                throw new InvalidOperationException("Empty response from Groq");

            var rawJson = response.Value.Content[0].Text;

            return ParseInsights(rawJson, Model);
        }

        private const string SystemPrompt = @"You are a senior business intelligence analyst.
Your role is to interpret pre-calculated financial KPIs and generate
a structured executive report in JSON format.

STRICT RULES — violating any rule makes the output unusable:
1. NEVER invent, estimate, or recalculate any financial figures.
2. ONLY reference the exact numbers provided in the input data.
3. If you cannot determine something from the data, say ""Insufficient data.""
4. Output ONLY valid JSON — no markdown, no prose outside the JSON object.
5. All arrays must contain 2–4 items unless data is insufficient.
6. Tone: professional, concise, executive-level. No filler phrases.

Output format (strict):
{
  ""summary"": ""string"",
  ""risks"": [""string"", ""string""],
  ""opportunities"": [""string"", ""string""],
  ""recommendations"": [""string"", ""string""],
  ""conclusion"": ""string""
}";

        private static string BuildPrompt(AiInsightRequestDto req)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"REPORT SCOPE: {req.Scope}");
            sb.AppendLine();
            sb.AppendLine("=== KEY PERFORMANCE INDICATORS ===");
            sb.AppendLine($"Lifetime Revenue: {req.KPIs.LifetimeRevenue:F2}");
            sb.AppendLine($"Current Month Revenue: {req.KPIs.CurrentMonthRevenue:F2}");
            sb.AppendLine($"Last Month Revenue: {req.KPIs.LastMonthRevenue:F2}");
            sb.AppendLine($"Month-over-Month Growth: {req.KPIs.GrowthPercentage}%");

            if (req.KPIs.TotalOrders.HasValue)
            {
                sb.AppendLine($"Total Orders: {req.KPIs.TotalOrders}");
                sb.AppendLine($"Average Order Value: {req.KPIs.AverageOrderValue:F2}");
                sb.AppendLine($"Average Monthly Revenue: {req.KPIs.AverageMonthlyRevenue:F2}");
            }

            sb.AppendLine();
            sb.AppendLine("=== REVENUE TREND (last 12 months) ===");

            foreach (var h in req.RevenueHistory)
            {
                var growth = h.GrowthPercentage.HasValue
                    ? $"{h.GrowthPercentage:+0.00;-0.00}%"
                    : "baseline";

                sb.AppendLine($"  {h.Label}: {h.Revenue:F2} | {h.Orders} orders | growth: {growth}");
            }

            sb.AppendLine();
            sb.AppendLine("=== TOP SERVICES BY REVENUE ===");

            foreach (var s in req.TopServices)
                sb.AppendLine(
                    $"  {s.ServiceName}: {s.Revenue:F2} ({s.RevenueShare}% of total) | {s.Orders} orders");

            if (req.AdminMetrics is not null)
            {
                sb.AppendLine();
                sb.AppendLine("=== PLATFORM METRICS (Admin) ===");
                sb.AppendLine($"Total Vendors: {req.AdminMetrics.TotalVendors}");
                sb.AppendLine($"Verified Vendors: {req.AdminMetrics.VerifiedVendors} ({req.AdminMetrics.VendorVerificationRate}%)");
                sb.AppendLine($"Total Customers: {req.AdminMetrics.TotalCustomers}");
                sb.AppendLine($"Total Orders: {req.AdminMetrics.TotalOrders}");
            }

            sb.AppendLine();
            sb.AppendLine("Analyze the above data and return your JSON response now.");

            return sb.ToString();
        }

        private static AiInsightResponseDto ParseInsights(string rawJson, string model)
        {
            var clean = rawJson
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var parsed = JsonSerializer.Deserialize<AiInsightOutput>(clean,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Failed to parse AI response JSON");

            return new AiInsightResponseDto
            {
                Summary = parsed.Summary,
                Risks = parsed.Risks,
                Opportunities = parsed.Opportunities,
                Recommendations = parsed.Recommendations,
                Conclusion = parsed.Conclusion,
                ModelUsed = model,
                GeneratedAt = DateTime.UtcNow
            };
        }

        private sealed record AiInsightOutput(
            string Summary,
            List<string> Risks,
            List<string> Opportunities,
            List<string> Recommendations,
            string Conclusion);
    }
}
