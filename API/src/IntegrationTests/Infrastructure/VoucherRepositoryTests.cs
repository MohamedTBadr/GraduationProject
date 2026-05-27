using Domain.Entities;
using EpicHub.IntegrationTests.Infrastructure.Shared;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EpicHub.IntegrationTests.Infrastructure.Tests;

public class VoucherRepositoryTests
{
    // ── GetByCode ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByCodeAsync_ReturnsVoucherWithOwnerLoaded()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var owner           = EntityBuilders.BuildUserWithReferralCode("owner@test.com", "REF42");
        db.Context.Users.Add(owner);
        db.Context.Vouchers.Add(EntityBuilders.BuildVoucher(owner.Id, "REWARD-REF42"));
        await db.Context.SaveChangesAsync();
        var repo            = new VoucherRepository(db.Context);

        var result = await repo.GetByCodeAsync("REWARD-REF42", CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Owner);                    // navigation property loaded
        Assert.Equal(owner.Email, result.Owner.Email);
    }

    [Fact]
    public async Task GetByCodeAsync_ReturnsNull_WhenCodeDoesNotExist()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new VoucherRepository(db.Context);

        var result = await repo.GetByCodeAsync("NONEXISTENT", CancellationToken.None);

        Assert.Null(result);
    }

    // ── ExistsForReferrer ────────────────────────────────────────────────────

    [Fact]
    public async Task ExistsForReferrerAsync_ReturnsTrue_WhenRewardVoucherAlreadyIssued()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var owner           = EntityBuilders.BuildUserWithReferralCode("owner@test.com", "REF42");
        db.Context.Users.Add(owner);
        db.Context.Vouchers.Add(EntityBuilders.BuildVoucher(owner.Id, "REWARD-REF42"));
        await db.Context.SaveChangesAsync();
        var repo            = new VoucherRepository(db.Context);

        var exists = await repo.ExistsForReferrerAsync(owner.Id, "REF42", CancellationToken.None);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsForReferrerAsync_ReturnsFalse_WhenNoRewardVoucherIssued()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var owner           = EntityBuilders.BuildUserWithReferralCode("owner@test.com", "REF99");
        db.Context.Users.Add(owner);
        await db.Context.SaveChangesAsync();
        var repo            = new VoucherRepository(db.Context);

        var exists = await repo.ExistsForReferrerAsync(owner.Id, "REF99", CancellationToken.None);

        Assert.False(exists);
    }

    // ── GetByOwnerId ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByOwnerIdAsync_ReturnsVouchersOrderedByCreatedAtDescending()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var owner           = EntityBuilders.BuildUser("owner@test.com");
        db.Context.Users.Add(owner);
        // daysOld: 2 = older, 0 = newer
        db.Context.Vouchers.AddRange(
            EntityBuilders.BuildVoucher(owner.Id, "WELCOME",      daysOld: 2),
            EntityBuilders.BuildVoucher(owner.Id, "REWARD-REF42", daysOld: 0));
        await db.Context.SaveChangesAsync();
        var repo            = new VoucherRepository(db.Context);

        var vouchers = (await repo.GetByOwnerIdAsync(owner.Id, CancellationToken.None)).ToList();

        // Newest first
        Assert.Equal(new[] { "REWARD-REF42", "WELCOME" }, vouchers.Select(v => v.Code));
    }

    [Fact]
    public async Task GetByOwnerIdAsync_ReturnsEmpty_WhenOwnerHasNoVouchers()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var owner           = EntityBuilders.BuildUser();
        db.Context.Users.Add(owner);
        await db.Context.SaveChangesAsync();
        var repo            = new VoucherRepository(db.Context);

        var vouchers = await repo.GetByOwnerIdAsync(owner.Id, CancellationToken.None);

        Assert.Empty(vouchers);
    }

    // ── AddAsync / SaveChangesAsync ──────────────────────────────────────────

    [Fact]
    public async Task AddAsync_WithSaveChangesAsync_PersistsVoucher()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var owner           = EntityBuilders.BuildUser("owner@test.com");
        db.Context.Users.Add(owner);
        await db.Context.SaveChangesAsync();
        var repo            = new VoucherRepository(db.Context);

        await repo.AddAsync(new Voucher
        {
            OwnerId         = owner.Id,
            Code            = "SAVE10",
            DiscountPercent = 10,
            ExpiresAt       = DateTime.UtcNow.AddDays(7)
        }, CancellationToken.None);
        await repo.SaveChangesAsync(CancellationToken.None);

        Assert.True(await db.Context.Vouchers.AnyAsync(v => v.Code == "SAVE10"));
    }

    [Fact]
    public async Task AddAsync_WithoutSaveChangesAsync_DoesNotPersistVoucher()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var owner           = EntityBuilders.BuildUser();
        db.Context.Users.Add(owner);
        await db.Context.SaveChangesAsync();
        var repo            = new VoucherRepository(db.Context);

        // Intentionally skip SaveChangesAsync
        await repo.AddAsync(new Voucher
        {
            OwnerId   = owner.Id,
            Code      = "UNSAVED",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        }, CancellationToken.None);

        // Fresh context should not see the unsaved voucher
        Assert.False(await db.Context.Vouchers.AsNoTracking().AnyAsync(v => v.Code == "UNSAVED"));
    }
}
