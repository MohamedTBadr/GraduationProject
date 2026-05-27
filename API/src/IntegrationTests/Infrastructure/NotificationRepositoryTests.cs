using Domain.Entities;
using Domain.Enums;
using EpicHub.IntegrationTests.Infrastructure.Shared;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EpicHub.IntegrationTests.Infrastructure.Tests;

public class NotificationRepositoryTests
{
    // ── AddRange ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddRangeAsync_PersistsAllNotifications()
    {
        await using var db = await TestDatabase.CreateAsync();
        var user           = EntityBuilders.BuildUser();
        db.Context.Users.Add(user);
        await db.Context.SaveChangesAsync();
        var repo           = new NotificationRepository(db.Context);

        var notifications = new[]
        {
            EntityBuilders.BuildNotification(user.Id, NotificationType.ORDER_PLACED,    "Order Placed"),
            EntityBuilders.BuildNotification(user.Id, NotificationType.ORDER_COMPLETED, "Order Completed")
        };
        await repo.AddRangeAsync(notifications);

        Assert.Equal(2, await db.Context.Notifications.CountAsync(n => n.UserId == user.Id));
    }

    // ── GetByUserId ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserIdAsync_ReturnsOnlyNotificationsForRequestedUser()
    {
        await using var db    = await TestDatabase.CreateAsync();
        var user              = EntityBuilders.BuildUser();
        var otherUser         = EntityBuilders.BuildUser();
        db.Context.Users.AddRange(user, otherUser);
        await db.Context.SaveChangesAsync();
        var repo              = new NotificationRepository(db.Context);

        await repo.AddRangeAsync(new[]
        {
            EntityBuilders.BuildNotification(user.Id,      NotificationType.ORDER_PLACED,    "Mine"),
            EntityBuilders.BuildNotification(otherUser.Id, NotificationType.ORDER_CANCELLED, "Not Mine")
        });

        var results = (await repo.GetByUserIdAsync(user.Id)).ToList();

        Assert.Single(results);
        Assert.Equal("Mine", results[0].Title);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsNotificationsOrderedByCreatedAtDescending()
    {
        await using var db = await TestDatabase.CreateAsync();
        var user           = EntityBuilders.BuildUser();
        db.Context.Users.Add(user);
        await db.Context.SaveChangesAsync();
        var repo           = new NotificationRepository(db.Context);

        await repo.AddRangeAsync(new[]
        {
            EntityBuilders.BuildNotification(user.Id, NotificationType.ORDER_PLACED,    "Old", daysOld: 1),
            EntityBuilders.BuildNotification(user.Id, NotificationType.ORDER_COMPLETED, "New", daysOld: 0)
        });

        var results = (await repo.GetByUserIdAsync(user.Id)).ToList();

        // Most recent notification should come first.
        Assert.Equal(new[] { "New", "Old" }, results.Select(n => n.Title));
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsEmpty_WhenUserHasNoNotifications()
    {
        await using var db = await TestDatabase.CreateAsync();
        var user           = EntityBuilders.BuildUser();
        db.Context.Users.Add(user);
        await db.Context.SaveChangesAsync();
        var repo           = new NotificationRepository(db.Context);

        var results = await repo.GetByUserIdAsync(user.Id);

        Assert.Empty(results);
    }

    // ── MarkAsRead ────────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkAsReadAsync_SetsIsReadToTrue_ForCorrectNotification()
    {
        await using var db = await TestDatabase.CreateAsync();
        var user           = EntityBuilders.BuildUser();
        db.Context.Users.Add(user);
        await db.Context.SaveChangesAsync();
        var repo           = new NotificationRepository(db.Context);

        var target = EntityBuilders.BuildNotification(user.Id, NotificationType.ORDER_PLACED, "To Mark");
        var other  = EntityBuilders.BuildNotification(user.Id, NotificationType.ORDER_PLACED, "Leave Unread");
        await repo.AddRangeAsync(new[] { target, other });
        db.Context.ChangeTracker.Clear();

        await repo.MarkAsReadAsync(target.Id, user.Id);
        db.Context.ChangeTracker.Clear();

        var markedNotification  = await db.Context.Notifications.FindAsync(target.Id);
        var unmarkedNotification = await db.Context.Notifications.FindAsync(other.Id);
        Assert.True(markedNotification!.IsRead);
        Assert.False(unmarkedNotification!.IsRead);     // only the target was marked
    }

    [Fact]
    public async Task MarkAsReadAsync_DoesNotMarkNotification_WhenUserIdDoesNotMatch()
    {
        await using var db = await TestDatabase.CreateAsync();
        var user           = EntityBuilders.BuildUser();
        db.Context.Users.Add(user);
        await db.Context.SaveChangesAsync();
        var repo           = new NotificationRepository(db.Context);

        var notification = EntityBuilders.BuildNotification(user.Id, NotificationType.ORDER_PLACED, "Test");
        await repo.AddRangeAsync(new[] { notification });
        db.Context.ChangeTracker.Clear();

        // Attempt to mark with a different userId
        await repo.MarkAsReadAsync(notification.Id, Guid.NewGuid());
        db.Context.ChangeTracker.Clear();

        var persisted = await db.Context.Notifications.FindAsync(notification.Id);
        Assert.False(persisted!.IsRead);
    }
}
