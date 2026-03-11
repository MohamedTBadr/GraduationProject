namespace Application.DTOs.MessageDTOs
{
    public record ConversationDto(
        Guid Id,
        Guid OtherUserId,
        string OtherUserName,
        MessageDto? LastMessage,
        int UnreadCount
    );
}
