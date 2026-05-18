using Application.DTOs.Orders;
using Application.Interfaces;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Controllers;
using IdempotentAPI.Filters;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController(IServiceManager serviceManager) : APIController
    {
        // POST api/orders
        [HttpPost]
        [Idempotent]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            request = request with { UserId = userId };
            var order = await serviceManager.OrderService.CreateOrderAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        // GET api/orders
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var orders = await serviceManager.OrderService.GetAllOrdersAsync(ct);
            return Ok(orders);
        }

        // GET api/orders/{id}
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            try
            {
                var order = await serviceManager.OrderService.GetOrderByIdAsync(id, ct);
                if (!IsAdminOrOwner(order.UserId))
                    return Forbid();

                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET api/orders/user/{userId}
        [HttpGet("user/{userId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetByUser(Guid userId, CancellationToken ct)
        {
            if (!IsAdminOrOwner(userId))
                return Forbid();

            var orders = await serviceManager.OrderService.GetOrdersByUserIdAsync(userId, ct);
            return Ok(orders);
        }

        // PATCH api/orders/{id}/payment-status
        [HttpPatch("{id:guid}/payment-status")]
        [Idempotent]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePaymentStatus(
            Guid id,
            [FromBody] UpdateOrderStatusRequest request,
            CancellationToken ct)
        {
            try
            {
                var updated = await serviceManager.OrderService.UpdatePaymentStatusAsync(id, request, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PATCH api/orders/{id}/payment-intent
        [HttpPatch("{id:guid}/payment-intent")]
        [Idempotent]
        [Authorize]
        public async Task<IActionResult> SetPaymentIntent(
            Guid id,
            [FromQuery] string paymentIntentId,
            CancellationToken ct)
        {
            try
            {
                var updated = await serviceManager.OrderService.SetPaymentIntentAsync(id, paymentIntentId, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST api/orders/{id}/cancel
        [HttpPost("{id:guid}/cancel")]
        [Idempotent]
        [Authorize]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        {
            try
            {
                var order = await serviceManager.OrderService.GetOrderByIdAsync(id, ct);
                if (!IsAdminOrOwner(order.UserId))
                    return Forbid();

                // Prevent clients from unilaterally cancelling Paid or Completed orders
                if (!IsAdmin() && order.PaymentStatus is "Paid" or "Completed")
                    return BadRequest(new { message = "Cannot cancel an order that has already been paid or completed." });

                await serviceManager.OrderService.CancelOrderAsync(id, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE api/orders/{id}
        [HttpDelete("{id:guid}")]
        [Idempotent]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                await serviceManager.OrderService.DeleteOrderAsync(id, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}