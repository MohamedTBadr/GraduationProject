using Application.DTOs.Vouchers;
using Application.Interfaces.Services;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class VoucherService(
    IVoucherRepository voucherRepo,
    IUserRepository userRepo,
    IConfiguration config) : IVoucherService
{
    // Called by AuthService after registration
    public async Task ApplyReferralAsync(string referralCode, Guid newUserId, CancellationToken ct)
    {
        // 1. Find the referrer by their code
        var referrer = await userRepo.GetByReferralCodeAsync(referralCode, ct);

        // 2. Silently ignore if code invalid or self-referral
        if (referrer is null || referrer.Id == newUserId)
            return;

        // 3. Guard: don't reward twice for the same referral
        var alreadyRewarded = await voucherRepo
            .ExistsForReferrerAsync(referrer.Id, referralCode, ct);

        if (alreadyRewarded) return;

        // 4. Create the 5% voucher for the referrer
        var voucher = new Voucher
        {
            OwnerId = referrer.Id,
            Code = $"REWARD-{referralCode}",
            DiscountPercent = 5,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        await voucherRepo.AddAsync(voucher, ct);
        await voucherRepo.SaveChangesAsync(ct);
    }

    // Returns the shareable referral link for the user
    public async Task<string> GetReferralLinkAsync(Guid userId, CancellationToken ct)
    {
        var user = await userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        var baseUrl = config["App:BaseUrl"];
        return $"{baseUrl}/register?ref={user.ReferralCode}";
    }

    // Returns all vouchers for a user
    public async Task<IEnumerable<VoucherDto>> GetMyVouchersAsync(Guid userId, CancellationToken ct)
    {
        var vouchers = await voucherRepo.GetByOwnerIdAsync(userId, ct);
        return vouchers.Select(v => new VoucherDto(
            v.Id,
            v.Code,
            v.DiscountPercent,
            v.IsUsed,
            v.ExpiresAt));
    }

    // Validates voucher at checkout — returns result instead of throwing
    public async Task<ApplyVoucherResult> ValidateVoucherAsync(string code, Guid userId, CancellationToken ct)
    {
        var voucher = await voucherRepo.GetByCodeAsync(code, ct);

        if (voucher is null)
            return new ApplyVoucherResult(false, 0, "Voucher not found.");

        if (voucher.OwnerId != userId)
            return new ApplyVoucherResult(false, 0, "Voucher does not belong to you.");

        if (voucher.IsUsed)
            return new ApplyVoucherResult(false, 0, "Voucher has already been used.");

        if (voucher.ExpiresAt < DateTime.UtcNow)
            return new ApplyVoucherResult(false, 0, "Voucher has expired.");

        return new ApplyVoucherResult(true, voucher.DiscountPercent, null);
    }

    // Called by OrderService after order is confirmed
    public async Task MarkVoucherUsedAsync(string code, CancellationToken ct)
    {
        var voucher = await voucherRepo.GetByCodeAsync(code, ct)
            ?? throw new KeyNotFoundException("Voucher not found.");

        voucher.IsUsed = true;
        await voucherRepo.SaveChangesAsync(ct);
    }
}