using Application.DTOs.Vouchers;

namespace Application.Interfaces.Services;

public interface IVoucherService
{
    Task<string> GetReferralLinkAsync(Guid userId, CancellationToken ct);
    Task ApplyReferralAsync(string referralCode, Guid newUserId, CancellationToken ct);
    Task<IEnumerable<VoucherDto>> GetMyVouchersAsync(Guid userId, CancellationToken ct);
    Task<ApplyVoucherResult> ValidateVoucherAsync(string code, Guid userId, CancellationToken ct);
    Task MarkVoucherUsedAsync(string code, CancellationToken ct);
    Task MarkVoucherUnusedAsync(string code, CancellationToken ct);
}