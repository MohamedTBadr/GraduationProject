using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.Services.Interfaces
{
    public interface IEventService
    {
        Task<EventResponseDto> GetByIdAsync(Guid id);
        Task<IEnumerable<EventSummaryDto>> GetAllAsync();
        Task<IEnumerable<EventSummaryDto>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<EventSummaryDto>> GetByStatusAsync(string status);
        Task<IEnumerable<EventSummaryDto>> GetByUserIdAndStatusAsync(Guid userId, string status);
        Task<EventResponseDto> CreateAsync(CreateEventDto dto);
        Task<EventResponseDto> UpdateAsync(Guid id, UpdateEventDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> UpdateStatusAsync(Guid id, string status);
    }
}