using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs;

namespace Application.Interfaces
{
    public interface IEventItemService
    {
        Task<EventItemResponseDto> GetByIdAsync(Guid id);
        Task<IEnumerable<EventItemResponseDto>> GetByEventIdAsync(Guid eventId);
        Task<EventItemResponseDto> CreateAsync(CreateEventItemDto dto);
        Task<IEnumerable<EventItemResponseDto>> CreateRangeAsync(Guid eventId, IEnumerable<CreateEventItemDto> dtos);
        Task<EventItemResponseDto> UpdateAsync(Guid id, UpdateEventItemDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeleteByEventIdAsync(Guid eventId);
    }
}
