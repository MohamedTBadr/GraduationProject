using Application.DTOs.Support;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdempotentAPI.Filters;
using Web.Api.Controllers;

namespace API.Controllers
{
    [ApiController]
    public class SupportTicketsController(IServiceManager serviceManager) : APIController
    {
        // ─── ADMIN ENDPOINTS ─────────────────────────────────────────────────────────

        [HttpGet("api/admin/support/tickets/stats")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TicketStatsDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        {
            var result = await serviceManager.SupportTicketService.GetStatsAsync(cancellationToken);
            return result.ToActionResult();
        }

        [HttpGet("api/admin/support/tickets")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PagedResult<TicketSummaryDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? priority,
            [FromQuery] string? type,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            CancellationToken cancellationToken = default)
        {
            var query = new TicketQueryDTO
            {
                Status = status,
                Priority = priority,
                Type = type,
                Page = page,
                Limit = limit
            };

            var result = await serviceManager.SupportTicketService.GetAllAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        [HttpGet("api/admin/support/tickets/{ticketId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TicketDetailsDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] string ticketId,
            CancellationToken cancellationToken)
        {
            var result = await serviceManager.SupportTicketService.GetByIdAsync(ticketId, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost("api/admin/support/tickets")]
        [Idempotent]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TicketDetailsDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateTicketRequestDTO request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = GetUsername();
            var result = await serviceManager.SupportTicketService.CreateAsync(request, username, cancellationToken);

            if (!result.IsSuccess)
                return result.ToActionResult();

            return CreatedAtAction(nameof(GetById), new { ticketId = result.Value.TicketId }, result.Value);
        }

        [HttpPost("api/admin/support/tickets/{ticketId}/reply")]
        [Idempotent]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TicketReplyResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Reply(
            [FromRoute] string ticketId,
            [FromBody] TicketReplyRequestDTO request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = GetUsername();
            var result = await serviceManager.SupportTicketService.ReplyAsync(ticketId, request, cancellationToken, username);
            return result.ToActionResult();
        }

        [HttpPost("api/admin/support/tickets/{ticketId}/assign")]
        [Idempotent]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TicketAssignResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Assign(
            [FromRoute] string ticketId,
            [FromBody] TicketAssignRequestDTO request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await serviceManager.SupportTicketService.AssignAsync(ticketId, request, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPatch("api/admin/support/tickets/{ticketId}/resolve")]
        [Idempotent]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TicketResolveResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Resolve(
            [FromRoute] string ticketId,
            [FromBody] TicketResolveRequestDTO request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = GetUsername();
            var result = await serviceManager.SupportTicketService.ResolveAsync(ticketId, request, cancellationToken, username);
            return result.ToActionResult();
        }

        // ─── VENDOR / CUSTOMER ENDPOINTS ─────────────────────────────────────────────

        [HttpPost("api/support/tickets")]
        [Idempotent]
        [Authorize(Roles = "Vendor,Customer")]
        [ProducesResponseType(typeof(TicketDetailsDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> OpenTicket(
            [FromBody] CreateTicketRequestDTO request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = GetUsername();
            var result = await serviceManager.SupportTicketService.CreateAsync(request, username, cancellationToken);

            if (!result.IsSuccess)
                return result.ToActionResult();

            return CreatedAtAction(nameof(GetById), new { ticketId = result.Value.TicketId }, result.Value);
        }

        [HttpPost("api/support/tickets/{ticketId}/escalate")]
        [Idempotent]
        [Authorize(Roles = "Vendor,Customer")]
        [ProducesResponseType(typeof(TicketEscalateResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Escalate(
            [FromRoute] string ticketId,
            [FromBody] TicketEscalateRequestDTO request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = GetUsername();

            // Ownership check is handled inside the service
            var result = await serviceManager.SupportTicketService.EscalateAsync(ticketId, request, cancellationToken, username);
            return result.ToActionResult();
        }

        // ─── HELPERS ─────────────────────────────────────────────────────────────────

        private string GetUsername() =>
            User.Identity?.Name ?? User.FindFirst("name")?.Value ?? "Unknown User";
    }
}