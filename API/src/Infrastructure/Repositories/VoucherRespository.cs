using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class VoucherRepository(ApplicationDbContext db) : IVoucherRepository
{
    public async Task<Voucher?> GetByCodeAsync(string code, CancellationToken ct) =>
        await db.Vouchers
                .Include(v => v.Owner)
                .FirstOrDefaultAsync(v => v.Code == code, ct);

    public async Task<IEnumerable<Voucher>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct) =>
        await db.Vouchers
                .Where(v => v.OwnerId == ownerId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync(ct);

    // Prevents rewarding User A twice if User B somehow registers again
    public async Task<bool> ExistsForReferrerAsync(Guid ownerId, string referralCode, CancellationToken ct) =>
        await db.Vouchers
                .AnyAsync(v => v.OwnerId == ownerId &&
                               v.Code == $"REWARD-{referralCode}", ct);

    public async Task AddAsync(Voucher voucher, CancellationToken ct) =>
        await db.Vouchers.AddAsync(voucher, ct);

    public async Task SaveChangesAsync(CancellationToken ct) =>
        await db.SaveChangesAsync(ct);
}