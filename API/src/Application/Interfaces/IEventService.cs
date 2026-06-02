using Application.DTOs;
using BLL.DTOs;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Application.Interfaces
{
    public interface IEventService
    {
        // Read
        Task<Result<EventResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<Result<IEnumerable<EventSummaryDto>>> GetAllAsync(CancellationToken cancellationToken);

        Task<Result<IEnumerable<EventResponseDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        Task<Result<IEnumerable<EventResponseDto>>> GetByUserIdAndStatusAsync(
            Guid userId,
            string status,
            CancellationToken cancellationToken);

        Task<Result<IEnumerable<EventResponseDto>>> GetByStatusAsync(
            string status,
            CancellationToken cancellationToken);

        // Write
        Task<Result<EventResponseDto>> CreateAsync(
            CreateEventDto dto,
            CancellationToken cancellationToken);

        Task<Result<EventResponseDto>> UpdateAsync(
            Guid id,
            UpdateEventDto dto,
            CancellationToken cancellationToken);

        Task<Result<bool>> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken);

        Task<Result<bool>> UpdateStatusAsync(
            Guid id,
            string status,
            CancellationToken cancellationToken);

        Task<Result<bool>> CancelEventAsync(
            Guid id,
            CancelEventRequest request,
            CancellationToken cancellationToken);

        // Items
        Task<Result<EventItemResponseDto>> ApproveItemAsync(
            Guid eventId,
            Guid itemId,
            Guid vendorId,
            bool approve,
            string? reason,
            CancellationToken cancellationToken);

        Task<Result<EventItemResponseDto>> AddItemAsync(
            Guid eventId,
            CreateEventItemDto dto,
            CancellationToken cancellationToken);

        Task<Result<EventItemResponseDto>> UpdateItemAsync(
            Guid eventId,
            Guid itemId,
            UpdateEventItemDto dto,
            CancellationToken cancellationToken);

        // Collaborators
        Task<Result<bool>> AddCollaboratorAsync(
            Guid eventId,
            string userEmailOrName,
            CollaboratorRole role,
            CancellationToken cancellationToken);

        Task<Result<bool>> RemoveCollaboratorAsync(
            Guid eventId,
            Guid userId,
            CancellationToken cancellationToken);

        Task<Result<IEnumerable<EventCollaboratorDto>>> GetCollaboratorsAsync(
            Guid eventId,
            CancellationToken cancellationToken);

        Task<bool> HasPermissionAsync(
            Guid eventId,
            Guid userId,
            bool requiresEdit,
            CancellationToken cancellationToken);
    }
}

