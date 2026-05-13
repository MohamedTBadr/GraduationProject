using Application.Interfaces;
using IdempotentAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;
using System;
using System.Threading.Tasks;
using Web.Api.Attributes;
using Web.Api.Controllers.Attributes;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AIController(IPlanningAIService aiService) : BaseController
    {
        [HttpPost("budget-allocation")]
        [HybridCache(3600, "ai-budget")]
        [Idempotent]
        public async Task<IActionResult> GetBudgetAllocation([FromBody] BudgetAllocationRequest request)
        {
            var result = await aiService.GetBudgetAllocationAsync(request.TotalBudget, request.EventTypeName);
            return result.IsSuccess ? Ok(result) : result.ToActionResult();
        }

        [HttpPost("event-timeline/{eventId}")]
        [HybridCache(3600, "ai-timeline")]
        [Idempotent]
        public async Task<IActionResult> GenerateTimeline(Guid eventId)
        {
            var result = await aiService.GenerateEventTimelineAsync(eventId);
            return result.IsSuccess ? Ok(result) : result.ToActionResult();
        }
    }

    public class BudgetAllocationRequest
    {
        public decimal TotalBudget { get; set; }
        public string EventTypeName { get; set; }
    }
}
