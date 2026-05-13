using Domain.Enums;

namespace Application.DTOs
{
    public class AddCollaboratorRequest
    {
        public string UserEmailOrName { get; set; }
        public CollaboratorRole Role { get; set; }
    }
}
