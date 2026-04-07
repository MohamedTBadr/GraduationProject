using Application.DTOs.PaymobDTOs;
using Application.Services;
using Infrastructure.Payments;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymobService _paymob;

        public PaymentsController(PaymobService paymob)
        {
            _paymob = paymob;
        }

        [HttpPost("paymob")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request, CancellationToken cancellationToken)
        {
            var iframeUrl = await _paymob.CreatePaymentAsync(
                request.Amount,
                request.Billing, cancellationToken);
            return Ok(new { iframeUrl });
        }

        [HttpPost("paymob/webhook")]
        public async Task<IActionResult> Webhook([FromQuery] PaymobWebhookPayload payload)
        {
            var raw = Request.Body; // optionally read as string to log raw payload
            await _paymob.HandleWebhookAsync(payload);
            return Ok();
        }
    }

}
