using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.EventTypesDTOs
{
    public record EventTypeCreateDto(string Name);
    public record EventTypeUpdateDto(Guid Id, string Name);
    public record EventTypeResponseDto(Guid Id, string Name );
}
