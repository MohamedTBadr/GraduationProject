using EpicHub.IntegrationTests.Infrastructure.Shared;
using Infrastructure.Repositories;
using Xunit;

namespace EpicHub.IntegrationTests.Infrastructure.Tests;

public class ServiceTypeRepositoryTests
{
    // Seeds the required VendorType FK dependency and returns its ID.
    private static async Task<Guid> SeedVendorTypeAsync(TestDatabase db, string name = "Creative")
    {
        var vendorType = EntityBuilders.BuildVendorType(name);
        db.Context.VendorTypes.Add(vendorType);
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();
        return vendorType.Id;
    }

    // ── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddTypeAsync_PersistsEntityWithCorrectVendorTypeId()
    {
        await using var db     = await TestDatabase.CreateAsync();
        var vendorTypeId       = await SeedVendorTypeAsync(db);
        var repo               = new ServiceTypeRepository(db.Context);
        var serviceType        = EntityBuilders.BuildServiceType(vendorTypeId, "Decor");

        await repo.AddTypeAsync(serviceType, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var persisted = await repo.GetServiceTypeByIdAsync(serviceType.Id, CancellationToken.None);
        Assert.Equal("Decor",      persisted.Name);
        Assert.Equal(vendorTypeId, persisted.VendorTypeId);
    }

    // ── Read ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetServiceTypeByIdAsync_ReturnsCorrectEntity()
    {
        await using var db     = await TestDatabase.CreateAsync();
        var vendorTypeId       = await SeedVendorTypeAsync(db);
        var repo               = new ServiceTypeRepository(db.Context);
        var serviceType        = EntityBuilders.BuildServiceType(vendorTypeId, "Catering");
        await repo.AddTypeAsync(serviceType, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var result = await repo.GetServiceTypeByIdAsync(serviceType.Id, CancellationToken.None);

        Assert.Equal(serviceType.Id, result.Id);
        Assert.Equal("Catering",     result.Name);
    }

    [Fact]
    public async Task GetAllServiceTypesAsync_ReturnsAllEntities()
    {
        await using var db   = await TestDatabase.CreateAsync();
        var vendorTypeId     = await SeedVendorTypeAsync(db);
        var repo             = new ServiceTypeRepository(db.Context);
        await repo.AddTypeAsync(EntityBuilders.BuildServiceType(vendorTypeId, "Decor"),     CancellationToken.None);
        await repo.AddTypeAsync(EntityBuilders.BuildServiceType(vendorTypeId, "Lighting"),  CancellationToken.None);

        var all = await repo.GetAllServiceTypesAsync(CancellationToken.None);

        Assert.Equal(2, all.Count);
        Assert.Contains(all, s => s.Name == "Decor");
        Assert.Contains(all, s => s.Name == "Lighting");
    }

    [Fact]
    public async Task GetAllServiceTypesAsync_ReturnsEmpty_WhenNoEntitiesExist()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new ServiceTypeRepository(db.Context);

        var all = await repo.GetAllServiceTypesAsync(CancellationToken.None);

        Assert.Empty(all);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateTypeAsync_PersistsNameChange()
    {
        await using var db   = await TestDatabase.CreateAsync();
        var vendorTypeId     = await SeedVendorTypeAsync(db);
        var repo             = new ServiceTypeRepository(db.Context);
        var serviceType      = EntityBuilders.BuildServiceType(vendorTypeId, "Decor");
        await repo.AddTypeAsync(serviceType, CancellationToken.None);

        serviceType.Name = "Premium Decor";
        await repo.UpdateTypeAsync(serviceType, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var updated = await repo.GetServiceTypeByIdAsync(serviceType.Id, CancellationToken.None);
        Assert.Equal("Premium Decor", updated.Name);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteTypeAsync_RemovesEntity()
    {
        await using var db   = await TestDatabase.CreateAsync();
        var vendorTypeId     = await SeedVendorTypeAsync(db);
        var repo             = new ServiceTypeRepository(db.Context);
        var serviceType      = EntityBuilders.BuildServiceType(vendorTypeId);
        await repo.AddTypeAsync(serviceType, CancellationToken.None);

        await repo.DeleteTypeAsync(serviceType.Id, CancellationToken.None);

        Assert.Empty(await repo.GetAllServiceTypesAsync(CancellationToken.None));
    }
}
