using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.MessageDTOs
{
    public record MessageDto(
        Guid Id,
        Guid SenderId,
        string SenderName,
        Guid ReceiverId,
        string Content,
        DateTime SentAt,
        bool IsRead,
        DateTime? ReadAt
    );
}
