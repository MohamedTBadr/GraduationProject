namespace Application.DTOs.MessageDTOs
{
    public record SendMessageRequest(
        Guid ReceiverId,
        string Content
    );
}
