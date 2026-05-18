using Application.Interfaces;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Controllers;

namespace API.Controllers;

[ApiController]
[Route("api/vouchers")]
[Authorize]
public class VoucherController(IServiceManager serviceManager) : APIController
{
    // GET api/vouchers/referral-link
    [HttpGet("referral-link")]
    public async Task<IActionResult> GetReferralLink(CancellationToken ct)
    {
        var userId = GetUserIdFromToken();
        var link = await serviceManager.VoucherService.GetReferralLinkAsync(userId, ct);
        return Ok(new { link });
    }

    // GET api/vouchers/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMyVouchers(CancellationToken ct)
    {
        var userId = GetUserIdFromToken();
        var vouchers = await serviceManager.VoucherService.GetMyVouchersAsync(userId, ct);
        return Ok(vouchers);
    }

    // GET api/vouchers/validate?code=REWARD-A3FK9ZBX
    [HttpGet("validate")]
    public async Task<IActionResult> Validate([FromQuery] string code, CancellationToken ct)
    {
        var userId = GetUserIdFromToken();
        var result = await serviceManager.VoucherService.ValidateVoucherAsync(code, userId, ct);

        if (!result.IsValid)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new
        {
            result.DiscountPercent,
            message = $"{result.DiscountPercent}% discount will be applied."
        });
    }
}