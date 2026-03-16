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
    public class EventController(IEventService _eventService, GeminiService _geminiService , IProductService _productService) : BaseController
    {
      

        // ─────────────────────────────────────────────────────────
        // GET api/events
        //   Admin  → all events
        //   Vendor → only their own events
        //   Client → only their own events
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = IsAdmin()
                ? await _eventService.GetAllAsync()
                : await _eventService.GetByUserIdAsync(GetUserIdFromToken());

            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────
        // GET api/events/{id}
        //   Admin  → any event
        //   Vendor → only events they are linked to as a service owner
        //   Client → only their own
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
        //   Admin  → can query any user's events
        //   Vendor → can only query their own
        //   Client → can only query their own
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
        //   Admin  → all events with that status
        //   Vendor → their own events with that status
        //   Client → their own events with that status
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

                // Non-admins get their own events filtered by status
                var userId = GetUserIdFromToken();
                var result = await _eventService.GetByUserIdAndStatusAsync(userId, status);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("createEventByAI/{eventId}")]
        public async Task<IActionResult> CreateEventItemsByAI(Guid eventId)
        {
            // Step 1: Get event
            var eventObject = await _eventService.GetByIdAsync(eventId);
            if (eventObject is null)
                return NotFound($"Event {eventId} not found.");

            var request = new AIRequest { Budget = eventObject.TotalBudget, GuestCount = eventObject.GuestCount, ServiceTypeName = eventObject.ServiceTypeName };
            var productLines = await _productService.AIFilterAsync(request);


            // Step 5: Build prompt
            var prompt = $@"
You are an event planning API that returns ONLY valid JSON. Do not include markdown, explanation, or extra text.

Plan a full event using the details and available products below.

EVENT DETAILS:
- Title: {eventObject.Title}
- Type: {eventObject.ServiceTypeName}
- Date: {eventObject.EventDate:yyyy-MM-dd}
- Location: {eventObject.Location?.City}, {eventObject.Location?.City}
- Guest Count: {eventObject.GuestCount}
- Total Budget: {eventObject.TotalBudget:C}
- Notes: {eventObject.Notes ?? "None"}

AVAILABLE PRODUCTS (within budget):
{string.Join("\n", productLines)}

Return a JSON object with this exact schema:
{{
  ""event_title"": string,
  ""event_type"": string,
  ""guest_count"": number,
  ""total_budget"": number,
  ""plan_summary"": string,
  ""selected_items"": [
    {{
      ""product_name"": string,
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

            // Step 6: Call Gemini
            var aiResponse = await _geminiService.SendMessageAsync(prompt);

            // Step 7: Return
            return Ok(new
            {
                eventId = eventObject.Id,
                eventTitle = eventObject.Title,
                budget = eventObject.TotalBudget,
                serviceType = eventObject.ServiceTypeName,
                productsConsidered = productLines.Value.Count,
                aiPlan = aiResponse
            });
        }



        // ─────────────────────────────────────────────────────────
        // POST api/events
        //   Admin  → can create for any user (UserId taken from body)
        //   Client → UserId is forced to their own id
        //   Vendor → cannot create events (403)
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
                return Forbid(); // Vendors cannot create events

            if (!IsAdmin())
                dto.UserId = GetUserIdFromToken(); // Clients always own their event

            var created = await _eventService.CreateAsync(dto);
            return Created();
        }

        // ─────────────────────────────────────────────────────────
        // PUT api/events/{id}
        //   Admin  → can update any event, any field
        //   Vendor → can update their own events; cannot set status to Completed
        //   Client → can update their own; cannot set status to Completed
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

                // Vendors and Clients cannot mark an event as Completed — only Admin can
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
        // PATCH api/events/{id}/status
        //   Admin  → any status transition allowed
        //   Client → can only cancel their own event (→ Cancelled)
        //   Vendor → cannot change status (403)
        // ─────────────────────────────────────────────────────────
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
        {
            if (IsVendor())
                return Forbid();

            try
            {
                var existing = await _eventService.GetByIdAsync(id);

                if (!IsAdminOrOwner(existing.UserId))
                    return Forbid();

                // Clients can only cancel — not complete — their events
                if (IsClient() && status != "Cancelled")
                    return Forbid();

                await _eventService.UpdateStatusAsync(id, status);
                return NoContent();
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
        // DELETE api/events/{id}
        //   Admin  → can delete any event regardless of status
        //   Client → can only delete their own events in "Planned" status
        //   Vendor → cannot delete events (403)
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

                // Clients can only delete events still in Planned status
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