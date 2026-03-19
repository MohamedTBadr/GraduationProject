using Application.DTOs.Orders;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Controllers;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController(IOrderService orderService) : BaseController
    {
        // POST api/orders
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            request = request with { UserId = userId };
            var order = await orderService.CreateOrderAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        // GET api/orders
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var orders = await orderService.GetAllOrdersAsync(ct);
            return Ok(orders);
        }

        // GET api/orders/{id}
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            try
            {
                var order = await orderService.GetOrderByIdAsync(id, ct);
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
            var orders = await orderService.GetOrdersByUserIdAsync(userId, ct);
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
                var updated = await orderService.UpdatePaymentStatusAsync(id, request, ct);
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
                var updated = await orderService.SetPaymentIntentAsync(id, paymentIntentId, ct);
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
                await orderService.CancelOrderAsync(id, ct);
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
                await orderService.DeleteOrderAsync(id, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}