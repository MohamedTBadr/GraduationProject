namespace Application.DTOs.Vouchers;

public record VoucherDto(
    Guid Id,
    string Code,
    decimal DiscountPercent,
    bool IsUsed,
    DateTime ExpiresAt);

public record ApplyVoucherResult(
    bool IsValid,
    decimal DiscountPercent,
    string? ErrorMessage);