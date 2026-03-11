using Application.Interfaces;
using Google.GenAI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

//[Authorize]
public class ChatHub(IChatService chatService) : Hub
{
    private Guid CurrentUserId =>
        Guid.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public override async Task OnConnectedAsync()
    {
        // notify everyone this user is online
        await Clients.Others.SendAsync("UserPresence", new
        {
            UserId = CurrentUserId,
            IsOnline = true
        });
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.Others.SendAsync("UserPresence", new
        {
            UserId = CurrentUserId,
            IsOnline = false
        });
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid receiverId, string content)
    {
        var message = await chatService
            .SendMessageAsync(CurrentUserId, receiverId, content);

        // confirm to sender
        await Clients.Caller.SendAsync("ReceiveMessage", message);

        // push to receiver instantly
        await Clients.User(receiverId.ToString())
            .SendAsync("ReceiveMessage", message);
    }

    public async Task MarkAsRead(Guid messageId)
    {
        await chatService.MarkAsReadAsync(messageId, CurrentUserId);

        await Clients.Caller.SendAsync("MessageRead", messageId);
    }

    public async Task Typing(Guid receiverId, bool isTyping)
    {
        await Clients.User(receiverId.ToString())
            .SendAsync("UserTyping", new
            {
                UserId = CurrentUserId,
                IsTyping = isTyping
            });
    }
}
//```

//---

//## One Rule to Remember
//```
//If the other user needs to know instantly  →  Hub (WebSocket)
//If it's just loading data for yourself     →  Controller (REST)