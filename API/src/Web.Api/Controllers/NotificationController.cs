using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationsController(
    NotificationService notificationService,
    NotificationRepository repo,
    SseConnectionManager sseManager) : ControllerBase
{
    // SSE endpoint — client connects here and keeps it open
    [HttpGet("stream")]
    public async Task Stream(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";

        sseManager.Add(userId, Response);

        try
        {
            // keep connection alive until client disconnects
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (TaskCanceledException) { }
        finally
        {
            sseManager.Remove(userId);
        }
    }

    // get all notifications
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notifications = await repo.GetByUserIdAsync(userId);
        return Ok(notifications);
    }

    // mark as read
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        await repo.MarkAsReadAsync(id);
        return NoContent();
    }
}