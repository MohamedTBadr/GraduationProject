using Application.DTOs.Vouchers;

namespace Application.Interfaces.Services;

public interface IVoucherService
{
    Task<Result<string>> GetReferralLinkAsync(Guid userId, CancellationToken ct);
    Task<Result<bool>> ApplyReferralAsync(string referralCode, Guid newUserId, CancellationToken ct);
    Task<Result<IEnumerable<VoucherDto>>> GetMyVouchersAsync(Guid userId, CancellationToken ct);
    Task<Result<ApplyVoucherResult>> ValidateVoucherAsync(string code, Guid userId, CancellationToken ct);
    Task<Result<bool>> MarkVoucherUsedAsync(string code, CancellationToken ct);
    Task<Result<bool>> MarkVoucherUnusedAsync(string code, CancellationToken ct);
}