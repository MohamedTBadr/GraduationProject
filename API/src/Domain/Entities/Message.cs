using Domain.Entities;

public class Message
{
    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }        // ← add FK
    public Conversation Conversation { get; private set; }  // ← add nav property
    public Guid SenderId { get; private set; }
    public ApplicationUser Sender { get; private set; }
    public Guid ReceiverId { get; private set; }
    public ApplicationUser Receiver { get; private set; }
    public string Content { get; private set; }
    public DateTime SentAt { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Message() { }

    public static Message Create(Guid conversationId, Guid senderId, Guid receiverId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty.");

        return new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,   // ← set it here
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };
    }

    public void MarkAsRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}