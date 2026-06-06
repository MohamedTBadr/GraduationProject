using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController(ApplicationDbContext context, IChatService chatService) : APIController
{
    private readonly IChatService _chatService = chatService;
    private readonly ApplicationDbContext _context = context;

    private Guid CurrentUserId => GetUserIdFromToken();

    // ✅ KEEP — load inbox once on page open
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var result = await _chatService.GetConversationsAsync(CurrentUserId);
        return Ok(result);
    }

    // ✅ KEEP — load history when opening a conversation
    [HttpGet("messages/{otherUserId}")]
    public async Task<IActionResult> GetMessages(
        Guid otherUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100
       )  
    {
        var messages = await _chatService
            .GetMessagesAsync(CurrentUserId, otherUserId, page, pageSize);
        return Ok(messages);
    }


    [AllowAnonymous]
    [HttpDelete]
    public async Task<IActionResult> WipeUp()
    {
        await _context.Messages.ExecuteDeleteAsync();
        await _context.Conversations.ExecuteDeleteAsync();
        return Ok("Wiped.");
    }

    // ❌ DELETED — SendMessage  (moved to Hub)
    // ❌ DELETED — MarkAsRead   (moved to Hub)
}