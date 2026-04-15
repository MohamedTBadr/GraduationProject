using Domain.Contracts;
using Domain.Entities;
using Google.GenAI.Types;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Repositories;

public class NotificationRepository(ApplicationDbContext db): INotificationRepository
{
    public async Task AddAsync(Notification notification)
    {
        await db.Notifications.AddAsync(notification);
        await db.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetByUserIdAsync(Guid userId) =>
        await db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        await db.Notifications
            .Where(n => n.Id == notificationId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
    public async Task AddRangeAsync(IEnumerable<Notification> notifications)
    {
        await db.Notifications.AddRangeAsync(notifications);
        await db.SaveChangesAsync();
    }
}