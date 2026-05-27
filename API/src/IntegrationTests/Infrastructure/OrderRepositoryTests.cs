using Domain.Entities;
using EpicHub.IntegrationTests.Infrastructure.Shared;
using Infrastructure.Repositories;
using Xunit;

namespace EpicHub.IntegrationTests.Infrastructure.Tests;

public class OrderRepositoryTests
{
    /// <summary>
    /// Seeds all FK dependencies (user, event type, vendor type, service type, vendor,
    /// service, event, and event item) and returns the owner + event for use in tests.
    /// </summary>
    private static async Task<(ApplicationUser owner, Event eventEntity, Service service)>
        SeedOrderPrerequisitesAsync(TestDatabase db)
    {
        var owner       = EntityBuilders.BuildUser();
        var vendorUser  = EntityBuilders.BuildUser();
        var eventType   = EntityBuilders.BuildEventType("Conference");
        var vendorType  = EntityBuilders.BuildVendorType("Venue");
        var serviceType = EntityBuilders.BuildServiceType(vendorType.Id, "Hall");
        var vendor      = EntityBuilders.BuildVendor(vendorUser.Id, vendorType.Id);
        var service     = EntityBuilders.BuildService(vendor.UserId, serviceType.Id, price: 200m);
        var eventEntity = EntityBuilders.BuildEvent(owner.Id, eventType.Id, "Conference");
        var item        = EntityBuilders.BuildEventItem(eventEntity.Id, service.Id, quantity: 3, price: 200m);

        db.Context.Users.AddRange(owner, vendorUser);
        db.Context.EventTypes.Add(eventType);
        db.Context.VendorTypes.Add(vendorType);
        db.Context.ServiceTypes.Add(serviceType);
        db.Context.Vendors.Add(vendor);
        db.Context.Services.Add(service);
        db.Context.Events.Add(eventEntity);
        db.Context.EventItems.Add(item);
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();

        return (owner, eventEntity, service);
    }

    // ── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_PersistsOrder()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, ev, _) = await SeedOrderPrerequisitesAsync(db);
        var repo           = new OrderRepository(db.Context);
        var order          = EntityBuilders.BuildOrder(owner.Id, ev.Id, amount: 600m);

        await repo.AddAsync(order, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        Assert.True(await repo.ExistsAsync(order.Id, CancellationToken.None));
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrderAmountAsync_ReturnsCorrectTotalForEvent()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, ev, _) = await SeedOrderPrerequisitesAsync(db);
        var repo           = new OrderRepository(db.Context);
        await repo.AddAsync(EntityBuilders.BuildOrder(owner.Id, ev.Id, amount: 600m), CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var amount = await repo.GetOrderAmountAsync(ev.Id, CancellationToken.None);

        Assert.Equal(600m, amount);
    }

    [Fact]
    public async Task GetEventWithItemsAsync_ReturnsEventWithPopulatedItems()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, ev, service) = await SeedOrderPrerequisitesAsync(db);
        var repo               = new OrderRepository(db.Context);
        await repo.AddAsync(EntityBuilders.BuildOrder(owner.Id, ev.Id), CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var loaded = await repo.GetEventWithItemsAsync(ev.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Single(loaded.EventItems);
        Assert.Equal(service.Id, loaded.EventItems[0].ServiceId);
    }

    [Fact]
    public async Task GetByPaymentIntentIdAsync_ReturnsMatchingOrder()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, ev, _) = await SeedOrderPrerequisitesAsync(db);
        var repo           = new OrderRepository(db.Context);
        var order          = EntityBuilders.BuildOrder(owner.Id, ev.Id, paymentIntentId: "pi-abc123");
        await repo.AddAsync(order, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var result = await repo.GetByPaymentIntentIdAsync("pi-abc123", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
    }

    [Fact]
    public async Task GetByPaymentIntentIdAsync_ReturnsNull_WhenIntentNotFound()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new OrderRepository(db.Context);

        var result = await repo.GetByPaymentIntentIdAsync("pi-nonexistent", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsOnlyOrdersForThatUser()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, ev, _) = await SeedOrderPrerequisitesAsync(db);
        var repo           = new OrderRepository(db.Context);
        await repo.AddAsync(EntityBuilders.BuildOrder(owner.Id, ev.Id), CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var ownerOrders = await repo.GetByUserIdAsync(owner.Id, CancellationToken.None);
        var otherOrders = await repo.GetByUserIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Single(ownerOrders);
        Assert.Empty(otherOrders);
    }

    [Fact]
    public async Task GetByIdWithItemsAsync_ReturnsNull_WhenOrderDoesNotExist()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new OrderRepository(db.Context);

        var result = await repo.GetByIdWithItemsAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PersistsPaymentStatusChange()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, ev, _) = await SeedOrderPrerequisitesAsync(db);
        var repo           = new OrderRepository(db.Context);
        var order          = EntityBuilders.BuildOrder(owner.Id, ev.Id);
        await repo.AddAsync(order, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var loaded = await repo.GetByIdWithItemsAsync(order.Id, CancellationToken.None);
        loaded!.PaymentStatus = OrderStatuses.Paid;
        await repo.UpdateAsync(loaded, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var updated = await repo.GetByIdAsync(order.Id, CancellationToken.None);
        Assert.Equal(OrderStatuses.Paid, updated!.PaymentStatus);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesOrder()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, ev, _) = await SeedOrderPrerequisitesAsync(db);
        var repo           = new OrderRepository(db.Context);
        var order          = EntityBuilders.BuildOrder(owner.Id, ev.Id);
        await repo.AddAsync(order, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        await repo.DeleteAsync(order.Id, CancellationToken.None);

        Assert.False(await repo.ExistsAsync(order.Id, CancellationToken.None));
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenOrderDoesNotExist()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new OrderRepository(db.Context);

        Assert.False(await repo.ExistsAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
