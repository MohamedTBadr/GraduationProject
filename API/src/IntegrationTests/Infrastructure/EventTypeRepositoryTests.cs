using EpicHub.IntegrationTests.Infrastructure.Shared;
using Infrastructure.Repositories;
using Xunit;

namespace EpicHub.IntegrationTests.Infrastructure.Tests;

public class EventTypeRepositoryTests
{
    // ── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_PersistsEntityToDatabase()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var repo            = new EventTypeRespository(db.Context);
        var eventType       = EntityBuilders.BuildEventType("Wedding");

        await repo.CreateAsync(eventType, CancellationToken.None);

        var persisted = await db.Context.EventTypes.FindAsync(eventType.Id);
        Assert.NotNull(persisted);
        Assert.Equal("Wedding", persisted.Name);
    }

    // ── Exists ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenEntityExists()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var repo            = new EventTypeRespository(db.Context);
        var eventType       = EntityBuilders.BuildEventType();
        await repo.CreateAsync(eventType, CancellationToken.None);

        var exists = await repo.ExistsAsync(eventType.Id, CancellationToken.None);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenEntityDoesNotExist()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new EventTypeRespository(db.Context);

        var exists = await repo.ExistsAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(exists);
    }

    // ── Read ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectEntity()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var repo            = new EventTypeRespository(db.Context);
        var eventType       = EntityBuilders.BuildEventType("Corporate");
        await repo.CreateAsync(eventType, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var result = await repo.GetByIdAsync(eventType.Id, CancellationToken.None);

        Assert.Equal(eventType.Id, result.Id);
        Assert.Equal("Corporate", result.Name);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PersistsNameChange()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var repo            = new EventTypeRespository(db.Context);
        var eventType       = EntityBuilders.BuildEventType("Wedding");
        await repo.CreateAsync(eventType, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        eventType.Name = "Corporate Wedding";
        await repo.UpdateAsync(eventType, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var updated = await repo.GetByIdAsync(eventType.Id, CancellationToken.None);
        Assert.Equal("Corporate Wedding", updated.Name);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesEntityFromDatabase()
    {
        await using var db  = await TestDatabase.CreateAsync();
        var repo            = new EventTypeRespository(db.Context);
        var eventType       = EntityBuilders.BuildEventType();
        await repo.CreateAsync(eventType, CancellationToken.None);

        await repo.DeleteAsync(eventType, CancellationToken.None);

        Assert.False(await repo.ExistsAsync(eventType.Id, CancellationToken.None));
    }
}
