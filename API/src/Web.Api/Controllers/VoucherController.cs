using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Controllers;

namespace API.Controllers;

[ApiController]
[Route("api/vouchers")]
[Authorize]
public class VoucherController(IVoucherService voucherService) : BaseController
{
    // GET api/vouchers/referral-link
    [HttpGet("referral-link")]
    public async Task<IActionResult> GetReferralLink(CancellationToken ct)
    {
        var userId = GetUserIdFromToken();
        var link = await voucherService.GetReferralLinkAsync(userId, ct);
        return Ok(new { link });
    }

    // GET api/vouchers/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMyVouchers(CancellationToken ct)
    {
        var userId = GetUserIdFromToken();
        var vouchers = await voucherService.GetMyVouchersAsync(userId, ct);
        return Ok(vouchers);
    }

    // GET api/vouchers/validate?code=REWARD-A3FK9ZBX
    [HttpGet("validate")]
    public async Task<IActionResult> Validate([FromQuery] string code, CancellationToken ct)
    {
        var userId = GetUserIdFromToken();
        var result = await voucherService.ValidateVoucherAsync(code, userId, ct);

        if (!result.IsValid)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new
        {
            result.DiscountPercent,
            message = $"{result.DiscountPercent}% discount will be applied."
        });
    }
}