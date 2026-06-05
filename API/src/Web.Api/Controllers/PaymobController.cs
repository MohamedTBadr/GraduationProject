using Application.DTOs.Orders;
using Application.DTOs.PaymobDTOs;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities;
using IdempotentAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : APIController
    {
        private readonly IPaymobService _paymob;
        private readonly IServiceManager _serviceManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentsController(IPaymobService paymob, IServiceManager serviceManager, UserManager<ApplicationUser> userManager)
        {
            _paymob = paymob;
            _serviceManager = serviceManager;
            _userManager = userManager;
        }

        [HttpPost("paymob")]
        [Authorize]
 
        public async Task<IActionResult> CreatePayment(
        [FromBody] PaymentRequest request,
        CancellationToken cancellationToken)
        {
            // 0. Input Validation: Ensure OrderId is provided
            // 1. Get user ID from token (assuming you have a method for this) then Get the order from the database using the OrderId from the request. Don't trust client-provided amount or user ID.
            // 1. Fetch the real order — don't trust client-provided amount
            var order = await _serviceManager.OrderService.GetOrderByIdAsync(request.OrderId, cancellationToken);
            if(order.Value.EventId is null)
                return NotFound("Order not found.");

            var eventData = await _serviceManager.EventService.GetByIdAsync((Guid)order.Value.EventId, cancellationToken);
            // 2. Security Check: Enforce ownership (IDOR Fix)
            if (!IsAdminOrOwner(order.Value.UserId))
                return Forbid();

            // 3. Business Logic Check: Block payment attempts on already paid orders
            if (order.Value.PaymentStatus is "Paid" or "Completed")
                return BadRequest("This order has already been paid.");

            // 4. Free Order Bypass Logic (100% off voucher or free events)
            if (order.Value.Amount <= 0)
            {
                await _serviceManager.OrderService.UpdatePaymentStatusAsync(order.Value.Id, new UpdateOrderStatusRequest("Paid"), cancellationToken);
                return Ok(new { isFree = true, message = "Order is free. Payment bypassed successfully.", redirectUrl = "/payment/success" });
            }

            var user = await _userManager.FindByIdAsync(order.Value.UserId.ToString());
            if (user == null)
                return BadRequest("User not found for this order.");

            var billingData = new BillingData
            {
                first_name = user.FirstName ?? "NA",
                last_name = user.LastName ?? "NA",
                email = user.Email ?? "NA",
                phone_number = user.PhoneNumber ?? "NA",
                city = eventData.Value?.Location?.City ?? "NA",
                country = "EG",
                street = eventData.Value?.Location?.Street ?? "NA",
                building = "NA",
                floor = "NA",
                apartment = "NA",
                state = eventData.Value?.Location?.State ?? "NA",
                postal_code = "NA"
            };

            // 5. Use order's real amount, not request.Amount
            var iframeUrl = await _paymob.CreatePaymentAsync(
                order.Value.Id,
                order.Value.Amount,   
                billingData,
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
