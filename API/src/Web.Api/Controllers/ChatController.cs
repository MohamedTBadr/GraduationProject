// Web.Api/Controllers/ChatController.cs
using Application.DTOs.MessageDTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : BaseController
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService) => _chatService = chatService;

        private Guid CurrentUserId =>
            GetUserIdFromToken();

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var result = await _chatService.GetConversationsAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpGet("messages/{otherUserId}")]
        public async Task<IActionResult> GetMessages(
            Guid otherUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var messages = await _chatService.GetMessagesAsync(CurrentUserId, otherUserId, page, pageSize);
            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var message = await _chatService.SendMessageAsync(
                CurrentUserId, request.ReceiverId, request.Content);
            return Ok(message);
        }

        [HttpPut("read/{messageId}")]
        public async Task<IActionResult> MarkAsRead(Guid messageId)
        {
            await _chatService.MarkAsReadAsync(messageId, CurrentUserId);
            return NoContent();
        }
    }
}