using Application.DTOs.PaymobDTOs;
using Application.Interfaces.Services;
using Application.Services;
using Infrastructure.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
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

            // 3. Use order's real amount, not request.Amount
            var iframeUrl = await _paymob.CreatePaymentAsync(
                order.Id,
                order.Amount,   
                request.Billing,
                cancellationToken);

            return Ok(iframeUrl);
        }
        [HttpPost("paymob/webhook")]
        public async Task<IActionResult> Webhook(
     [FromBody] PaymobWebhookPayload payload,
     [FromQuery] string hmac,           // ← Paymob sends this as query param
     CancellationToken cancellationToken)
        {
            if (!_paymob.ValidateHmac(payload, hmac))
                return Unauthorized();

            await _paymob.HandleWebhookAsync(payload, cancellationToken);
            return Ok();
        }
    }

}
