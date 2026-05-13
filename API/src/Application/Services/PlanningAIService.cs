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
    public class PlanningAIService(LlamaService llamaService, IEventRepository eventRepository) : IPlanningAIService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public async Task<Result<BudgetAllocationResponse>> GetBudgetAllocationAsync(decimal totalBudget, string eventTypeName)
        {
            var prompt = $@"As an expert event planner, provide a budget allocation for a {eventTypeName} with a total budget of {totalBudget}.
Break it down into logical categories like Venue, Catering, Decor, Photography, Entertainment, and Miscellaneous.

Return ONLY a JSON object with this structure:
{{
    ""TotalBudget"": {totalBudget},
    ""EventType"": ""{eventTypeName}"",
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

            var bookedItems = string.Join(", ", ev.EventItems.Select(i => $"{i.ServiceName} ({i.ItemStatus})"));
            var eventType = ev.EventType?.Name ?? "General Event";

            var prompt = $@"
                Create a minute-by-minute timeline for a {eventType} titled '{ev.Title}'.
                The event date is {ev.EventDate:MMMM dd, yyyy}.
                Booked services/items: {bookedItems}.
                Location: {ev.Location?.City}, {ev.Location?.State}.
                
                Provide a structured schedule starting from setup to wrap-up.
                
                Return ONLY a JSON object with this structure:
                {{
                    ""EventId"": ""{eventId}"",
                    ""EventTitle"": ""{ev.Title}"",
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
    }
}
