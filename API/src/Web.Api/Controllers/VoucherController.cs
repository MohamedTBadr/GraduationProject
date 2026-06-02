using Application.Interfaces;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Attributes;
using Web.Api.Controllers;

namespace API.Controllers;

[ApiController]
[Route("api/vouchers")]
[Authorize]
public class VoucherController(IServiceManager serviceManager) : APIController
{
    // GET api/vouchers/referral-link
    [HttpGet("referral-link")]
    [HybridCache(300, "vouchers", "vouchers/referral-link", Variance = CacheVariance.PerUser)]
    public async Task<IActionResult> GetReferralLink(CancellationToken ct)
    {
        var userId = GetUserIdFromToken();
        var link = await serviceManager.VoucherService.GetReferralLinkAsync(userId, ct);
        return Ok(link.Value);
    }

    // GET api/vouchers/my
    [HttpGet("my")]
    [HybridCache(300, "vouchers", "vouchers/my", Variance = CacheVariance.Adaptive)]
    public async Task<IActionResult> GetMyVouchers(CancellationToken ct)
    {
        var userId = GetUserIdFromToken();
        var vouchers = await serviceManager.VoucherService.GetMyVouchersAsync(userId, ct);
        return Ok(vouchers);
    }

    // GET api/vouchers/validate?code=REWARD-A3FK9ZBX
    [HttpGet("validate")]
    [HybridCache(300, "vouchers", "vouchers/validate", Variance = CacheVariance.Adaptive)]
    public async Task<IActionResult> Validate([FromQuery] string code, CancellationToken ct)
    {
        var userId = GetUserIdFromToken();
        var result = await serviceManager.VoucherService.ValidateVoucherAsync(code, userId, ct);

        if (!result.Value.IsValid)
            return BadRequest(new { message = result.Value.ErrorMessage });

        return Ok(new
        {
            result.Value.DiscountPercent,
            message = $"{result.Value.DiscountPercent}% discount will be applied."
        });
    }
}
