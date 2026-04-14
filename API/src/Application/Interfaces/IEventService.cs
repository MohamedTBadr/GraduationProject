using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using BLL.DTOs;

namespace Application.Interfaces
{
    public interface IEventService
    {
        Task<EventResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<EventSummaryDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<EventSummaryDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<EventSummaryDto>> GetByStatusAsync(string status, CancellationToken cancellationToken);
        Task<IEnumerable<EventSummaryDto>> GetByUserIdAndStatusAsync(Guid userId, string status, CancellationToken cancellationToken);
        Task<EventResponseDto> CreateAsync(CreateEventDto dto, CancellationToken cancellationToken);
        Task<EventResponseDto> UpdateAsync(Guid id, UpdateEventDto dto, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken);
        Task CancelEventAsync(Guid id, CancelEventRequest request, CancellationToken cancellationToken);
        Task ApproveItemAsync(Guid eventId, Guid itemId, Guid vendorId, bool approve, string? reason, CancellationToken cancellationToken   );

        // IEventService — add
        Task<EventItemResponseDto> AddItemAsync(Guid eventId, CreateEventItemDto dto, CancellationToken cancellationToken);
        Task<EventItemResponseDto> UpdateItemAsync(Guid eventId, Guid itemId, UpdateEventItemDto dto, CancellationToken cancellationToken);

    }
}