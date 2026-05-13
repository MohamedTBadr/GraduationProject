using Domain.Enums;
using System;

namespace Domain.Entities
{
    public class EventCollaborator
    {
        public Guid Id { get; set; }
        
        public Guid EventId { get; set; }
        public Event Event { get; set; }
        
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        public CollaboratorRole Role { get; set; }
        public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    }
}
