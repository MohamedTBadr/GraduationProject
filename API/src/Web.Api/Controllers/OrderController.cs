using Application.DTOs.Orders;
using Application.Interfaces;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Controllers;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController(IServiceManager serviceManager) : BaseController
    {
        // POST api/orders
        [HttpPost]
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
        [Authorize]
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
            var orders = await serviceManager.OrderService.GetOrdersByUserIdAsync(userId, ct);
            return Ok(orders);
        }

        // PATCH api/orders/{id}/payment-status
        [HttpPatch("{id:guid}/payment-status")]
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
        [Authorize]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        {
            try
            {
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