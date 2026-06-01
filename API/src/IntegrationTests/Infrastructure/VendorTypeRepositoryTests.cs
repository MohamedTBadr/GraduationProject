using EpicHub.IntegrationTests.Infrastructure.Shared;
using Infrastructure.Repositories;
using Xunit;

namespace EpicHub.IntegrationTests.Infrastructure.Tests;

public class VendorTypeRepositoryTests
{
    // ── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddVendorTypeAsync_PersistsEntity()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var repo            = new VendorTypeRepository(db.Context);
        var vendorType      = EntityBuilders.BuildVendorType("Photography");

        await repo.AddVendorTypeAsync(vendorType, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var persisted = await repo.GetVendorTypeByIdAsync(vendorType.Id, CancellationToken.None);
        Assert.NotNull(persisted);
        Assert.Equal("Photography", persisted.Name);
    }

    // ── Read ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetVendorTypeByIdAsync_ReturnsNull_WhenEntityNotFound()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new VendorTypeRepository(db.Context);

        var result = await repo.GetVendorTypeByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetVendorTypesAsync_ReturnsAllEntities()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var repo            = new VendorTypeRepository(db.Context);
        await repo.AddVendorTypeAsync(EntityBuilders.BuildVendorType("Photography"), CancellationToken.None);
        await repo.AddVendorTypeAsync(EntityBuilders.BuildVendorType("Catering"),    CancellationToken.None);

        var all = await repo.GetVendorTypesAsync(CancellationToken.None);

        Assert.Equal(2, all.Count);
        Assert.Contains(all, v => v.Name == "Photography");
        Assert.Contains(all, v => v.Name == "Catering");
    }

    [Fact]
    public async Task GetVendorTypesAsync_ReturnsEmpty_WhenNoEntitiesExist()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new VendorTypeRepository(db.Context);

        var all = await repo.GetVendorTypesAsync(CancellationToken.None);

        Assert.Empty(all);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateVendorTypeAsync_PersistsNameChange()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var repo            = new VendorTypeRepository(db.Context);
        var vendorType      = EntityBuilders.BuildVendorType("Photography");
        await repo.AddVendorTypeAsync(vendorType, CancellationToken.None);

        vendorType.Name = "Event Photography";
        await repo.UpdateVendorTypeAsync(vendorType, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var updated = await repo.GetVendorTypeByIdAsync(vendorType.Id, CancellationToken.None);
        Assert.Equal("Event Photography", updated!.Name);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteVendorTypeAsync_RemovesEntity()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var repo            = new VendorTypeRepository(db.Context);
        var vendorType      = EntityBuilders.BuildVendorType();
        await repo.AddVendorTypeAsync(vendorType, CancellationToken.None);

        await repo.DeleteVendorTypeAsync(vendorType.Id, CancellationToken.None);

        Assert.Null(await repo.GetVendorTypeByIdAsync(vendorType.Id, CancellationToken.None));
    }
}
