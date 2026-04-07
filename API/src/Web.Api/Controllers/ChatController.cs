using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : BaseController
{
    private readonly IChatService _chatService;
    public ChatController(IChatService chatService) => _chatService = chatService;

    private Guid CurrentUserId => GetUserIdFromToken();

    // ✅ KEEP — load inbox once on page open
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(CancellationToken cancellationToken)
    {
        var result = await _chatService.GetConversationsAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    // ✅ KEEP — load history when opening a conversation
    [HttpGet("messages/{otherUserId}")]
    public async Task<IActionResult> GetMessages(
        Guid otherUserId,
         CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100
       )  
    {
        var messages = await _chatService
            .GetMessagesAsync(CurrentUserId, otherUserId, page, pageSize, cancellationToken);
        return Ok(messages);
    }

    // ❌ DELETED — SendMessage  (moved to Hub)
    // ❌ DELETED — MarkAsRead   (moved to Hub)
}