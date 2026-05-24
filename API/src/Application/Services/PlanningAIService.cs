using Application.Interfaces;
using Application.Services.Helpers;
using Domain.Contracts;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PlanningAIService(LlamaService llamaService, IEventRepository eventRepository, IOrderRepository orderRepository, ISearchService searchService) : IPlanningAIService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public async Task<Result<BudgetAllocationResponse>> GetBudgetAllocationAsync(decimal totalBudget, string eventTypeName)
        {
            var safeEventTypeName = SanitizeForPrompt(eventTypeName);
            var prompt = $@"As an expert event planner, provide a budget allocation for a {safeEventTypeName} with a total budget of {totalBudget}.
Break it down into logical categories like Venue, Catering, Decor, Photography, Entertainment, and Miscellaneous.

Return ONLY a JSON object with this structure:
{{
    ""TotalBudget"": {totalBudget},
    ""EventType"": ""{safeEventTypeName}"",
    ""Categories"": [
        {{ ""Name"": ""Category Name"", ""Amount"": 0, ""Percentage"": 0, ""Description"": ""Brief explanation"" }}
    ],
    ""Advice"": ""A short pro-tip for this budget level.""
}}";

            var result = await llamaService.SendMessageAsync(prompt, "You are a professional event financial advisor. Return JSON only.");

            if (result.IsFailure) return Result<BudgetAllocationResponse>.Failure(result.Error);

            try
            {
                var allocation = JsonSerializer.Deserialize<BudgetAllocationResponse>(result.Value, JsonOptions);
                return Result<BudgetAllocationResponse>.Success(allocation!);
            }
            catch (Exception ex)
            {
                return Result<BudgetAllocationResponse>.Unexpected(5002, $"Failed to parse AI response: {ex.Message}");
            }
        }

        public async Task<Result<EventTimelineResponse>> GenerateEventTimelineAsync(Guid eventId)
        {
            var ev = await eventRepository.GetByIdWithItemsAsync(eventId, default);
            if (ev == null) return Result<EventTimelineResponse>.NotFound(404, "Event not found.");

            var bookedItems = string.Join(", ", ev.EventItems.Select(i => $"{SanitizeForPrompt(i.Service.Name)} ({SanitizeForPrompt(i.ItemStatus)})"));
            var eventType = SanitizeForPrompt(ev.EventType?.Name ?? "General Event");
            var eventTitle = SanitizeForPrompt(ev.Title);
            var locationCity = SanitizeForPrompt(ev.Location?.City ?? string.Empty);
            var locationState = SanitizeForPrompt(ev.Location?.State ?? string.Empty);

            var prompt = $@"
                Create a minute-by-minute timeline for a {eventType} titled '{eventTitle}'.
                The event date is {ev.EventDate:MMMM dd, yyyy}.
                Booked services/items: {bookedItems}.
                Location: {locationCity}, {locationState}.
                
                Provide a structured schedule starting from setup to wrap-up.
                
                Return ONLY a JSON object with this structure:
                {{
                    ""EventId"": ""{eventId}"",
                    ""EventTitle"": ""{eventTitle}"",
                    ""Timeline"": [
                        {{ ""Time"": ""HH:MM AM/PM"", ""Activity"": ""Description"", ""Duration"": ""X mins/hours"", ""Importance"": ""High/Medium/Low"" }}
                    ],
                    ""PlanningNotes"": ""Important logistical advice for this specific timeline.""
                }}
                ";

            var result = await llamaService.SendMessageAsync(prompt, "You are a professional event coordinator. Return JSON only.");

            if (result.IsFailure) return Result<EventTimelineResponse>.Failure(result.Error);

            try
            {
                var timeline = JsonSerializer.Deserialize<EventTimelineResponse>(result.Value, JsonOptions);
                return Result<EventTimelineResponse>.Success(timeline!);
            }
            catch (Exception ex)
            {
                return Result<EventTimelineResponse>.Unexpected(5002, $"Failed to parse AI response: {ex.Message}");
            }
        }

        public async Task<Result<RecommendationResponse>> GetClientsLikeYouRecommendationsAsync(Guid eventId, Guid userId)
        {
            var currentEvent = await eventRepository.GetByIdWithItemsAsync(eventId, default);
            if (currentEvent == null) return Result<RecommendationResponse>.NotFound(404, "Event not found.");

            var userOrders = await orderRepository.GetByUserIdAsync(userId, default);

            var allBookedItems = userOrders
                .SelectMany(o => o.Event?.EventItems ?? new List<EventItem>())
                .ToList();

            var bookedVendorIdsStr = string.Join(" ", allBookedItems
                .Select(i => i.Service.VendorId.ToString())
                .Distinct());

            var bookedCategoriesStr = string.Join(" ", allBookedItems
                .Select(i => i.Service.Name?.Replace(" ", ""))
                .Distinct());

            string prompt;

            if (string.IsNullOrWhiteSpace(bookedVendorIdsStr) && string.IsNullOrWhiteSpace(bookedCategoriesStr))
            {
                // ── Cold Start: No booking history ────────────────────────────────────
                prompt = $@"
            The user is planning a {SanitizeForPrompt(currentEvent.EventType?.Name ?? "Event")} with a total budget of {currentEvent.TotalBudget}.
            They have no prior booking history on our platform.
            Based on standard industry practices for this type of event, recommend 3 essential service categories they should consider booking.

            Return ONLY a valid JSON object with this exact structure, no extra text:
            {{
                ""Recommendations"": [
                    {{
                        ""ServiceId"":   ""00000000-0000-0000-0000-000000000000"",
                        ""ServiceName"": ""<name of the service category>"",
                        ""VendorName"":  ""General Recommendation"",
                        ""Reasoning"":   ""Why this service is essential for this event type.""
                    }}
                ]
            }}
        ";
            }
            else
            {
                // ── Collaborative Filtering ───────────────────────────────────────────
                var similarUserIds = (await searchService.SearchSimilarUsersAsync(bookedVendorIdsStr, bookedCategoriesStr, 10))
                    .Where(sUserId => sUserId != userId)
                    .Distinct()
                    .ToList();

                var candidateServices = new List<string>();

                if (similarUserIds.Any())
                {
                    var similarUserOrders = await orderRepository.GetByUserIdsAsync(similarUserIds, default);
                    var similarItems = similarUserOrders
                        .SelectMany(o => o.Event?.EventItems ?? new List<EventItem>())
                        .ToList();

                    foreach (var item in similarItems)
                    {
                        var alreadyBooked = allBookedItems.Any(b =>
                            b.Service.Id == item.Service.Id);

                        if (!alreadyBooked)
                        {
                            candidateServices.Add(
                                $"{{ " +
                                $"\"ServiceId\": \"{item.Service.Id}\", " +
                                $"\"ServiceName\": \"{SanitizeForPrompt(item.Service.Name)}\", " +
                                $"\"VendorName\": \"{SanitizeForPrompt(item.Service.Vendor.BusinessName)}\", " +
                                $"\"Price\": {item.Service.Price} " +
                                $"}}"
                            );
                        }
                    }
                }

                candidateServices = candidateServices.Distinct().Take(15).ToList();
                var candidateStr = candidateServices.Any()
                    ? string.Join(", ", candidateServices)
                    : "None available";

                prompt = $@"
            The user is planning a {SanitizeForPrompt(currentEvent.EventType?.Name ?? "Event")} with a budget of {currentEvent.TotalBudget}.
            They have already booked these services: {string.Join(", ", allBookedItems.Select(i => SanitizeForPrompt(i.Service.Name)).Distinct())}.

            Based on collaborative filtering, users who planned similar events also booked these candidate services:
            [{candidateStr}]

            Select the top 3 best matching services from the candidates that this user should book next.
            - If candidates are available, use their exact ServiceId, ServiceName, and VendorName from the list above.
            - If candidates list is 'None available', suggest 3 general essential services with ServiceId as ""00000000-0000-0000-0000-000000000000"".
            - Reasoning must start with: 'People planning similar events also booked...'

            Return ONLY a valid JSON object with this exact structure, no extra text:
            {{
                ""Recommendations"": [
                    {{
                        ""ServiceId"":   ""<actual ServiceId or 00000000-0000-0000-0000-000000000000>"",
                        ""ServiceName"": ""<service name>"",
                        ""VendorName"":  ""<vendor business name>"",
                        ""Reasoning"":   ""<reasoning string>""
                    }}
                ]
            }}
        ";
            }

            var result = await llamaService.SendMessageAsync(
                prompt,
                "You are a personalized event recommendation engine. Return valid JSON only, no markdown, no explanation.");

            if (result.IsFailure) return Result<RecommendationResponse>.Failure(result.Error);

            try
            {
                // Strip markdown fences in case Llama wraps output in ```json ... ```
                var raw = result.Value
                    .Trim()
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                var responseObj = JsonSerializer.Deserialize<RecommendationResponse>(raw, JsonOptions);
                return Result<RecommendationResponse>.Success(responseObj!);
            }
            catch (Exception ex)
            {
                return Result<RecommendationResponse>.Unexpected(5002, $"Failed to parse AI response: {ex.Message}");
            }
        }

        private static string SanitizeForPrompt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("```", string.Empty)
                .Trim();
        }
    }
}
