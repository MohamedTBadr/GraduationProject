using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Application.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;
using System;
using System.Threading.Tasks;

namespace Web.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EventController(
        IEventService _eventService,
        LlamaService _llamaService,
        IServiceService _serviceService) : BaseController
    {
        protected Guid UserId => GetUserIdFromToken();

        // ─────────────────────────────────────────────────────────
        // GET api/events
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = IsAdmin()
                ? await _eventService.GetAllAsync( cancellationToken)
                : await _eventService.GetByUserIdAsync(UserId, cancellationToken); // ← was calling GetUserIdFromToken() directly, use property instead

            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        // ─────────────────────────────────────────────────────────
        // GET api/events/{id}
        // ─────────────────────────────────────────────────────────
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
           
                var result = await _eventService.GetByIdAsync(id, cancellationToken);
                if (!IsAdminOrOwner(result.Value.UserId))
                    return Forbid();

                return result.IsSuccess ? Ok(result) : NotFound(result);
            
        }

        // ─────────────────────────────────────────────────────────
        // GET api/events/user/{userId}
        // ─────────────────────────────────────────────────────────
        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByUser(Guid userId, CancellationToken cancellationToken)
        {
            if (!IsAdminOrOwner(userId))
                return Forbid();

            var result = await _eventService.GetByUserIdAsync(userId, cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        // ─────────────────────────────────────────────────────────
        // GET api/events/status/{status}
        // ─────────────────────────────────────────────────────────
        [HttpGet("status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByStatus(string status, CancellationToken cancellationToken)
        {
          
                if (IsAdmin())
                {
                    var all = await _eventService.GetByStatusAsync(status, cancellationToken);
                    return Ok(all);
                }

                var result = await _eventService.GetByUserIdAndStatusAsync(UserId, status, cancellationToken);
                return result.IsSuccess ? Ok(result) : NotFound(result);
            
           
        }

        // ─────────────────────────────────────────────────────────
        // POST api/events/createEventByAI/{eventId}
        // ─────────────────────────────────────────────────────────
        [HttpPost("createEventByAI/{eventId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateEventItemsByAI(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            // 1. Get event
            var eventResult = await _eventService.GetByIdAsync(eventId, cancellationToken);
            if (eventResult.IsFailure)
                return eventResult.ToActionResult();

            var eventObject = eventResult.Value;

            // 2. Filter services by budget/type
            var request = new AIRequest
            {
                Budget = eventObject.TotalBudget,
                GuestCount = eventObject.GuestCount,
                EventTypeName = eventObject.EventTypeName
            };

            var servicesResult = await _serviceService.AIFilterAsync(request, cancellationToken);
            if (servicesResult.IsFailure)
                return servicesResult.ToActionResult();

            var serviceLines = servicesResult.Value;

            // 3. Build prompt
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

            // 4. Call Llama
            var aiResult = await _llamaService.SendMessageAsync(
                prompt,
                systemPrompt: "You are an event planning engine. Respond with JSON only."
            );

            if (aiResult.IsFailure)
                return aiResult.ToActionResult();

            // 5. Return success
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (IsVendor())
                return Forbid();

            if (IsClient())
                dto.UserId = UserId;

           
                var created = await _eventService.CreateAsync(dto, cancellationToken);
                return created.IsSuccess
                    ? Created()
                    : created.ToActionResult();

        }

        // ─────────────────────────────────────────────────────────
        // PUT api/events/{id}
        // ─────────────────────────────────────────────────────────
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

           
                var existing = await _eventService.GetByIdAsync(id, cancellationToken);

                if (!IsAdminOrOwner(existing.Value.UserId))
                    return Forbid();

                if ((IsVendor() || IsClient()) && dto.EventStatus == "Completed")
                    return Forbid();

                var updated = await _eventService.UpdateAsync(id, dto, cancellationToken);
                if(dto.EventStatus == "Completed")
                {
                    // Send email notification logic here, e.g.:
                    // await _emailService.SendEventCompletedNotificationAsync(existing.UserId, updated.Id);
                }
                return updated.IsSuccess ? Ok(updated) : updated.ToActionResult();
       
        }





        // ─────────────────────────────────────────────────────────
        // POST api/events/{eventId}/items
        // ─────────────────────────────────────────────────────────
        [HttpPost("{eventId:guid}/items")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddItem(
            Guid eventId,
            [FromBody] CreateEventItemDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (IsVendor())
                return Forbid();

                var existing = await _eventService.GetByIdAsync(eventId, cancellationToken);

                if (!IsAdminOrOwner(existing.Value.UserId))
                    return Forbid();

                var result = await _eventService.AddItemAsync(eventId, dto, cancellationToken);
                return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = eventId }, result) : result.ToActionResult();
           
        }

        // ─────────────────────────────────────────────────────────
        // PUT api/events/{eventId}/items/{itemId}
        // ─────────────────────────────────────────────────────────
        [HttpPut("{eventId:guid}/items/{itemId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

           
                var existing = await _eventService.GetByIdAsync(eventId, cancellationToken);

                if (!IsAdminOrOwner(existing.Value.UserId))
                    return Forbid();

                var result = await _eventService.UpdateItemAsync(eventId, itemId, dto, cancellationToken);
                return result.IsSuccess ? Ok(result) : result.ToActionResult();
        
        }
        // ─────────────────────────────────────────────────────────
        // PATCH api/events/{eventId}/items/{itemId}/approve
        // ─────────────────────────────────────────────────────────
        [HttpPatch("{eventId:guid}/items/{itemId:guid}/approve")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveItem(
            Guid eventId,
            Guid itemId,
            [FromBody] ApproveItemRequest request,
            CancellationToken cancellationToken)
        {
            if (!IsVendor())
                return Forbid();

            
                var result = await _eventService.ApproveItemAsync(eventId, itemId, UserId, request.Approve, request.Reason, cancellationToken);
                return result.IsSuccess ? NoContent() : result.ToActionResult();
           
      
        }

        // ─────────────────────────────────────────────────────────
        // PATCH api/events/{id}/cancel
        // ─────────────────────────────────────────────────────────
        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelEvent(Guid id, [FromBody] CancelEventRequest cancelEventRequest, CancellationToken cancellationToken)
        {
           
                var existing = await _eventService.GetByIdAsync(id, cancellationToken);
                if (!IsAdminOrOwner(existing.Value.UserId))
                    return Forbid();

                if (IsVendor())
                    return Forbid();

                if (IsClient() && existing.Value.EventStatus != "Approved")
                    return Forbid();

                if (IsClient() && existing.Value.EventDate.Date <= DateTime.Today.AddDays(7))
                    return BadRequest(new { message = "You cannot cancel an event less than 7 days before it occurs." });

                var result = await _eventService.CancelEventAsync(id, cancelEventRequest, cancellationToken);
                return result.IsSuccess ? NoContent() : result.ToActionResult();
          
        }

        // ─────────────────────────────────────────────────────────
        // DELETE api/events/{id}
        // ─────────────────────────────────────────────────────────
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            if (IsVendor())
                return Forbid();

          
                var existing = await _eventService.GetByIdAsync(id, cancellationToken);

                if (!IsAdminOrOwner(existing.Value.UserId))
                    return Forbid();

                if (IsClient() && existing.Value.EventStatus != "Planned")
                    return BadRequest(new { message = "You can only delete events with 'Planned' status." });

                var result = await _eventService.DeleteAsync(id, cancellationToken);
                return result.IsSuccess ? NoContent() : result.ToActionResult();
          
        }
    }
}