using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using BLL.DTOs;

namespace Application.Interfaces
{
    public interface IEventService
    {
        Task<Result<EventResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<IEnumerable<EventSummaryDto>>> GetAllAsync(CancellationToken cancellationToken);
        Task<Result<IEnumerable<EventSummaryDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<Result<IEnumerable<EventSummaryDto>>> GetByStatusAsync(string status, CancellationToken cancellationToken);
        Task<Result<IEnumerable<EventSummaryDto>>> GetByUserIdAndStatusAsync(Guid userId, string status, CancellationToken cancellationToken);
        Task<Result<EventResponseDto>> CreateAsync(CreateEventDto dto, CancellationToken cancellationToken);
        Task<Result<EventResponseDto>> UpdateAsync(Guid id, UpdateEventDto dto, CancellationToken cancellationToken);
        Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<bool>> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken);
        Task<Result<bool>> CancelEventAsync(Guid id, CancelEventRequest request, CancellationToken cancellationToken);
        Task<Result<EventItemResponseDto>> ApproveItemAsync(Guid eventId, Guid itemId, Guid vendorId, bool approve, string? reason, CancellationToken cancellationToken   );

        // IEventService — add
        Task<Result<EventItemResponseDto>> AddItemAsync(Guid eventId, CreateEventItemDto dto, CancellationToken cancellationToken);
        Task<Result<EventItemResponseDto>> UpdateItemAsync(Guid eventId, Guid itemId, UpdateEventItemDto dto, CancellationToken cancellationToken);
    }
}