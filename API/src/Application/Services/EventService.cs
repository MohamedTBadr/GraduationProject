using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Entities;

namespace Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepo;

        private static readonly HashSet<string> ValidStatuses =
            new() { "Planned", "Approved", "Completed", "Cancelled" };

        public EventService(IEventRepository eventRepo)
        {
            _eventRepo = eventRepo;
        }

        // ── Read ──────────────────────────────────────────────────

        public async Task<EventResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _eventRepo.GetByIdWithItemsAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Event with id '{id}' was not found.");

            return entity.ToResponseDto();
        }

        public async Task<IEnumerable<EventSummaryDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            var entities = await _eventRepo.GetAllAsync(cancellationToken);
            return entities.Select(e => e.ToSummaryDto());
        }

        public async Task<IEnumerable<EventSummaryDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var entities = await _eventRepo.GetByUserIdAsync(userId, cancellationToken);
            return entities.Select(e => e.ToSummaryDto());
        }

        public async Task<IEnumerable<EventSummaryDto>> GetByUserIdAndStatusAsync(Guid userId, string status, CancellationToken cancellationToken)
        {
            ValidateStatus(status);
            var entities = await _eventRepo.GetByUserIdAsync(userId, cancellationToken);
            return entities.Where(e => e.EventStatus == status).Select(e => e.ToSummaryDto());
        }

        public async Task<IEnumerable<EventSummaryDto>> GetByStatusAsync(string status, CancellationToken cancellationToken)
        {
            ValidateStatus(status);
            var entities = await _eventRepo.GetByStatusAsync(status, cancellationToken);
            return entities.Select(e => e.ToSummaryDto());
        }

        // ── Write ─────────────────────────────────────────────────

        public async Task<EventResponseDto> CreateAsync(CreateEventDto dto, CancellationToken cancellationToken)
        {
            var created = await _eventRepo.CreateAsync(dto.ToEntity(), cancellationToken);
            return created.ToResponseDto();
        }

        public async Task<EventResponseDto> UpdateAsync(Guid id, UpdateEventDto dto, CancellationToken cancellationToken)
        {
            var entity = await _eventRepo.GetByIdWithItemsAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Event with id '{id}' was not found.");

            ValidateStatus(dto.EventStatus);
            dto.ApplyTo(entity);

            var updated = await _eventRepo.UpdateAsync(entity, cancellationToken);
            return updated.ToResponseDto();
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            if (!await _eventRepo.ExistsAsync(id, cancellationToken))
                throw new KeyNotFoundException($"Event with id '{id}' was not found.");

            return await _eventRepo.DeleteAsync(id, cancellationToken);
        }

        public async Task<bool> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken)
        {
            ValidateStatus(status);

            var entity = await _eventRepo.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Event with id '{id}' was not found.");

            entity.EventStatus = status;
            await _eventRepo.UpdateAsync(entity, cancellationToken);
            return true;
        }

        public async Task CancelEventAsync(Guid id, CancelEventRequest request, CancellationToken cancellationToken)
        {
            var entity = await _eventRepo.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Event with id '{id}' was not found.");

            if (entity.EventStatus == "Cancelled")
                throw new InvalidOperationException("Event is already cancelled.");

            if (entity.EventStatus == "Completed")
                throw new InvalidOperationException("A completed event cannot be cancelled.");

            request.ApplyTo(entity);
            await _eventRepo.UpdateAsync(entity, cancellationToken);
        }

        public async Task ApproveItemAsync(Guid eventId, Guid itemId, Guid vendorId, bool approve, string? reason, CancellationToken cancellationToken)
        {
            var item = await _eventRepo.GetItemByIdAsync(itemId, cancellationToken);

            if (item == null || item.EventId != eventId)
                throw new KeyNotFoundException("Item not found in this event.");

            if (item.VendorId != vendorId)
                throw new UnauthorizedAccessException("You do not own this item.");

            if (item.ItemStatus != "Pending")
                throw new InvalidOperationException($"Item is already '{item.ItemStatus}'.");

            item.ItemStatus = approve ? "Approved" : "Rejected";
            item.RejectionReason = approve ? null : reason;

            await _eventRepo.UpdateItemAsync(item, cancellationToken);
            await SyncEventStatusAsync(eventId,cancellationToken);
        }



        // EventService — add both methods
        public async Task<EventItemResponseDto> AddItemAsync(Guid eventId, CreateEventItemDto dto, CancellationToken cancellationToken)
        {
            var ev = await _eventRepo.GetByIdAsync(eventId, cancellationToken)
                ?? throw new KeyNotFoundException($"Event '{eventId}' not found.");

            if (ev.EventStatus is "Cancelled" or "Completed")
                throw new InvalidOperationException($"Cannot add items to a '{ev.EventStatus}' event.");

            var item = new EventItem
            {
                EventId = eventId,
                ServiceName = dto.ServiceName,
                ServiceImage = dto.ServiceImage,
                Price = dto.Price,
                VendorId = dto.VendorId,
                VendorName = dto.VendorName,
                Quantity = dto.Quantity,
                ItemStatus = "Pending"
            };

            var created = await _eventRepo.AddItemAsync(item, cancellationToken);
            return created.ToResponseDto();
        }

        public async Task<EventItemResponseDto> UpdateItemAsync(Guid eventId, Guid itemId, UpdateEventItemDto dto, CancellationToken cancellationToken)
        {
            var item = await _eventRepo.GetItemByIdAsync(itemId, cancellationToken);

            if (item == null || item.EventId != eventId)
                throw new KeyNotFoundException("Item not found in this event.");

            if (item.ItemStatus is "Approved" or "Rejected")
                throw new InvalidOperationException($"Cannot edit an item that is already '{item.ItemStatus}'.");

            item.ServiceName = dto.ServiceName;
            item.ServiceImage = dto.ServiceImage;
            item.Price = dto.Price;
            item.VendorName = dto.VendorName;
            item.Quantity = dto.Quantity;
            item.ItemStatus = "Pending"; // reset to pending after edit

            var updated = await _eventRepo.UpdateItemAsync(item, cancellationToken);
            return updated.ToResponseDto();
        }
        // ── Helpers ───────────────────────────────────────────────

        private void ValidateStatus(string status)
        {
            if (!ValidStatuses.Contains(status))
                throw new ArgumentException($"Invalid status '{status}'. Valid values: Planned, Approved, Completed, Cancelled.");
        }

        private async Task SyncEventStatusAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var ev = await _eventRepo.GetByIdWithItemsAsync(eventId, cancellationToken);
            if (ev == null) return;

            if (ev.EventItems.All(i => i.ItemStatus == "Approved"))
                ev.EventStatus = "Approved";
            else if (ev.EventItems.Any(i => i.ItemStatus == "Rejected"))
                ev.EventStatus = "Cancelled";

            await _eventRepo.UpdateAsync(ev, cancellationToken);
        }
    }

    // ── Mappers ───────────────────────────────────────────────────

    internal static class EventMappers
    {
        // ── Event → DTOs ──────────────────────────────────────────

        internal static EventResponseDto ToResponseDto(this Event e) => new()
        {
            Id = e.Id,
            UserId = e.UserId,
            Title = e.Title,
            EventTypeName = e.EventType.Name,
            EventDate = e.EventDate,
            TotalBudget = e.TotalBudget,
            GuestCount = e.GuestCount,
            Notes = e.Notes,
            EventStatus = e.EventStatus,
            CancellationReason = e.CancellationReason,
            AdditionalNotes = e.AdditionalNotes,
            CancelledAt = e.CancelledAt,
            Location = e.Location?.ToDto(),
            EventItems = e.EventItems?.Select(i => i.ToResponseDto()).ToList() ?? new()
        };

        internal static EventSummaryDto ToSummaryDto(this Event e) => new()
        {
            Id = e.Id,
            Title = e.Title,
            EventDate = e.EventDate,
            EventStatus = e.EventStatus,
            TotalBudget = e.TotalBudget,
            ItemCount = e.EventItems?.Count ?? 0
        };

        internal static EventItemResponseDto ToResponseDto(this EventItem i) => new()
        {
            Id = i.Id,
            EventId = i.EventId,
            ServiceImage = i.ServiceImage,
            ServiceName = i.ServiceName,
            Price = i.Price,
            VendorId = i.VendorId,
            VendorName = i.VendorName,
            Quantity = i.Quantity,
            ItemStatus = i.ItemStatus,
            RejectionReason = i.RejectionReason
        };

        internal static AddressDto ToDto(this Address a) => new()
        {
            Street = a.Street,
            City = a.City,
            State = a.State
        };

        // ── DTOs → Entity ─────────────────────────────────────────

        internal static Event ToEntity(this CreateEventDto dto) => new()
        {
            UserId = (Guid)dto.UserId!,
            Title = dto.Title,
            EventTypeId = dto.EventTypeId,
            EventDate = dto.EventDate,
            TotalBudget = dto.TotalBudget,
            GuestCount = dto.GuestCount,
            Notes = dto.Notes,
            EventStatus = "Planned",
            Location = dto.Location?.ToEntity()
        };

        internal static void ApplyTo(this UpdateEventDto dto, Event entity)
        {
            entity.Title = dto.Title;
            entity.EventTypeId = dto.EventTypeId;
            entity.EventDate = dto.EventDate;
            entity.TotalBudget = dto.TotalBudget;
            entity.GuestCount = dto.GuestCount;
            entity.Notes = dto.Notes;
            entity.EventStatus = dto.EventStatus;

            if (dto.Location != null)
            {
                entity.Location ??= new Address();
                dto.Location.ApplyTo(entity.Location);
            }
        }

        internal static void ApplyTo(this CancelEventRequest request, Event entity)
        {
            entity.EventStatus = "Cancelled";
            entity.CancellationReason = request.Reason;
            entity.AdditionalNotes = request.AdditionalNotes;
            entity.CancelledAt = DateTime.UtcNow;
        }

        internal static Address ToEntity(this AddressDto dto) => new()
        {
            Street = dto.Street,
            City = dto.City,
            State = dto.State
        };

        internal static void ApplyTo(this AddressDto dto, Address address)
        {
            address.Street = dto.Street;
            address.City = dto.City;
            address.State = dto.State;
        }
    }
}