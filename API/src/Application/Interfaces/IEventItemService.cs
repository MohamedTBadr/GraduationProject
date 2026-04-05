using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IEventItemService
    {
        Task<EventItemResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<EventItemResponseDto>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken);
        Task<EventItemResponseDto> CreateAsync(CreateEventItemDto dto, CancellationToken cancellationToken);
        Task<IEnumerable<EventItemResponseDto>> CreateRangeAsync(Guid eventId, IEnumerable<CreateEventItemDto> dtos, CancellationToken cancellationToken);
        Task<EventItemResponseDto> UpdateAsync(Guid id, UpdateEventItemDto dto, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> DeleteByEventIdAsync(Guid eventId);
    }
}
