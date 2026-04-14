using Application.DTOs.EventTypesDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEventTypeService
    {
        Task<IEnumerable<EventTypeResponseDto>> GetAllAsync(CancellationToken ct);
        Task<EventTypeResponseDto?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<EventTypeResponseDto> CreateAsync(EventTypeCreateDto dto, CancellationToken ct);
        Task UpdateAsync(EventTypeUpdateDto dto, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    }
}
