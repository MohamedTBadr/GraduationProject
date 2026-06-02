using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Application.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;
using System;
using System.Linq;
using System.Threading.Tasks;
using IdempotentAPI.Filters;
using Domain.Enums;
using Web.Api.Attributes;
using Web.Api.Controllers.Attributes;

namespace Web.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EventController(
        IServiceManager serviceManager
        ) : APIController
    {
        protected Guid UserId => GetUserIdFromToken();

        // ─────────────────────────────────────────────────────────
        // GET api/events
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HybridCache(1800, "events", Variance =CacheVariance.Adaptive)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = IsAdmin()
                ? await serviceManager.EventService.GetAllAsync(cancellationToken)
                : await serviceManager.EventService.GetByUserIdAsync(UserId, cancellationToken);

            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        // ─────────────────────────────────────────────────────────
        // GET api/events/{id}
        // ─────────────────────────────────────────────────────────
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HybridCache(1800, "events", "events/{id}",Variance =CacheVariance.Adaptive)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await serviceManager.EventService.GetByIdAsync(id, cancellationToken);
            if (result.IsFailure) return result.ToActionResult();

            if (!await HasAccess(id, result.Value.UserId))
                return Forbid();

            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────
        // GET api/events/user/{userId}
        // ─────────────────────────────────────────────────────────
        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HybridCache(1800, "events", "events/user/{userId}", Variance = CacheVariance.Adaptive)]
        public async Task<IActionResult> GetByUser(Guid userId, CancellationToken cancellationToken)
        {
            if (!IsAdminOrOwner(userId))
                return Forbid();

            var result = await serviceManager.EventService.GetByUserIdAsync(userId, cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        // ─────────────────────────────────────────────────────────
        // GET api/events/status/{status}
        // ─────────────────────────────────────────────────────────
        [HttpGet("status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HybridCache(1800, "events", "events/status/{status}", Variance = CacheVariance.Adaptive)]
        public async Task<IActionResult> GetByStatus(string status, CancellationToken cancellationToken)
        {
            if (IsAdmin())
            {
                var all = await serviceManager.EventService.GetByStatusAsync(status, cancellationToken);
                return Ok(all);
            }

            var result = await serviceManager.EventService.GetByUserIdAndStatusAsync(UserId, status, cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        // ─────────────────────────────────────────────────────────
        // POST api/events/createEventByAI/{eventId}
        // ─────────────────────────────────────────────────────────
        [HttpPost("createEventByAI/{eventId}")]
        [Idempotent]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateEventItemsByAI(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            var eventResult = await serviceManager.EventService.GetByIdAsync(eventId, cancellationToken);
            if (eventResult.IsFailure)
                return eventResult.ToActionResult();

            if (!await HasAccess(eventId, eventResult.Value.UserId, requiresEdit: true))
                return Forbid();

            var eventObject = eventResult.Value;

            var request = new AIRequest
            {
                Budget = eventObject.TotalBudget,
                GuestCount = eventObject.GuestCount,
                EventTypeName = eventObject.EventTypeName
            };

            var servicesResult = await serviceManager.ServiceService.AIFilterAsync(request, cancellationToken);
            if (servicesResult.IsFailure)
                return servicesResult.ToActionResult();

            var serviceLines = servicesResult.Value
                .Select(s => $"- ServiceId: {s.Id} | Name: {s.Name} | Vendor: {s.VendorName} | Type: {s.ServiceTypeName} | Price: {s.Price:C}")
                .ToList();

            var prompt = $$"""
You are an event planning API that returns ONLY valid JSON.
Do not include markdown, explanation, or extra text.

Plan a full event using the details and available services below.

EVENT DETAILS:
- Title: {{eventObject.Title}}
- Event Type: {{eventObject.EventTypeName}}
- Date: {{eventObject.EventDate:yyyy-MM-dd}}
- Location: {{eventObject.Location?.City}}, {{eventObject.Location?.State}}
- Guest Count: {{eventObject.GuestCount}}
- Total Budget: {{eventObject.TotalBudget:C}}
- Notes: {{eventObject.Notes ?? "None"}}

AVAILABLE SERVICES (within budget):
{{string.Join("\n", serviceLines)}}

Return a JSON object with this exact schema:
{
  "event_title": string,
  "event_type": string,
  "guest_count": number,
  "total_budget": number,
  "plan_summary": string,
  "selected_items": [
    {
      "ServiceId": "Guid",
      "Service_name": string,
      "category": string,
      "vendor": string,
      "price": number,
      "reason": string
    }
  ],
  "total_cost": number,
  "remaining_budget": number,
  "tips": [string]
}

Only return JSON. No markdown. No explanation.
""";

            var aiResult = await serviceManager.LlamaService.SendMessageAsync(
                prompt,
                systemPrompt: "You are an event planning engine. Respond with JSON only."
            );

            if (aiResult.IsFailure)
                return aiResult.ToActionResult();

            return Result<object>.Success(new
            {
                eventId = eventObject.Id,
                eventTitle = eventObject.Title,
                budget = eventObject.TotalBudget,
                eventTypeName = eventObject.EventTypeName,
                servicesConsidered = serviceLines.Count,
                aiPlan = aiResult.Value
            }).ToActionResult();
        }

        // ─────────────────────────────────────────────────────────
        // POST api/events
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        [Idempotent]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [InvalidateCache("events", "dashboard-stats")]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (IsVendor())
                return Forbid();

            if (IsClient())
                dto.UserId = UserId;

            var created = await serviceManager.EventService.CreateAsync(dto, cancellationToken);
            return created.IsSuccess
                ? Ok(created)
                : created.ToActionResult();
        }

        // ─────────────────────────────────────────────────────────
        // PUT api/events/{id}
        // ─────────────────────────────────────────────────────────
        [HttpPut("{id:guid}")]
        [Idempotent]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [InvalidateCache("events", "events/{id}", "dashboard-stats")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await serviceManager.EventService.GetByIdAsync(id, cancellationToken);
            if (existing.IsFailure) return existing.ToActionResult();

            if (!await HasAccess(id, existing.Value.UserId, requiresEdit: true))
                return Forbid();

            if ((IsVendor() || IsClient()) && dto.EventStatus == "Completed")
                return Forbid();

            var updated = await serviceManager.EventService.UpdateAsync(id, dto, cancellationToken);
            
            return updated.IsSuccess ? Ok(updated) : updated.ToActionResult();
        }

        // ─────────────────────────────────────────────────────────
        // POST api/events/{eventId}/items
        // ─────────────────────────────────────────────────────────
        [HttpPost("{eventId:guid}/items")]
        [Idempotent]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [InvalidateCache("events", "events/{eventId}", "dashboard-stats")]
        public async Task<IActionResult> AddItem(
            Guid eventId,
            [FromBody] CreateEventItemDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (IsVendor())
                return Forbid();

            var existing = await serviceManager.EventService.GetByIdAsync(eventId, cancellationToken);
            if (existing.IsFailure) return existing.ToActionResult();

            if (!await HasAccess(eventId, existing.Value.UserId, requiresEdit: true))
                return Forbid();

            var result = await serviceManager.EventService.AddItemAsync(eventId, dto, cancellationToken);
            return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = eventId }, result) : result.ToActionResult();
        }

        // ─────────────────────────────────────────────────────────
        // PUT api/events/{eventId}/items/{itemId}
        // ─────────────────────────────────────────────────────────
        [HttpPut("{eventId:guid}/items/{itemId:guid}")]
        [Idempotent]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [InvalidateCache("events", "events/{eventId}", "dashboard-stats")]
        public async Task<IActionResult> UpdateItem(
            Guid eventId,
            Guid itemId,
            [FromBody] UpdateEventItemDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (IsVendor())
                return Forbid();

            var existing = await serviceManager.EventService.GetByIdAsync(eventId, cancellationToken);
            if (existing.IsFailure) return existing.ToActionResult();

            if (!await HasAccess(eventId, existing.Value.UserId, requiresEdit: true))
                return Forbid();

            var result = await serviceManager.EventService.UpdateItemAsync(eventId, itemId, dto, cancellationToken);
            return result.IsSuccess ? Ok(result) : result.ToActionResult();
        }

        // ─────────────────────────────────────────────────────────
        // PATCH api/events/{eventId}/items/{itemId}/approve
        // ─────────────────────────────────────────────────────────
        [HttpPatch("{eventId:guid}/items/{itemId:guid}/approve")]
        [Idempotent]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [InvalidateCache("events", "events/{eventId}", "vendors/{UserId}/bookings", "dashboard-stats")]
        public async Task<IActionResult> ApproveItem(
            Guid eventId,
            Guid itemId,
            [FromBody] ApproveItemRequest request,
            CancellationToken cancellationToken)
        {
            if (!IsVendor())
                return Forbid();

            var result = await serviceManager.EventService.ApproveItemAsync(eventId, itemId, UserId, request.Approve, request.Reason, cancellationToken);
            var eventDto = await serviceManager.EventService.GetByIdAsync(eventId, cancellationToken);
            if (result.IsSuccess)
            {
                await serviceManager.NotificationService.SendAsync(eventDto.Value.UserId, "ITEM_APPROVAL_UPDATE", "Item Approval Update", $"Your event item '{result.Value.ServiceName}' has been approved");
            }
            return result.IsSuccess ? NoContent() : result.ToActionResult();
        }

        // ─────────────────────────────────────────────────────────
        // PATCH api/events/{id}/cancel
        // ─────────────────────────────────────────────────────────
        [HttpPatch("{id:guid}/cancel")]
        [Idempotent]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [InvalidateCache("events", "dashboard-stats")]
        public async Task<IActionResult> CancelEvent(Guid id, [FromBody] CancelEventRequest cancelEventRequest, CancellationToken cancellationToken)
        {
            var existing = await serviceManager.EventService.GetByIdAsync(id, cancellationToken);
            if (existing.IsFailure) return existing.ToActionResult();

            if (!await HasAccess(id, existing.Value.UserId, requiresEdit: true))
                return Forbid();

            if (IsVendor())
                return Forbid();

            if (IsClient() && existing.Value.EventStatus != "Approved")
                return Forbid();

            if (IsClient() && existing.Value.EventDate.Date <= DateTime.Today.AddDays(7))
                return BadRequest(new { message = "You cannot cancel an event less than 7 days before it occurs." });

            var result = await serviceManager.EventService.CancelEventAsync(id, cancelEventRequest, cancellationToken);
            if (result.IsSuccess)
            {
                await serviceManager.NotificationService.SendBulkAsync(
                    existing.Value.EventItems.Select(item =>
                    (
                        UserId: item.VendorId,
                        Type: "EVENT_CANCELLED",
                        Title: "Event Cancelled",
                        Message: $"Your event '{existing.Value.Title}' has been cancelled. Reason: {cancelEventRequest.Reason}"
                    )));
            }
            return result.IsSuccess ? NoContent() : result.ToActionResult();
        }

        // ─────────────────────────────────────────────────────────
        // DELETE api/events/{id}
        // ─────────────────────────────────────────────────────────
        [HttpDelete("{id:guid}")]
        [Idempotent]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [InvalidateCache("events", "events/{id}", "dashboard-stats")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            if (IsVendor())
                return Forbid();

            var existing = await serviceManager.EventService.GetByIdAsync(id, cancellationToken);
            if (existing.IsFailure) return existing.ToActionResult();

            if (!IsAdminOrOwner(existing.Value.UserId))
                return Forbid();

            if (IsClient() && existing.Value.EventStatus != "Planned")
                return BadRequest(new { message = "You can only delete events with 'Planned' status." });

            var result = await serviceManager.EventService.DeleteAsync(id, cancellationToken);
            return result.IsSuccess ? NoContent() : result.ToActionResult();
        }

        // ─────────────────────────────────────────────────────────
        // COLLABORATION ENDPOINTS
        // ─────────────────────────────────────────────────────────

        [HttpPost("{id:guid}/collaborators")]
        [Idempotent]
        public async Task<IActionResult> AddCollaborator(Guid id, [FromBody] AddCollaboratorRequest request, CancellationToken cancellationToken)
        {
            var existing = await serviceManager.EventService.GetByIdAsync(id, cancellationToken);
            if (existing.IsFailure) return existing.ToActionResult();

            if (!IsAdminOrOwner(existing.Value.UserId))
                return Forbid();

            var result = await serviceManager.EventService.AddCollaboratorAsync(id, request.UserEmailOrName, request.Role, cancellationToken);
            return result.IsSuccess ? Ok(result) : result.ToActionResult();
        }

        [HttpGet("{id:guid}/collaborators")]
        public async Task<IActionResult> GetCollaborators(Guid id, CancellationToken cancellationToken)
        {
            var existing = await serviceManager.EventService.GetByIdAsync(id, cancellationToken);
            if (existing.IsFailure) return existing.ToActionResult();

            if (!await HasAccess(id, existing.Value.UserId))
                return Forbid();

            var result = await serviceManager.EventService.GetCollaboratorsAsync(id, cancellationToken);
            return result.IsSuccess ? Ok(result) : result.ToActionResult();
        }

        [HttpDelete("{id:guid}/collaborators/{userId:guid}")]
        [Idempotent]
        public async Task<IActionResult> RemoveCollaborator(Guid id, Guid userId, CancellationToken cancellationToken)
        {
            var existing = await serviceManager.EventService.GetByIdAsync(id, cancellationToken);
            if (existing.IsFailure) return existing.ToActionResult();

            if (!IsAdminOrOwner(existing.Value.UserId))
                return Forbid();

            var result = await serviceManager.EventService.RemoveCollaboratorAsync(id, userId, cancellationToken);
            return result.IsSuccess ? NoContent() : result.ToActionResult();
        }

        private async Task<bool> HasAccess(Guid eventId, Guid ownerId, bool requiresEdit = false)
        {
            if (IsAdmin() || UserId == ownerId) return true;
            return await serviceManager.EventService.HasPermissionAsync(eventId, UserId, requiresEdit, default);
        }
    }
}