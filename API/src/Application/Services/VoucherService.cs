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
    public async Task<Result<bool>> ApplyReferralAsync(string referralCode, Guid newUserId, CancellationToken ct)
    {
        // 1. Find the referrer by their code
        var referrer = await userRepo.GetByReferralCodeAsync(referralCode, ct);

        // 2. Silently ignore if code invalid or self-referral
        if (referrer is null || referrer.Id == newUserId)
            return Result<bool>.BusinessRule(422, "Invalid referral code or self-referral.");

        // 3. Guard: don't reward twice for the same referral
        var alreadyRewarded = await voucherRepo
            .ExistsForReferrerAsync(referrer.Id, referralCode, ct);

        if (alreadyRewarded) return Result<bool>.BusinessRule(422, "Referral already applied.");

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
        return Result<bool>.Success(true);
    }

    // Returns the shareable referral link for the user
    public async Task<Result<string>> GetReferralLinkAsync(Guid userId, CancellationToken ct)
    {
        var user = await userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");
        if(user.ReferralCode is null)
        {
            // Generate a unique referral code (e.g., using a GUID or a hash)
            user.ReferralCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            await userRepo.SaveChangesAsync(ct);
        }        
        return Result<string>.Success(user.ReferralCode);
    }

    // Returns all vouchers for a user
    public async Task<Result<IEnumerable<VoucherDto>>> GetMyVouchersAsync(Guid userId, CancellationToken ct)
    {
        var vouchers = await voucherRepo.GetByOwnerIdAsync(userId, ct);
        var dtos = vouchers.Select(v => new VoucherDto(
                v.Id,
                v.Code,
                v.DiscountPercent,
                v.IsUsed,
                v.ExpiresAt
        ));
        return Result<IEnumerable<VoucherDto>>.Success(dtos);
    }

    // Validates voucher at checkout — returns result instead of throwing
    public async Task<Result<ApplyVoucherResult>> ValidateVoucherAsync(string code, Guid userId, CancellationToken ct)
    {
        var voucher = await voucherRepo.GetByCodeAsync(code, ct);

        if (voucher is null)
            return Result<ApplyVoucherResult>.NotFound(404, "Voucher not found.");

        if (voucher.OwnerId != userId)
            return Result<ApplyVoucherResult>.BusinessRule(422, "Voucher does not belong to you.");

        if (voucher.IsUsed)
            return Result<ApplyVoucherResult>.BusinessRule(422, "Voucher has already been used.");

        if (voucher.ExpiresAt < DateTime.UtcNow)
            return Result<ApplyVoucherResult>.BusinessRule(422, "Voucher has expired.");

        return Result<ApplyVoucherResult>.Success(new ApplyVoucherResult(true, voucher.DiscountPercent, null));
    }

    // Called by OrderService after order is confirmed
    public async Task<Result<bool>> MarkVoucherUsedAsync(string code, CancellationToken ct)
    {
        var voucher = await voucherRepo.GetByCodeAsync(code, ct);
        if (voucher is null)
            return Result<bool>.NotFound(404, "Voucher not found.");

        voucher.IsUsed = true;
        await voucherRepo.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
    

    public async Task<Result<bool>> MarkVoucherUnusedAsync(string code, CancellationToken ct)
    {
        var voucher = await voucherRepo.GetByCodeAsync(code, ct);
        if (voucher is not null)
        {
            voucher.IsUsed = false;
            await voucherRepo.SaveChangesAsync(ct);
            return Result<bool>.Success(true);
        }
        return Result<bool>.NotFound(404, "Voucher not found.");
    }
}