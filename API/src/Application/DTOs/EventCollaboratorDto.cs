using Domain.Enums;
using System;

namespace Application.DTOs
{
    public class EventCollaboratorDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public CollaboratorRole Role { get; set; }
        public DateTime InvitedAt { get; set; }
    }
}
