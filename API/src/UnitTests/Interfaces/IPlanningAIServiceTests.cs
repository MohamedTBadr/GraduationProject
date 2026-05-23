using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application;
using Application.Interfaces;
using Moq;
using Xunit;

namespace Application.UnitTests.Interfaces
{
    public class IPlanningAIServiceTests
    {
        private readonly Mock<IPlanningAIService> _mockService;

        public IPlanningAIServiceTests()
        {
            _mockService = new Mock<IPlanningAIService>();
        }

        [Fact]
        public async Task GetBudgetAllocationAsync_Success_ReturnsBudgetAllocationResponse()
        {
            // Arrange
            decimal totalBudget = 5000m;
            string eventType = "Wedding";
            var response = new BudgetAllocationResponse
            {
                TotalBudget = totalBudget,
                EventType = eventType,
                Categories = new List<BudgetCategory>
                {
                    new BudgetCategory { Name = "Catering", Amount = 2000m, Percentage = 40, Description = "Food and Drinks" }
                },
                Advice = "Book early!"
            };
            var result = Result<BudgetAllocationResponse>.Success(response);

            _mockService.Setup(s => s.GetBudgetAllocationAsync(totalBudget, eventType))
                .ReturnsAsync(result);

            // Act
            var actual = await _mockService.Object.GetBudgetAllocationAsync(totalBudget, eventType);

            // Assert
            Assert.True(actual.IsSuccess);
            Assert.Equal(response, actual.Value);
        }

        [Fact]
        public async Task GetBudgetAllocationAsync_Failure_ReturnsErrorResult()
        {
            // Arrange
            decimal totalBudget = -500m;
            string eventType = "";
            var result = Result<BudgetAllocationResponse>.Validation(123, "Invalid budget");

            _mockService.Setup(s => s.GetBudgetAllocationAsync(totalBudget, eventType))
                .ReturnsAsync(result);

            // Act
            var actual = await _mockService.Object.GetBudgetAllocationAsync(totalBudget, eventType);

            // Assert
            Assert.False(actual.IsSuccess);
            Assert.NotNull(actual.Error);
            Assert.Equal(123, actual.Error.Code);
        }

        [Fact]
        public async Task GenerateEventTimelineAsync_Success_ReturnsEventTimelineResponse()
        {
            // Arrange
            Guid eventId = Guid.NewGuid();
            var response = new EventTimelineResponse
            {
                EventId = eventId,
                EventTitle = "My Event",
                Timeline = new List<TimelineItem>
                {
                    new TimelineItem { Time = "10:00", Activity = "Start", Duration = "1h", Importance = "High" }
                },
                PlanningNotes = "Notes"
            };
            var result = Result<EventTimelineResponse>.Success(response);

            _mockService.Setup(s => s.GenerateEventTimelineAsync(eventId))
                .ReturnsAsync(result);

            // Act
            var actual = await _mockService.Object.GenerateEventTimelineAsync(eventId);

            // Assert
            Assert.True(actual.IsSuccess);
            Assert.Equal(response, actual.Value);
        }

        [Fact]
        public async Task GenerateEventTimelineAsync_Failure_ReturnsNotFound()
        {
            // Arrange
            Guid eventId = Guid.NewGuid();
            var result = Result<EventTimelineResponse>.NotFound(404, "Event not found");

            _mockService.Setup(s => s.GenerateEventTimelineAsync(eventId))
                .ReturnsAsync(result);

            // Act
            var actual = await _mockService.Object.GenerateEventTimelineAsync(eventId);

            // Assert
            Assert.False(actual.IsSuccess);
            Assert.NotNull(actual.Error);
            Assert.Equal(404, actual.Error.Code);
        }

        [Fact]
        public async Task GetClientsLikeYouRecommendationsAsync_Success_ReturnsRecommendationResponse()
        {
            // Arrange
            Guid eventId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var response = new RecommendationResponse
            {
                Recommendations = new List<RecommendationItem>
                {
                    new RecommendationItem { ServiceId = Guid.NewGuid(), ServiceName = "DJ", VendorName = "Music Inc", Reasoning = "Popular" }
                }
            };
            var result = Result<RecommendationResponse>.Success(response);

            _mockService.Setup(s => s.GetClientsLikeYouRecommendationsAsync(eventId, userId))
                .ReturnsAsync(result);

            // Act
            var actual = await _mockService.Object.GetClientsLikeYouRecommendationsAsync(eventId, userId);

            // Assert
            Assert.True(actual.IsSuccess);
            Assert.Equal(response, actual.Value);
        }

        [Fact]
        public async Task GetClientsLikeYouRecommendationsAsync_FailureCallback_ReturnsError()
        {
            // Arrange
            Guid eventId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var result = Result<RecommendationResponse>.BusinessRule(500, "Error finding clients");

            _mockService.Setup(s => s.GetClientsLikeYouRecommendationsAsync(eventId, userId))
                .ReturnsAsync(result);

            // Act
            var actual = await _mockService.Object.GetClientsLikeYouRecommendationsAsync(eventId, userId);

            // Assert
            Assert.False(actual.IsSuccess);
            Assert.NotNull(actual.Error);
            Assert.Equal(500, actual.Error.Code);
            Assert.Equal("Error finding clients", actual.Error.Description);
        }
    }
}