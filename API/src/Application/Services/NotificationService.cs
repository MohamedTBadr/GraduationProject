using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

public class NotificationService(
    NotificationRepository repo,
    SseConnectionManager sseManager)
{
    public async Task SendAsync(Guid userId, string type, string title, string message)
    {
        // 1. persist to DB
        var notification = new Notification
        {
            UserId = userId,
            Type = Enum.Parse<NotificationType>(type),
            Title = title,
            Message = message
        };
        await repo.AddAsync(notification);

        // 2. push via SSE if user is connected
        if (sseManager.TryGet(userId, out var response))
        {
            var json = JsonSerializer.Serialize(notification);
            await response!.WriteAsync($"data: {json}\n\n");
            await response.Body.FlushAsync();
        }
    }
    public async Task SendBulkAsync(IEnumerable<(Guid UserId, string Type, string Title, string Message)> notifications)
    {
        // 1. Build entities
        var entities = notifications.Select(n => new Notification
        {
            UserId = n.UserId,
            Type = Enum.Parse<NotificationType>(n.Type),
            Title = n.Title,
            Message = n.Message
        }).ToList();

        // 2. Bulk persist to DB
        await repo.AddRangeAsync(entities);

        // 3. Push via SSE to connected users
        foreach (var notification in entities)
        {
            if (sseManager.TryGet(notification.UserId, out var response))
            {
                var json = JsonSerializer.Serialize(notification);
                await response!.WriteAsync($"data: {json}\n\n");
                await response.Body.FlushAsync();
            }
        }
    }
}