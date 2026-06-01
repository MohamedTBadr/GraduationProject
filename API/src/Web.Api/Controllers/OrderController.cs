using Application.DTOs.Orders;
using Application.Interfaces;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Controllers;
using IdempotentAPI.Filters;
using Web.Api.Attributes;
using Web.Api.Controllers.Attributes;


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController(IServiceManager serviceManager) : APIController
    {
        // POST api/orders
        [HttpPost]
        [Idempotent]
        [Authorize(Roles = "Customer")]
        [InvalidateCache("orders", "dashboard-stats")]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            request = request with { UserId = userId };
            var order = await serviceManager.OrderService.CreateOrderAsync(request, ct);
            return order.IsSuccess ? Ok(order) : order.ToActionResult();
        }

        // GET api/orders
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [HybridCache(300, "orders", Variance = CacheVariance.Adaptive)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var orders = await serviceManager.OrderService.GetAllOrdersAsync(ct);
            return orders.IsSuccess ? Ok(orders.Value) : orders.ToActionResult();
        }

        // GET api/orders/{id}
        [HttpGet("{id:guid}")]
        [Authorize]
        [HybridCache(300, "orders", "orders/{id}", Variance = CacheVariance.Adaptive)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var order = await serviceManager.OrderService.GetOrderByIdAsync(id, ct);
            if (!order.IsSuccess)
                return order.ToActionResult();

            if (!IsAdminOrOwner(order.Value.UserId))
                return Forbid();

            return Ok(order.Value);
        }

        // GET api/orders/user/{userId}
        [HttpGet("user/{userId:guid}")]
        [Authorize]
        [HybridCache(300, "orders", "orders/user/{userId}", Variance = CacheVariance.Adaptive)]
        public async Task<IActionResult> GetByUser(Guid userId, CancellationToken ct)
        {
            if (!IsAdminOrOwner(userId))
                return Forbid();

            var orders = await serviceManager.OrderService.GetOrdersByUserIdAsync(userId, ct);
            return orders.IsSuccess ? Ok(orders.Value) : orders.ToActionResult();
        }

        // PATCH api/orders/{id}/payment-status
        [HttpPatch("{id:guid}/payment-status")]
        [Idempotent]
        [Authorize(Roles = "Admin")]
        [InvalidateCache("orders", "orders/{id}", "dashboard-stats")]
        public async Task<IActionResult> UpdatePaymentStatus(
            Guid id,
            [FromBody] UpdateOrderStatusRequest request,
            CancellationToken ct)
        {
            try
            {
                var updated = await serviceManager.OrderService.UpdatePaymentStatusAsync(id, request, ct);
                return updated.IsSuccess ? Ok(updated.Value) : updated.ToActionResult();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // PATCH api/orders/{id}/payment-intent
        [HttpPatch("{id:guid}/payment-intent")]
        [Idempotent]
        [Authorize]
        [InvalidateCache("orders", "orders/{id}", "dashboard-stats")]
        public async Task<IActionResult> SetPaymentIntent(
            Guid id,
            [FromQuery] string paymentIntentId,
            CancellationToken ct)
        {

                var updated = await serviceManager.OrderService.SetPaymentIntentAsync(id, paymentIntentId, ct);
                return updated.IsSuccess ? Ok(updated.Value) : updated.ToActionResult();
            
        }

        // POST api/orders/{id}/cancel
        [HttpPost("{id:guid}/cancel")]
        [Idempotent]
        [Authorize]
        [InvalidateCache("orders", "orders/{id}", "dashboard-stats")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        {

                var order = await serviceManager.OrderService.GetOrderByIdAsync(id, ct);
                if (!IsAdminOrOwner(order.Value.UserId))
                    return Forbid();

                // Prevent clients from unilaterally cancelling Paid or Completed orders
                if (!IsAdmin() && order.Value.PaymentStatus is "Paid" or "Completed")
                    return BadRequest(new { message = "Cannot cancel an order that has already been paid or completed." });

               var result = await serviceManager.OrderService.CancelOrderAsync(id, ct);
                return result.IsSuccess ? NoContent() : result.ToActionResult();
            
        }

        // DELETE api/orders/{id}
        [HttpDelete("{id:guid}")]
        [Idempotent]
        [Authorize(Roles = "Admin")]
        [InvalidateCache("orders", "orders/{id}", "dashboard-stats")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
           
                var result = await serviceManager.OrderService.DeleteOrderAsync(id, ct);
                return result.IsSuccess ? NoContent() : result.ToActionResult();
            
        }
    }
}