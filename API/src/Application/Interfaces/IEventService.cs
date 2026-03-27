using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using BLL.DTOs;

namespace Application.Interfaces
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
        Task CancelEventAsync(Guid id, CancelEventRequest request);
        Task ApproveItemAsync(Guid eventId, Guid itemId, Guid vendorId, bool approve, string? reason);

    }
}