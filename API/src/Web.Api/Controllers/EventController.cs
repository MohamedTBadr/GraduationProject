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
        GeminiService _geminiService,
        IServiceService _ServiceService) : BaseController
    {
        protected Guid UserId => GetUserIdFromToken();

        // ─────────────────────────────────────────────────────────
        // GET api/events
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = IsAdmin()
                ? await _eventService.GetAllAsync()
                : await _eventService.GetByUserIdAsync(UserId); // ← was calling GetUserIdFromToken() directly, use property instead

            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────
        // GET api/events/{id}
        // ─────────────────────────────────────────────────────────
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _eventService.GetByIdAsync(id);

                if (!IsAdminOrOwner(result.UserId))
                    return Forbid();

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────
        // GET api/events/user/{userId}
        // ─────────────────────────────────────────────────────────
        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            if (!IsAdminOrOwner(userId))
                return Forbid();

            var result = await _eventService.GetByUserIdAsync(userId);
            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────
        // GET api/events/status/{status}
        // ─────────────────────────────────────────────────────────
        [HttpGet("status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByStatus(string status)
        {
            try
            {
                if (IsAdmin())
                {
                    var all = await _eventService.GetByStatusAsync(status);
                    return Ok(all);
                }

                var result = await _eventService.GetByUserIdAndStatusAsync(UserId, status);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────
        // POST api/events/createEventByAI/{eventId}
        // ─────────────────────────────────────────────────────────
        [HttpPost("createEventByAI/{eventId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateEventItemsByAI(Guid eventId)
        {
            try
            {
                var eventObject = await _eventService.GetByIdAsync(eventId);

                var request = new AIRequest
                {
                    Budget = eventObject.TotalBudget,
                    GuestCount = eventObject.GuestCount,
                    CategoryName = eventObject.CategoryName
                };

                var ServiceLines = await _ServiceService.AIFilterAsync(request);

                var prompt = $@"
You are an event planning API that returns ONLY valid JSON. Do not include markdown, explanation, or extra text.

Plan a full event using the details and available Services below.

EVENT DETAILS:
- Title: {eventObject.Title}
- Category: {eventObject.CategoryName}
- Date: {eventObject.EventDate:yyyy-MM-dd}
- Location: {eventObject.Location?.City}, {eventObject.Location?.State}  
- Guest Count: {eventObject.GuestCount}
- Total Budget: {eventObject.TotalBudget:C}
- Notes: {eventObject.Notes ?? "None"}

AVAILABLE SERVICES (within budget):
{string.Join("\n", ServiceLines)}

Return a JSON object with this exact schema:
{{
  ""event_title"": string,
  ""event_type"": string,
  ""guest_count"": number,
  ""total_budget"": number,
  ""plan_summary"": string,
  ""selected_items"": [
    {{
      ""ServiceId"": ""Guid"",
      ""Service_name"": string,
      ""category"": string,
      ""vendor"": string,
      ""price"": number,
      ""reason"": string
    }}
  ],
  ""total_cost"": number,
  ""remaining_budget"": number,
  ""tips"": [string]
}}

Only return JSON. No markdown. No explanation.
";
                var aiResponse = await _geminiService.SendMessageAsync(prompt);

                return Ok(new
                {
                    eventId = eventObject.Id,
                    eventTitle = eventObject.Title,
                    budget = eventObject.TotalBudget,
                    category = eventObject.CategoryName,
                    servicesConsidered = ServiceLines.Value.Count, // ← was capital S (inconsistent casing)
                    aiPlan = aiResponse
                });
            }
            catch (KeyNotFoundException ex) // ← was missing; GetByIdAsync throws, not returns null
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────
        // POST api/events
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (IsVendor())
                return Forbid();

            if (IsClient())
                dto.UserId = UserId;

            try
            {
                var created = await _eventService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created); // ← was Created() with no body or location
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────
        // PUT api/events/{id}
        // ─────────────────────────────────────────────────────────
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var existing = await _eventService.GetByIdAsync(id);

                if (!IsAdminOrOwner(existing.UserId))
                    return Forbid();

                if ((IsVendor() || IsClient()) && dto.EventStatus == "Completed")
                    return Forbid();

                var updated = await _eventService.UpdateAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
            [FromBody] ApproveItemRequest request)
        {
            if (!IsVendor())
                return Forbid();

            try
            {
                await _eventService.ApproveItemAsync(eventId, itemId, UserId, request.Approve, request.Reason);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────
        // PATCH api/events/{id}/cancel
        // ─────────────────────────────────────────────────────────
        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelEvent(Guid id, [FromBody] CancelEventRequest cancelEventRequest)
        {
            try
            {
                var existing = await _eventService.GetByIdAsync(id);

                if (!IsAdminOrOwner(existing.UserId))
                    return Forbid();

                if (IsVendor())
                    return Forbid();

                if (IsClient() && existing.EventStatus != "Approved")
                    return Forbid();

                if (IsClient() && existing.EventDate.Date <= DateTime.Today.AddDays(7))
                    return BadRequest(new { message = "You cannot cancel an event less than 7 days before it occurs." });

                await _eventService.CancelEventAsync(id, cancelEventRequest);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────
        // DELETE api/events/{id}
        // ─────────────────────────────────────────────────────────
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (IsVendor())
                return Forbid();

            try
            {
                var existing = await _eventService.GetByIdAsync(id);

                if (!IsAdminOrOwner(existing.UserId))
                    return Forbid();

                if (IsClient() && existing.EventStatus != "Planned")
                    return BadRequest(new { message = "You can only delete events with 'Planned' status." });

                await _eventService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}