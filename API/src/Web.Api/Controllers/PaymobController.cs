using Application.DTOs.Orders;
using Application.DTOs.PaymobDTOs;
using Application.Interfaces.Services;
using Infrastructure.Payments;
using IdempotentAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : APIController
    {
        private readonly PaymobService _paymob;
        private readonly IOrderService _orderService;

        public PaymentsController(PaymobService paymob, IOrderService orderService)
        {
            _paymob = paymob;
            _orderService = orderService;
        }

        [HttpPost("paymob")]
        [Authorize]
 
        public async Task<IActionResult> CreatePayment(
        [FromBody] PaymentRequest request,
        CancellationToken cancellationToken)
        {
            // 1. Fetch the real order — don't trust client-provided amount
            var order = await _orderService.GetOrderByIdAsync(request.OrderId, cancellationToken);

            // 2. Security Check: Enforce ownership (IDOR Fix)
            if (!IsAdminOrOwner(order.Value.UserId))
                return Forbid();

            // 3. Business Logic Check: Block payment attempts on already paid orders
            if (order.Value.PaymentStatus is "Paid" or "Completed")
                return BadRequest("This order has already been paid.");

            // 4. Free Order Bypass Logic (100% off voucher or free events)
            if (order.Value.Amount <= 0)
            {
                await _orderService.UpdatePaymentStatusAsync(order.Value.Id, new UpdateOrderStatusRequest("Paid"), cancellationToken);
                return Ok(new { isFree = true, message = "Order is free. Payment bypassed successfully.", redirectUrl = "/payment/success" });
            }

            // 5. Use order's real amount, not request.Amount
            var iframeUrl = await _paymob.CreatePaymentAsync(
                order.Value.Id,
                order.Value.Amount,   
                request.Billing,
                cancellationToken);

            return Ok(iframeUrl);
        }
        [HttpPost("paymob/webhook")]
        public IActionResult Webhook(
     [FromBody] PaymobWebhookPayload payload,
     [FromQuery] string hmac)           // ← Paymob sends this as query param
        {
            if (!_paymob.ValidateHmac(payload, hmac))
                return Unauthorized();

            // Offload to background job so we can return 200 OK to Paymob immediately
            Hangfire.BackgroundJob.Enqueue<PaymobService>(s => s.HandleWebhookAsync(payload, CancellationToken.None));
            
            return Ok();
        }
    }

}
