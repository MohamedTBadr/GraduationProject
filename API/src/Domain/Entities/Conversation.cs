namespace Domain.Entities
{
    public class Conversation
        {
            public Guid Id { get; private set; }
            public Guid User1Id { get; private set; }
            public ApplicationUser User1 { get; private set; }
            public Guid User2Id { get; private set; }
            public ApplicationUser User2 { get; private set; }
            public DateTime CreatedAt { get; private set; }
            private readonly List<Message> _messages = new();
            public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

            private Conversation() { }

            public static Conversation Create(Guid user1Id, Guid user2Id) => new()
            {
                Id = Guid.NewGuid(),
                User1Id = user1Id,
                User2Id = user2Id,
                CreatedAt = DateTime.UtcNow
            };
        }
    
}
