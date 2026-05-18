using Shared;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPlanningAIService
    {
        Task<Result<BudgetAllocationResponse>> GetBudgetAllocationAsync(decimal totalBudget, string eventTypeName);
        Task<Result<EventTimelineResponse>> GenerateEventTimelineAsync(Guid eventId);
        Task<Result<RecommendationResponse>> GetClientsLikeYouRecommendationsAsync(Guid eventId, Guid userId);
    }

    public class BudgetAllocationResponse
    {
        public decimal TotalBudget { get; set; }
        public string EventType { get; set; }
        public List<BudgetCategory> Categories { get; set; }
        public string Advice { get; set; }
    }

    public class BudgetCategory
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
        public string Description { get; set; }
    }

    public class EventTimelineResponse
    {
        public Guid EventId { get; set; }
        public string EventTitle { get; set; }
        public List<TimelineItem> Timeline { get; set; }
        public string PlanningNotes { get; set; }
    }

    public class TimelineItem
    {
        public string Time { get; set; }
        public string Activity { get; set; }
        public string Duration { get; set; }
        public string Importance { get; set; } // Low, Medium, High
    }

    public class RecommendationItem
    {
        [JsonPropertyName("ServiceId")]
        public Guid ServiceId { get; set; }       // ✅ was VendorId — now actual ServiceId

        [JsonPropertyName("ServiceName")]
        public string ServiceName { get; set; }   // ✅ added — consumer knows what to show

        [JsonPropertyName("VendorName")]
        public string VendorName { get; set; }    // ✅ added — consumer knows who provides it

        [JsonPropertyName("Reasoning")]
        public string Reasoning { get; set; }
    }

    public class RecommendationResponse
    {
        [JsonPropertyName("Recommendations")]
        public List<RecommendationItem> Recommendations { get; set; } = new();
    }
}
