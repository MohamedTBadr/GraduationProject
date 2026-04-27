using Application.DTOs.Support;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/admin/support/tickets")]
    [Authorize(Roles = "Admin")]
    public class SupportTicketsController(ISupportTicketService ticketService) : ControllerBase
    {
        // ─── GET STATS ───────────────────────────────────────────────────────────────

        [HttpGet("stats")]
        [ProducesResponseType(typeof(TicketStatsDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        {
            var result = await ticketService.GetStatsAsync(cancellationToken);
            return result.ToActionResult();
        }

        // ─── LIST ALL ────────────────────────────────────────────────────────────────

        [HttpGet]
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
                Status   = status,
                Priority = priority,
                Type     = type,
                Page     = page,
                Limit    = limit
            };

            var result = await ticketService.GetAllAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        // ─── GET BY ID ───────────────────────────────────────────────────────────────

        [HttpGet("{ticketId}")]
        [ProducesResponseType(typeof(TicketDetailsDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] string ticketId,
            CancellationToken cancellationToken)
        {
            var result = await ticketService.GetByIdAsync(ticketId, cancellationToken);
            return result.ToActionResult();
        }

        // ─── REPLY ───────────────────────────────────────────────────────────────────

        [HttpPost("{ticketId}/reply")]
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

            var result = await ticketService.ReplyAsync(ticketId, request, cancellationToken);
            return result.ToActionResult();
        }

        // ─── ASSIGN ──────────────────────────────────────────────────────────────────

        [HttpPost("{ticketId}/assign")]
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

            var result = await ticketService.AssignAsync(ticketId, request, cancellationToken);
            return result.ToActionResult();
        }

        // ─── RESOLVE ─────────────────────────────────────────────────────────────────

        [HttpPatch("{ticketId}/resolve")]
        [ProducesResponseType(typeof(TicketResolveResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Resolve(
            [FromRoute] string ticketId,
            [FromBody] TicketResolveRequestDTO request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await ticketService.ResolveAsync(ticketId, request, cancellationToken);
            return result.ToActionResult();
        }

        // ─── ESCALATE ────────────────────────────────────────────────────────────────

        [HttpPost("{ticketId}/escalate")]
        [ProducesResponseType(typeof(TicketEscalateResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Escalate(
            [FromRoute] string ticketId,
            [FromBody] TicketEscalateRequestDTO request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await ticketService.EscalateAsync(ticketId, request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
