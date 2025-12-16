using BLL.DTOs.PaymobDTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace PAL.Controllers
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
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            var iframeUrl = await _paymob.CreatePaymentAsync(
                request.Amount,
                request.Billing);

            return Ok(new { iframeUrl });
        }

        [HttpPost("paymob/webhook")]
        public async Task<IActionResult> Webhook([FromBody] dynamic payload)
        {
            await _paymob.HandleWebhookAsync(payload);
            return Ok();
        }
    }

}
