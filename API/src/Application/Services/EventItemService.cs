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
    public class EventItemService : IEventItemService
    {
        private readonly IEventItemRepository _itemRepo;
        private readonly IEventRepository     _eventRepo;

        public EventItemService(
            IEventItemRepository itemRepo,
            IEventRepository     eventRepo)
        {
            _itemRepo  = itemRepo;
            _eventRepo = eventRepo;
        }

        // ── Read ──────────────────────────────────────────────────

        public async Task<EventItemResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _itemRepo.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                throw new KeyNotFoundException($"EventItem with id '{id}' was not found.");

            return MapToDto(entity);
        }

        public async Task<IEnumerable<EventItemResponseDto>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken)
        {
            if (!await _eventRepo.ExistsAsync(eventId, cancellationToken))
                throw new KeyNotFoundException($"Event with id '{eventId}' was not found.");

            var entities = await _itemRepo.GetByEventIdAsync(eventId, cancellationToken);
            return entities.Select(MapToDto);
        }

        // ── Write ─────────────────────────────────────────────────

        public async Task<EventItemResponseDto> CreateAsync(CreateEventItemDto dto, CancellationToken cancellationToken)
        {
            if (!await _eventRepo.ExistsAsync(dto.EventId, cancellationToken))
                throw new KeyNotFoundException($"Event with id '{dto.EventId}' was not found.");

            var entity = MapFromCreateDto(dto);
            var created = await _itemRepo.CreateAsync(entity, cancellationToken);
            
            return MapToDto(created);
        }

        public async Task<IEnumerable<EventItemResponseDto>> CreateRangeAsync(
            Guid eventId, IEnumerable<CreateEventItemDto> dtos, CancellationToken cancellationToken)
        {
            if (!await _eventRepo.ExistsAsync(eventId, cancellationToken))
                throw new KeyNotFoundException($"Event with id '{eventId}' was not found.");

            var entities = dtos.Select(d => { d.EventId = eventId; return MapFromCreateDto(d); });
            var created  = await _itemRepo.CreateRangeAsync(entities, cancellationToken);
            return created.Select(MapToDto);
        }

        public async Task<EventItemResponseDto> UpdateAsync(Guid id, UpdateEventItemDto dto, CancellationToken cancellationToken)
        {
            var entity = await _itemRepo.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                throw new KeyNotFoundException($"EventItem with id '{id}' was not found.");

       
            entity.Quantity     = dto.Quantity;

            var updated = await _itemRepo.UpdateAsync(entity, cancellationToken);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            if (!await _itemRepo.ExistsAsync(id, cancellationToken))
                throw new KeyNotFoundException($"EventItem with id '{id}' was not found.");

            return await _itemRepo.DeleteAsync(id, cancellationToken);
        }

        public async Task<bool> DeleteByEventIdAsync(Guid eventId, CancellationToken cancellationToken)
        {
            if (!await _eventRepo.ExistsAsync(eventId, cancellationToken))
                throw new KeyNotFoundException($"Event with id '{eventId}' was not found.");

            return await _itemRepo.DeleteByEventIdAsync(eventId, cancellationToken);
        }

        // ── Mappers ───────────────────────────────────────────────

        private static EventItem MapFromCreateDto(CreateEventItemDto dto) => new()
        {
            EventId      = dto.EventId,
 
            Quantity     = dto.Quantity
        };

        private static EventItemResponseDto MapToDto(EventItem i) => new()
        {
            Id           = i.Id,
            EventId      = i.EventId,
            ServiceImage = i.Service.ServiceImages.FirstOrDefault()?.ToString() ?? string.Empty,
            ServiceName  = i.Service.Name,
            Price        = i.Price,
            Quantity     = i.Quantity
        };
    }
}
