using Application.DTOs.EventTypesDTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Attributes;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventTypesController(IServiceManager serviceManager) : ControllerBase
    {
        [HttpGet]
        [HybridCache(1800, "EventTypes", PerRole = true)]
        
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await serviceManager.EventTypeService.GetAllAsync(ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        // Ensure the tag here matches the invalidation pattern exactly
        [HybridCache(1800, "EventType/{id}", PerRole = true)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await serviceManager.EventTypeService.GetByIdAsync(id, ct);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        // Invalidates the list since a new item was added
        [InvalidateCache("EventTypes")]
        public async Task<IActionResult> Create(EventTypeCreateDto dto, CancellationToken ct)
        {
            var result = await serviceManager.EventTypeService.CreateAsync(dto, ct);
            //return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            return Created();
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        // Invalidates both the specific item and the list to maintain consistency
        [InvalidateCache("EventTypes", "EventType/{id}")]
        public async Task<IActionResult> Update(Guid id, EventTypeUpdateDto dto, CancellationToken ct)
        {
            // Ensure the ID in the URL matches the DTO or override it
            if (id != dto.Id) return BadRequest("ID mismatch");

            await serviceManager.EventTypeService.UpdateAsync(dto, ct);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        // Invalidates the specific item tag and the collection tag
        [InvalidateCache("EventTypes", "EventType/{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var success = await serviceManager.EventTypeService.DeleteAsync(id, ct);
            return success.IsSuccess ? NoContent() : success.ToActionResult();
        }
    }
}