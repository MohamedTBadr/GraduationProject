using Domain.Entities;
using Domain.Enums;
using EpicHub.IntegrationTests.Infrastructure.Shared;
using Infrastructure.Repositories;
using Xunit;

namespace EpicHub.IntegrationTests.Infrastructure.Tests;

public class EventRepositoryTests
{
    // Seeds the minimum entities required for an Event row.
    private static async Task<(ApplicationUser owner, ApplicationUser collaborator, Guid eventTypeId)>
        SeedPrerequisitesAsync(TestDatabase db)
    {
        var owner        = EntityBuilders.BuildUser();
        var collaborator = EntityBuilders.BuildUser();
        var eventType    = EntityBuilders.BuildEventType();
        db.Context.Users.AddRange(owner, collaborator);
        db.Context.EventTypes.Add(eventType);
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();
        return (owner, collaborator, eventType.Id);
    }

    // ── GetByUserId ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserIdAsync_ReturnsEventsWhereUserIsOwner()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, _, eventTypeId) = await SeedPrerequisitesAsync(db);
        db.Context.Events.Add(EntityBuilders.BuildEvent(owner.Id, eventTypeId));
        await db.Context.SaveChangesAsync();
        var repo = new EventRepository(db.Context);

        var results = (await repo.GetByUserIdAsync(owner.Id, CancellationToken.None)).ToList();

        Assert.Single(results);
        Assert.Equal(owner.Id, results[0].UserId);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsEmpty_WhenUserHasNoEvents()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, _, _) = await SeedPrerequisitesAsync(db);
        var repo           = new EventRepository(db.Context);

        var results = await repo.GetByUserIdAsync(owner.Id, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetByUserIdAsync_IncludesEventsWhereUserIsCollaborator()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, collaborator, eventTypeId) = await SeedPrerequisitesAsync(db);
        var ev = EntityBuilders.BuildEvent(owner.Id, eventTypeId);
        db.Context.Events.Add(ev);
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();
        var repo = new EventRepository(db.Context);

        await repo.AddCollaboratorAsync(new EventCollaborator
        {
            Id      = Guid.NewGuid(),
            EventId = ev.Id,
            UserId  = collaborator.Id,
            Role    = CollaboratorRole.Viewer
        }, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var results = (await repo.GetByUserIdAsync(collaborator.Id, CancellationToken.None)).ToList();

        Assert.Single(results);
    }

    // ── Collaborators ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AddCollaboratorAsync_PersistsCollaboratorWithCorrectRole()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, collaborator, eventTypeId) = await SeedPrerequisitesAsync(db);
        var ev = EntityBuilders.BuildEvent(owner.Id, eventTypeId);
        db.Context.Events.Add(ev);
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();
        var repo = new EventRepository(db.Context);

        await repo.AddCollaboratorAsync(new EventCollaborator
        {
            Id      = Guid.NewGuid(),
            EventId = ev.Id,
            UserId  = collaborator.Id,
            Role    = CollaboratorRole.Editor
        }, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var collaborators = (await repo.GetCollaboratorsAsync(ev.Id, CancellationToken.None)).ToList();
        Assert.Single(collaborators);
        Assert.Equal(collaborator.Id,      collaborators[0].UserId);
        Assert.Equal(CollaboratorRole.Editor, collaborators[0].Role);
    }

    [Fact]
    public async Task GetCollaboratorsAsync_ReturnsEmpty_WhenNoCollaboratorsAdded()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, _, eventTypeId) = await SeedPrerequisitesAsync(db);
        var ev = EntityBuilders.BuildEvent(owner.Id, eventTypeId);
        db.Context.Events.Add(ev);
        await db.Context.SaveChangesAsync();
        var repo = new EventRepository(db.Context);

        var collaborators = await repo.GetCollaboratorsAsync(ev.Id, CancellationToken.None);

        Assert.Empty(collaborators);
    }

    [Fact]
    public async Task RemoveCollaboratorAsync_RemovesOnlyTargetedCollaborator()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, collaborator, eventTypeId) = await SeedPrerequisitesAsync(db);
        var ev = EntityBuilders.BuildEvent(owner.Id, eventTypeId);
        db.Context.Events.Add(ev);
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();
        var repo = new EventRepository(db.Context);

        await repo.AddCollaboratorAsync(new EventCollaborator
        {
            Id      = Guid.NewGuid(),
            EventId = ev.Id,
            UserId  = collaborator.Id,
            Role    = CollaboratorRole.Viewer
        }, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        await repo.RemoveCollaboratorAsync(ev.Id, collaborator.Id, CancellationToken.None);

        Assert.Empty(await repo.GetCollaboratorsAsync(ev.Id, CancellationToken.None));
    }

    // ── GetByStatus ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByStatusAsync_ReturnsOnlyMatchingEvents()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, _, eventTypeId) = await SeedPrerequisitesAsync(db);
        db.Context.Events.Add(EntityBuilders.BuildEvent(owner.Id, eventTypeId, "Planned One",   EventStatuses.Planned));
        db.Context.Events.Add(EntityBuilders.BuildEvent(owner.Id, eventTypeId, "Planned Two",   EventStatuses.Planned));
        db.Context.Events.Add(EntityBuilders.BuildEvent(owner.Id, eventTypeId, "Cancelled One", EventStatuses.Cancelled));
        await db.Context.SaveChangesAsync();
        var repo = new EventRepository(db.Context);

        var planned = (await repo.GetByStatusAsync(EventStatuses.Planned, CancellationToken.None)).ToList();

        Assert.Equal(2, planned.Count);
        Assert.All(planned, e => Assert.Equal(EventStatuses.Planned, e.EventStatus));
    }

    // ── CRUD ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenEventDoesNotExist()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new EventRepository(db.Context);

        var result = await repo.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_PersistsTitleChange()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, _, eventTypeId) = await SeedPrerequisitesAsync(db);
        var ev = EntityBuilders.BuildEvent(owner.Id, eventTypeId, "Original Title");
        db.Context.Events.Add(ev);
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();
        var repo = new EventRepository(db.Context);

        var loaded = await repo.GetByIdAsync(ev.Id, CancellationToken.None);
        loaded!.Title = "Updated Title";
        await repo.UpdateAsync(loaded, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        Assert.Equal("Updated Title", (await repo.GetByIdAsync(ev.Id, CancellationToken.None))!.Title);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEvent_AndReturnsFalseOnExistsCheck()
    {
        await using var db = await TestDatabase.CreateAsync();
        var (owner, _, eventTypeId) = await SeedPrerequisitesAsync(db);
        var ev = EntityBuilders.BuildEvent(owner.Id, eventTypeId);
        db.Context.Events.Add(ev);
        await db.Context.SaveChangesAsync();
        var repo = new EventRepository(db.Context);

        var deleted = await repo.DeleteAsync(ev.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.False(await repo.ExistsAsync(ev.Id, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenEventDoesNotExist()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new EventRepository(db.Context);

        var deleted = await repo.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(deleted);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenEventDoesNotExist()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new EventRepository(db.Context);

        Assert.False(await repo.ExistsAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
